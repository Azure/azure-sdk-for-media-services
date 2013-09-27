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

using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Origin Metrics Monitor for all origin services
    /// </summary>
    public sealed class AllOriginMetricsMonitor : OriginMetricsMonitor
    {
        private readonly IQueryable<IOriginMetric> _metricsQueryable;

        /// <summary>
        /// Construct an AllOriginMetricsMonitor object
        /// </summary>
        /// <param name="metricsQueryable"></param>
        internal AllOriginMetricsMonitor(IQueryable<IOriginMetric> metricsQueryable)
        {
            _metricsQueryable = metricsQueryable;
        }

        /// <summary>
        /// Get the list of Origin Metrics 
        /// There is only one element in the list if monitoring a single origin
        /// </summary>
        /// <returns>The list of metrics</returns>
        protected override ReadOnlyCollection<IOriginMetric> GetOriginMetrics()
        {
            return _metricsQueryable.ToList().AsReadOnly();
        }
    }
}
