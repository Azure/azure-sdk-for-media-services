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

using System;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IChannelMetric"/>.
    /// </summary>
    public sealed class MetricBaseCollection<T> : CloudBaseCollection<T>
    {
        internal const string ChannelMetricSet = "ChannelMetrics";
        internal const string OriginMetricSet = "OriginMetrics";

        private readonly IMetricsMonitor<T> _monitor;

        /// <summary>
        /// Initializes a new instance of the MetricBaseCollection class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal MetricBaseCollection(CloudMediaContext cloudMediaContext)
        {
            DataContextFactory = cloudMediaContext.DataContextFactory;

            var type = typeof (T);
            if (type == typeof (IOriginMetric))
            {
                Queryable =
                    DataContextFactory.CreateDataServiceContext().CreateQuery<OriginMetricData>(OriginMetricSet)
                        as IQueryable<T>;
            }
            else if (type == typeof (IChannelMetric))
            {
                Queryable =
                    DataContextFactory.CreateDataServiceContext().CreateQuery<ChannelMetricData>(ChannelMetricSet)
                        as IQueryable<T>;
            }

            _monitor = new MetricsMonitor<T>(Queryable);
        }

        /// <summary>
        /// subscribe or unsubscribe an event handler to the MetricsRecevied event
        /// An timer will be started automatically if there is any subscriber and
        /// will be stopped if there is no subscriber.
        /// The timer interval can be set using SetInterval
        /// </summary>
        public event EventHandler<MetricsEventArgs<T>> MetricsReceived
        {
            add
            {
                _monitor.MetricsReceived += value;
            }
            remove
            {
                // ReSharper disable once DelegateSubtraction
                _monitor.MetricsReceived -= value;
            }
        }

        /// <summary>
        /// Set the metric retrieval timer interval
        /// </summary>
        public void SetInterval(TimeSpan interval)
        {
            _monitor.SetInterval(interval);
        }

        /// <summary>
        /// Return the monitor instances
        /// </summary>
        internal IMetricsMonitor<T> Monitor
        {
            get
            {
                return _monitor;
            }
        }

        /**********************************************************************************************************
        /// <summary>
        /// Get the metrics of a specific channel or origin
        /// i.e. context.ChannelMetrics.GetMetric(id). 
        /// If you know the metric Id, 
        /// this is more efficient than context.ChannelMetrics.Where(m => m.Id = metricId).Single() 
        /// </summary>
        /// <param name="metricId"></param>
        /// <returns></returns>
        public T GetMetric(string metricId)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", ChannelMetricSet, metricId), UriKind.Relative);
            var dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var metric = dataContext.Execute<ChannelMetricData>(uri).SingleOrDefault();
            return metric;
        }
        ***********************************************************************************************************/
    }
}
