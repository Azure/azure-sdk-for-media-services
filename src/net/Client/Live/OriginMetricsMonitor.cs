//-----------------------------------------------------------------------
// <copyright file="ErrorDetail.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Origin metrics monitor class
    /// </summary>
    public abstract class OriginMetricsMonitor : MetricsMonitor
    {
        /// <summary>
        /// EventHandler for the origin metric received
        /// </summary>
        public EventHandler<OriginMetricsEventArgs> MetricReceived { get; set; }
        
        /// <summary>
        /// Get the list of Origin Metrics 
        /// There is only one element in the list if monitoring a single origin
        /// </summary>
        /// <returns>The list of metrics</returns>
        protected abstract IList<IOriginMetric> GetOriginMetrics();

        protected override void GetMetrics()
        {
            if (MetricReceived == null) return;

            var metrics = GetOriginMetrics();

            var metricReceivedHandlers = MetricReceived;
            if (metricReceivedHandlers != null)
            {
                metricReceivedHandlers.BeginInvoke(
                    this,
                    new OriginMetricsEventArgs
                    {
                        OriginMetrics = metrics
                    },
                    null,
                    null);
            }
        }
    }
}
