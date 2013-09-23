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
    /// Represents a collection of <see cref="IChannelMetric"/>.
    /// </summary>
    public sealed class ChannelMetricBaseCollection : CloudBaseCollection<IChannelMetric>
    {
        internal const string ChannelMetricSet = "ChannelMetrics";
        private AllChannelMetricsMonitor _monitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMetricBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal ChannelMetricBaseCollection(CloudMediaContext cloudMediaContext)
        {
            DataContextFactory = cloudMediaContext.DataContextFactory;
            Queryable = DataContextFactory.CreateDataServiceContext().CreateQuery<ChannelMetricData>(ChannelMetricSet);
        }

        /// <summary>
        /// Get the metrics monitor for all channel services
        /// </summary>
        public AllChannelMetricsMonitor Monitor
        {
            get { return _monitor ?? (_monitor = new AllChannelMetricsMonitor(Queryable)); }
        }

        /**********************************************************************************************************
        /// <summary>
        /// Get the metrics of a specific channel service
        /// i.e. context.ChannelMetrics.GetMetric(id). 
        /// If you know the metric Id, 
        /// this is more efficient than context.ChannelMetrics.Where(m => m.Id = metricId).Single() 
        /// </summary>
        /// <param name="metricId"></param>
        /// <returns></returns>
        public IChannelMetric GetMetric(string metricId)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", ChannelMetricSet, metricId), UriKind.Relative);
            var dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var metric = dataContext.Execute<ChannelMetricData>(uri).SingleOrDefault();
            return metric;
        }
        ***********************************************************************************************************/
    }
}
