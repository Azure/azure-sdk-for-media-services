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
    /// Represents a collection of metrics.
    /// </summary>
    public class MetricBaseCollection<T> : CloudBaseCollection<T> where T : IMetric
    {
        /// <summary>
        /// Return the monitor instances
        /// </summary>
        internal IMetricsMonitor<T> Monitor { get; private set; }

        /// <summary>
        /// Initialize the MetricBaseCollection object
        /// </summary>
        /// <param name="queryable"></param>
        public void Initialize(IQueryable<T> queryable)
        {
            Queryable = queryable;
            Monitor = new MetricsMonitor<T>(queryable);
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
                Monitor.MetricsReceived += value;
            }
            remove
            {
                // ReSharper disable once DelegateSubtraction
                Monitor.MetricsReceived -= value;
            }
        }

        /// <summary>
        /// Set the metric retrieval timer interval
        /// </summary>
        public void SetInterval(TimeSpan interval)
        {
            Monitor.SetInterval(interval);
        }
    }
}
