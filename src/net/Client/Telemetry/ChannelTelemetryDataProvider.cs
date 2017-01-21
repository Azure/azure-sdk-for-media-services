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

namespace Microsoft.WindowsAzure.MediaServices.Client.Telemetry
{
    /// <summary>
    /// Retrieves telemetry data for a channel.
    /// </summary>
    public class ChannelTelemetryDataProvider
    {
        private TelemetryDataCache _dataCache;
        private TelemetryStorage _storage;

        /// <summary>
        /// Channel Id.
        /// </summary>
        public Guid ChannelId { get; }

        /// <summary>
        /// Initializes an instance of the <see cref="ChannelTelemetryDataProvider"/> class./> 
        /// </summary>
        /// <param name="channelId">Channel Id.</param>
        /// <param name="dataCache">Sas uri cache.</param>
        /// <param name="storage">Telemetry storage provider and parser.</param>
        internal ChannelTelemetryDataProvider(
            Guid channelId,
            TelemetryDataCache dataCache,
            TelemetryStorage storage)
        {
            _dataCache = dataCache;
            _storage = storage;
            ChannelId = channelId;
        }

        /// <summary>
        /// Returns monitoring data for specified time interval.
        /// </summary>
        /// <param name="start">Start time of requested data.</param>
        /// <param name="end">End time of requested data.</param>
        /// <returns>A collection of <see cref="IChannelHeartbeat"/>.</returns>
        public ICollection<IChannelHeartbeat> GetChannelHeartbeats(DateTime start, DateTime end)
        {
            List<MonitoringSasUri> requiredUris = _dataCache.GetRequiredUris(start, end);

            return _storage.GetChannelMetrics(requiredUris, ChannelId, start, end);
        }
    }
}
