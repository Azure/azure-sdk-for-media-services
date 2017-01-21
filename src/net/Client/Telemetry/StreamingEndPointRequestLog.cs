//-----------------------------------------------------------------------
// <copyright file="StreamingEndPointRequestLog.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A Streaming EndPoint request log metric.
    /// </summary>
    internal class StreamingEndPointRequestLog : IStreamingEndpointRequestLog
    {
        /// <summary>
        /// Gets the partition key of the record.
        /// </summary>
        public string PartitionKey { get; private set; }

        /// <summary>
        /// Gets the row key of the record.
        /// </summary>
        public string RowKey { get; private set; }

        /// <summary>
        /// Gets the Media Services account ID.
        /// </summary>
        public Guid AccountId { get; private set; }

        /// <summary>
        /// Gets the Media Services Streaming Endpoint ID.
        /// </summary>
        public Guid StreamingEndpointId { get; private set; }

        /// <summary>
        /// Gets the observed time of the metric.
        /// </summary>
        public DateTime ObservedTime { get; private set; }

        /// <summary>
        /// Gets the Streaming Endpoint host name.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the result code.
        /// </summary>
        public string ResultCode { get; private set; }

        /// <summary>
        /// Gets the request count.
        /// </summary>
        public int RequestCount { get; private set; }

        /// <summary>
        /// Gets the bytes sent.
        /// </summary>
        public long BytesSent { get; private set; }

        /// <summary>
        /// Gets the server latency.
        /// </summary>
        public int ServerLatency { get; private set; }

        /// <summary>
        /// Gets the end to end request time.
        /// </summary>
        public int EndToEndLatency { get; private set; }

        /// <summary>
        /// Initializes a new instance of the StreamingEndpointRequestLog class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="accountId">The Media Services account ID.</param>
        /// <param name="streamingEndpointId">The Streaming Endpoint ID.</param>
        /// <param name="observedTime">The observed time of the metric.</param>
        /// <param name="hostName">The Streaming Endpoint host name.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="resultCode">The result code.</param>
        /// <param name="requestCount">The request count.</param>
        /// <param name="bytesSent">The bytes sent.</param>
        /// <param name="serverLatency">The server latency.</param>
        /// <param name="endToEndLatency">The end to end request time.</param>
        internal StreamingEndPointRequestLog(string partitionKey, string rowKey, Guid accountId, Guid streamingEndpointId, DateTime observedTime, string hostName, int statusCode, string resultCode, int requestCount, long bytesSent, int serverLatency, int endToEndLatency)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            AccountId = accountId;
            StreamingEndpointId = streamingEndpointId;
            ObservedTime = observedTime;
            HostName = hostName;
            StatusCode = statusCode;
            ResultCode = resultCode;
            RequestCount = requestCount;
            BytesSent = bytesSent;
            ServerLatency = serverLatency;
            EndToEndLatency = endToEndLatency;
        }

        /// <summary>
        /// Creates a StreamingEndpointRequestLog object from a Azure Table Storage row.
        /// </summary>
        /// <param name="entity">The Azure Table Storage row.</param>
        /// <returns>The new StreamingEndpointRequestLog object.</returns>
        internal static StreamingEndPointRequestLog FromTableEntity(DynamicTableEntity entity)
        {
            var partitionKeyParts = entity.PartitionKey.Split('_');

            return new StreamingEndPointRequestLog(
                entity.PartitionKey,
                entity.RowKey,
                Guid.ParseExact(partitionKeyParts[0], "N"),
                entity.Properties["ServiceId"].GuidValue.GetValueOrDefault(),
                entity.Properties["ObservedTime"].DateTime.GetValueOrDefault(),
                entity.Properties["HostName"].StringValue,
                entity.Properties["StatusCode"].Int32Value.GetValueOrDefault(),
                entity.Properties["ResultCode"].StringValue,
                entity.Properties["RequestCount"].Int32Value.GetValueOrDefault(),
                entity.Properties["BytesSent"].Int64Value.GetValueOrDefault(),
                entity.Properties["ServerLatency"].Int32Value.GetValueOrDefault(),
                entity.Properties["E2ELatency"].Int32Value.GetValueOrDefault());
        }
    }
}
