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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Channel Metrics Monitor for a signle channel service
    /// </summary>
    public sealed class SingleChannelMetricsMonitor : ChannelMetricsMonitor
    {
        private readonly IChannel _channel;

        /// <summary>
        /// Create a SingleChannelMetricsMonitor object with the channel ID
        /// </summary>
        /// <param name="channel">The channel object hosting the monitor</param>
        internal SingleChannelMetricsMonitor(IChannel channel)
        {
            _channel = channel;
        }

        /// <summary>
        /// Get the list of Channel Metrics 
        /// There is only one element in the list if monitoring a single channel
        /// </summary>
        /// <returns>The list of metrics</returns>
        protected override IList<IChannelMetric> GetChannelMetrics()
        {
            return new List<IChannelMetric> { _channel.GetMetric() };
        }
    }
}
