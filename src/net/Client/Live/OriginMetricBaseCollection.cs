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
            get { return _monitor ?? (_monitor = new AllOriginMetricsMonitor(Queryable)); }
        }

        /**********************************************************************************************************
        /// <summary>
        /// Get the metrics of a specific origin service
        /// i.e. context.OriginMetrics.GetMetric(id). If you know the metric Id, 
        /// this is more efficient than context.OriginMetrics.Where(m => m.Id = metricId).Single() 
        /// </summary>
        /// <param name="metricId"></param>
        /// <returns></returns>
        public IOriginMetric GetMetric(string metricId)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", OriginMetricSet, metricId), UriKind.Relative);
            var dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var metric = dataContext.Execute<OriginMetricData>(uri).SingleOrDefault();
            return metric;
        }
        ***********************************************************************************************************/
    }
}
