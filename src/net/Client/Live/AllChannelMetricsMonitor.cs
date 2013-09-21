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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Channel Metrics Monitor for all channel services
    /// </summary>
    public sealed class AllChannelMetricsMonitor : ChannelMetricsMonitor
    {
        private readonly IQueryable<IChannelMetric> _metricsQueryable;

        /// <summary>
        /// Construct an AllChannelMetricsMonitor object
        /// </summary>
        /// <param name="metricsQueryable"></param>
        internal AllChannelMetricsMonitor(IQueryable<IChannelMetric> metricsQueryable)
        {
            _metricsQueryable = metricsQueryable;
        }

        /// <summary>
        /// Get the list of Channel Metrics 
        /// There is only one element in the list if monitoring a single channel
        /// </summary>
        /// <returns>The list of metrics</returns>
        protected override IList<IChannelMetric> GetChannelMetrics()
        {
            return _metricsQueryable.ToList();
        }
    }
}
