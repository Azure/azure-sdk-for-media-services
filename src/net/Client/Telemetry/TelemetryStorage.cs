//-----------------------------------------------------------------------
// <copyright file="TelemetryStorage.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A client for reading metrics written to Azure Table Storage by the Media Services Telemetry service.
    /// </summary>
    internal class TelemetryStorage
    {
        private const int TimeSkewMinutes = 5;
        private const int SecondsPerDay = 86400;
        private const string ChannelMetrics = "ChannelHeartbeat";
        private const string StreamingEndPointMetrics = "StreamingEndpointRequestLog";

        /// <summary>
        /// Gets metrics for a Media Services Channel.
        /// </summary>
        /// <param name="channelId">The Channel ID.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>The Channel metrics for the given channel within the given time range.</returns>
        public ICollection<IChannelHeartbeat> GetChannelMetrics(
            List<MonitoringSasUri> monitoringSasUris,
            Guid channelId,
            DateTime start,
            DateTime end)
        {
            return GetMetrics(monitoringSasUris, channelId, start, end, CreateChannelMetrics);
        }

        /// <summary>
        /// Gets metrics for a Media Services Streaming EndPoint.
        /// </summary>
        /// <param name="streamingEndPointId">The Streaming EndPoint ID.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>The Streaming Endpoint metrics for the given streaming end point within the given time range.</returns>
        public ICollection<IStreamingEndpointRequestLog> GetStreamingEndPointMetrics(
            List<MonitoringSasUri> monitoringSasUris,
            Guid streamingEndPointId,
            DateTime start, 
            DateTime end)
        {
            return GetMetrics(monitoringSasUris, streamingEndPointId, start, end, CreateStreamingEndPointMetrics);
        } 

        private static T GetMetrics<T>(
            List<MonitoringSasUri> monitoringSasUris,
            Guid serviceId,
            DateTime start,
            DateTime end,
            Func<IEnumerable<DynamicTableEntity>, Predicate<DynamicTableEntity>, T>  createFunc)
        {
            // Expand the query range to allow for telemetry batching and latency (the result will later be filtered to the requested
            // range.
            var queryStart = start.AddMinutes(-TimeSkewMinutes);
            var queryEnd = end.AddMinutes(TimeSkewMinutes);

            // Create queries for each day in the query range.
            var queries = CreateQueries(monitoringSasUris, serviceId, queryStart, queryEnd);

            // Drop items outside of the query range.
            Predicate<DynamicTableEntity> filter = entity =>
            {
                var observedTime = entity.Properties["ObservedTime"].DateTime.GetValueOrDefault();
                return observedTime >= start && observedTime <= end;
            };

            return createFunc(queries, filter);
        }

        private static IEnumerable<DynamicTableEntity> CreateQueries(
            List<MonitoringSasUri> monitoringSasUris,
            Guid serviceId,
            DateTime queryStart,
            DateTime queryEnd)
        {
            if (monitoringSasUris == null)
            {
                throw new ArgumentNullException(nameof(monitoringSasUris));
            }

            var accountId = monitoringSasUris.First().AccountId;

            if (accountId == Guid.Empty)
            {
                throw new ArgumentNullException("Account Id is not specified in MonitoringSasUri");
            }

            var partitionKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", accountId.ToString("n"), serviceId.ToString("n"));
            var startMinRowKey = string.Format(CultureInfo.InvariantCulture, "{0:D5}_99999", SecondsPerDay - (int)queryStart.TimeOfDay.TotalSeconds);
            var endMaxRowKey = string.Format(CultureInfo.InvariantCulture, "{0:D5}_00000", SecondsPerDay - (int)queryEnd.TimeOfDay.TotalSeconds);

            return  monitoringSasUris.SelectMany((uris, index) =>
            {
                var startOfRange = uris.MetricDataDate.Date == queryStart.Date;
                var endOfRange = uris.MetricDataDate.Date == queryEnd.Date;

                if (startOfRange && endOfRange)
                {
                    // For dates that contain both the start and end of the query range (i.e. the query does not cross any UTC
                    // day boundaries), give a range of row keys within the partition.

                    Expression<Func<DynamicTableEntity, bool>> filter = x =>
                        x.PartitionKey == partitionKey &&
                        string.Compare(x.RowKey, startMinRowKey, StringComparison.Ordinal) < 0 &&
                        string.Compare(x.RowKey, endMaxRowKey, StringComparison.Ordinal) > 0;

                    return RetrieveData(uris, filter);
                }
                else if (startOfRange)
                {
                    // For dates that contain the start of the query range (i.e. the query starts on this date and continues to the
                    // next day), specify the minimum row key.
                    Expression<Func<DynamicTableEntity, bool>> filter = x =>
                        x.PartitionKey == partitionKey &&
                        string.Compare(x.RowKey, startMinRowKey, StringComparison.Ordinal) < 0;
                    return RetrieveData(uris, filter);
                }
                else if (endOfRange)
                {
                    // For dates that contain the end of the query range (i.e. the query starts on a previous day and continues through
                    // part of this day), specify the maximum row key.
                    Expression<Func<DynamicTableEntity, bool>> filter = x =>
                        x.PartitionKey == partitionKey &&
                        string.Compare(x.RowKey, endMaxRowKey, StringComparison.Ordinal) > 0;
                    return RetrieveData(uris, filter);
                }
                else
                {
                    // For dates where the query does not start or end (i.e. whole days within the query), filter only on the
                    // partition key.
                    Expression<Func<DynamicTableEntity, bool>> filter = x => x.PartitionKey == partitionKey;
                    return RetrieveData(uris, filter);
                }
            });
        }

        private static IEnumerable<DynamicTableEntity> RetrieveData(MonitoringSasUri uri, Expression<Func<DynamicTableEntity, bool>> filter)
        {
            List<DynamicTableEntity> records = null;

            if (uri.SasUriExpiryDate <= DateTime.UtcNow)
            {
                throw new ArgumentException("SAS URL is expired.");
            }

            foreach (var url in uri.SasUris)
            {
                CloudTable table = new CloudTable(new Uri(url));

                records = table
                    .CreateQuery<DynamicTableEntity>()
                    .Where(filter)
                    .SkipTableNotFoundErrors()
                    .ToList();

                if (records.Count() != 0)
                {
                    break;
                }
            }

            return records;
        }

        private static ICollection<IChannelHeartbeat> CreateChannelMetrics(IEnumerable<DynamicTableEntity> items, Predicate<DynamicTableEntity> predicate)
        {
            var channelHeartbeats = new List<IChannelHeartbeat>();
            // Execute each of the queries (this could be executed in parallel if needed).
            foreach (var item in items)
            {
                var itemName = item.Properties["Name"].StringValue;
                if (!predicate(item))
                {
                    continue;
                }
                // Parse the items and them to the result collections.
                switch (itemName)
                {
                    case ChannelMetrics:
                        channelHeartbeats.Add(ChannelHeartbeat.FromTableEntity(item));
                        break;
                }
            }
            return channelHeartbeats;
        }

        private static ICollection<IStreamingEndpointRequestLog> CreateStreamingEndPointMetrics(IEnumerable<DynamicTableEntity> items, Predicate<DynamicTableEntity> predicate)
        {
            var streamingEndPointRequestLogs = new List<IStreamingEndpointRequestLog>();
            foreach (var item in items)
            {

                var itemName = item.Properties["Name"].StringValue;

                if (!predicate(item))
                {
                    continue;
                }
                switch (itemName)
                {
                    case StreamingEndPointMetrics:
                        streamingEndPointRequestLogs.Add(StreamingEndPointRequestLog.FromTableEntity(item));
                        break;
                }
            }
            return streamingEndPointRequestLogs;
        }
    }
}
