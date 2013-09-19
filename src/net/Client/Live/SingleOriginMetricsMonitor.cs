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
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Origin Metrics Monitor for a signle origin service
    /// </summary>
    public sealed class SingleOriginMetricsMonitor : OriginMetricsMonitor
    {
        private readonly IOrigin _origin;

        /// <summary>
        /// Create a SingleOriginMetricsMonitor object with the origin ID
        /// </summary>
        /// <param name="origin">The origin object hosting the monitor</param>
        internal SingleOriginMetricsMonitor(IOrigin origin)
        {
            _origin = origin;
        }

        /// <summary>
        /// Get the list of Origin Metrics 
        /// There is only one element in the list if monitoring a single origin
        /// </summary>
        /// <returns>The list of metrics</returns>
        protected override IList<IOriginMetric> GetOriginMetrics()
        {
            return new List<IOriginMetric> {_origin.GetMetric()};
        }
    }
}
