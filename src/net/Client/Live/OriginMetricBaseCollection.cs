// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IOriginMetric"/>.
    /// </summary>
    public sealed class OriginMetricBaseCollection : CloudBaseCollection<IOriginMetric>
    {
        internal const string OriginMetricSet = "OriginMetrics";
        private AllOriginMetricsMonitor _monitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginMetricBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal OriginMetricBaseCollection(CloudMediaContext cloudMediaContext)
        {
            DataContextFactory = cloudMediaContext.DataContextFactory;
            Queryable = DataContextFactory.CreateDataServiceContext().CreateQuery<OriginMetricData>(OriginMetricSet);
        }

        /// <summary>
        /// Get the metrics monitor for all origin services
        /// </summary>
        public AllOriginMetricsMonitor Monitor
        {
            get
            {
                if (_monitor == null)
                {
                    _monitor = new AllOriginMetricsMonitor(Queryable);
                }

                return _monitor;
            }
        }
    }
}
