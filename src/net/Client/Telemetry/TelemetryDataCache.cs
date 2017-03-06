// Copyright 2014 Microsoft Corporation
// 
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Telemetry
{
    /// <summary>
    /// Caches and retieves telemetry data.
    /// </summary>
    internal class TelemetryDataCache
    {
        private readonly TimeSpan _timeSkew = TimeSpan.FromMinutes(1);
        private DateTime _lastCleanUp;
        private Func<DateTime, DateTime, IEnumerable<MonitoringSasUri>> _getSasUris;

        internal TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
        internal Dictionary<DateTime, MonitoringSasUri> _monitoringSasUriDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryDataCache"/> class./>  
        /// </summary>
        /// <param name="getSasUris">A method to obtain Sas Uris from REST.</param>
        internal TelemetryDataCache(
            Func<DateTime, DateTime, IEnumerable<MonitoringSasUri>> getSasUris)
        {
            _getSasUris = getSasUris;
            _monitoringSasUriDictionary = new Dictionary<DateTime, MonitoringSasUri>();
            _lastCleanUp = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns list of <see cref="MonitoringSasUri"/> for specified interval.
        /// </summary>
        /// <param name="start">Start time in UTC.</param>
        /// <param name="end">End time in UTC.</param>
        /// <returns></returns>
        public List<MonitoringSasUri> GetRequiredUris(DateTime start, DateTime end)
        {
            if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Start and end dates must be in UTC format.");
            }

            if (start > end)
            {
                throw new ArgumentException("Start time must be earlier than end time.");
            }

            List<MonitoringSasUri> requiredUris;

            if (!TryGetRequiredMonitoringUris(start, end, out requiredUris))
            {
                requiredUris = _getSasUris(start, end).ToList();

                foreach (var uri in requiredUris)
                {
                    _monitoringSasUriDictionary[uri.MetricDataDate.Date] = uri;
                }
            }

            return requiredUris;
        }

        private bool TryGetRequiredMonitoringUris(DateTime start, DateTime end, out List<MonitoringSasUri> uris)
        {
            if ((DateTime.UtcNow - _lastCleanUp) > _cleanupInterval)
            {
                RemoveExpiredRecordsFromCache();
                _lastCleanUp = DateTime.UtcNow;
            }

            uris = new List<MonitoringSasUri>();

            for (var i = start.Date; i <= end.Date; i = i.AddDays(1))
            {
                MonitoringSasUri monitoringUri;

                var hasKey = _monitoringSasUriDictionary.TryGetValue(i.Date, out monitoringUri);
                if (!hasKey || monitoringUri.SasUriExpiryDate.Subtract(_timeSkew) <= DateTime.UtcNow)
                {
                    return false;
                }

                uris.Add(monitoringUri);
            }

            return true;
        }

        private void RemoveExpiredRecordsFromCache()
        {
            var keys = _monitoringSasUriDictionary.Keys.ToList();

            foreach (var key in keys)
            {
                if (_monitoringSasUriDictionary[key].SasUriExpiryDate <= DateTime.UtcNow)
                {
                    _monitoringSasUriDictionary.Remove(key);
                }
            }
        }
    }
}
