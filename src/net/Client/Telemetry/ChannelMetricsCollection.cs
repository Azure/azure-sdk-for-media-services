//-----------------------------------------------------------------------
// <copyright file="ChannelMetricsCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of Channel Metrics written to Azure Table Storage 
    /// by the Media Services Telemetry service.
    /// </summary>
    public sealed class ChannelMetricsCollection
    {
        internal ChannelMetricsCollection(MediaContextBase cloudMediaContext)
        {
            MediaContext = cloudMediaContext;
        }

        /// <summary>
        /// Get metrics for a Media Services Channel.
        /// </summary>
        /// <param name="endpointAddress">The Telemetry endpoint address</param>
        /// <param name="storageAccountKey">The Storage account key.</param>
        /// <param name="mediaServicesAccountName">The Media Services account ID.</param>
        /// <param name="channelId">The Channel ID</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>The Channel metrics for the given channel within the given time range.</returns>
        public ICollection<IChannelHeartbeat> GetChannelMetrics(
            string endpointAddress, 
            string storageAccountKey,
            string mediaServicesAccountName,
            string channelId,
            DateTime start,
            DateTime end)
        {
            return AsyncHelper.Wait(GetChannelMetricsAsync(
                endpointAddress,
                storageAccountKey,
                mediaServicesAccountName,
                channelId,
                start,
                end));
        }

        /// <summary>
        /// Get metrics for a Media Services Channel in in asynchronous mode.
        /// </summary>
        /// <param name="endpointAddress">The Telemetry endpoint address</param>
        /// <param name="storageAccountKey">The Storage account key.</param>
        /// <param name="mediaServicesAccountName">The Media Services account ID.</param>
        /// <param name="channelId">The Channel ID</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>The Task of the Channel metrics for the given channel within the given time range.</returns>
        public Task<ICollection<IChannelHeartbeat>> GetChannelMetricsAsync(
            string endpointAddress,
            string storageAccountKey,
            string mediaServicesAccountName,
            string channelId,
            DateTime start,
            DateTime end)
        {
            if (endpointAddress == null)
            {
                throw new ArgumentNullException("endpointAddress");
            }

            if (storageAccountKey == null)
            {
                throw new ArgumentNullException("storageAccountKey");
            }

            if (mediaServicesAccountName == null)
            {
                throw new ArgumentNullException("mediaServicesAccountName");
            }

            Guid mediaServicesAccountId;
            if (!Guid.TryParse(mediaServicesAccountName, out mediaServicesAccountId))
            {
                throw new ArgumentException(StringTable.InvalidMediaServicesAccountNameInput);
            }

            Guid channelGuid;
            if (!Guid.TryParse(channelId, out channelGuid))
            {
                throw new ArgumentException(StringTable.InvalidChannelInput);
            }

            if (start.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(StringTable.NonUtcDateTime, "start");
            }

            if (end.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(StringTable.NonUtcDateTime, "end");
            }

            if (start >= end)
            {
                throw new ArgumentException(StringTable.InvalidTimeRange, "start");
            }

            var storageAccount = TelemetryUtilities.GetStorageAccountName(endpointAddress);
            if (MediaContext.StorageAccounts.Where(c => c.Name == storageAccount).FirstOrDefault() == null)
            {
                throw new ArgumentException(StringTable.InvalidStorageAccount);
            }

            return Task.Factory.StartNew(() =>
            {
                var telemetryStorage = TelemetryStorageFactory.CreateTelemetryStorage(
                    new StorageCredentials(storageAccount, storageAccountKey),
                    new Uri(endpointAddress));

                return telemetryStorage.GetChannelMetrics(
                    mediaServicesAccountId,
                    channelGuid,
                    start,
                    end);
            });
        }

        /// <summary>
        /// Media Context
        /// </summary>
        public MediaContextBase MediaContext { get; set; }
    }
}
