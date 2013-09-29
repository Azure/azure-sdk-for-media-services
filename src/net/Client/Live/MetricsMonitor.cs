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
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Channel metrics monitor class
    /// </summary>
    public sealed class MetricsMonitor<T> : LiveMonitor, IMetricsMonitor<T>
    {
        private const string AllMetricsEventHandlerKey = "00000000";

        private readonly IQueryable<T> _metricsQueryable;
        private readonly Dictionary<string, EventHandler<MetricsEventArgs<T>>> _eventHandlers;
        private readonly object _objectLock = new Object();

        /// <summary>
        /// Construct a MetricsMonitor object for origin or channel
        /// </summary>
        /// <param name="metricsQueryable">all metrics queryable</param>
        internal MetricsMonitor(IQueryable<T> metricsQueryable)
        {
            _metricsQueryable = metricsQueryable;
            _eventHandlers = new Dictionary<string, EventHandler<MetricsEventArgs<T>>>();
        }

        /// <summary>
        /// Set the default timer interval
        /// </summary>
        protected override TimeSpan DefaultTimerInterval
        {
            get
            {
                return TimeSpan.FromSeconds(30);
            }
        }

        /// <summary>
        /// EventHandler for all channel or origin metrics received
        /// </summary>
        public event EventHandler<MetricsEventArgs<T>> MetricsReceived
        {
            add
            {
                Subscribe(AllMetricsEventHandlerKey, value);
            }
            remove
            {
                Unsubscribe(AllMetricsEventHandlerKey, value);
            }
        }

        /// <summary>
        /// Subscribe an event handler to the monitor for a specific channel or origin 
        /// </summary>
        /// <param name="id">Channel or Origin ID</param>
        /// <param name="metricsReceived">Metric received event handler</param>
        public void Subscribe(string id, EventHandler<MetricsEventArgs<T>> metricsReceived)
        {
            var internalId = GetGuidString(id);

            lock (_objectLock)
            {
                if (_eventHandlers.ContainsKey(internalId))
                {
                    _eventHandlers[internalId] += metricsReceived;
                }
                else
                {
                    _eventHandlers.Add(internalId, metricsReceived);
                }

                // start the timer
                Start();
            }
        }

        /// <summary>
        /// Unsubscribe an event handler to the monitor for a specific channel or origin
        /// </summary>
        /// <param name="id">Channel or Origin ID</param>
        /// <param name="metricsReceived">Metric received event handler</param>
        public void Unsubscribe(string id, EventHandler<MetricsEventArgs<T>> metricsReceived)
        {
            var internalId = GetGuidString(id);

            lock (_objectLock)
            {
                if (_eventHandlers.ContainsKey(internalId))
                {
                    // ReSharper disable once DelegateSubtraction
                    _eventHandlers[internalId] -= metricsReceived;
                    if (_eventHandlers[internalId] == null)
                    {
                        _eventHandlers.Remove(internalId);
                    }
                }

                if (_eventHandlers.Count == 0)
                {
                    // if there is no subscriber, stop
                    Stop();
                }
            }
        }

        /// <summary>
        /// Override the timer elapsed event: Retrieve and publish events
        /// </summary>
        protected override void DoMonitor()
        {
            lock (_objectLock)
            {
                if (_eventHandlers.Count <= 0) return;
                
                var metrics = GetMetrics();

                //notify single channel or origin metric subscriber
                foreach (var handler in _eventHandlers)
                {
                    handler.Value.BeginInvoke(
                        this,
                        new MetricsEventArgs<T>
                        {
                            Metrics =
                                handler.Key == AllMetricsEventHandlerKey
                                    ? metrics.Values.ToList().AsReadOnly()
                                    : new List<T> {metrics[handler.Key]}.AsReadOnly()
                        },
                        null,
                        null);
                }
            }
        }

        /// <summary>
        /// Get the list of Metrics 
        /// There is only one element in the list if monitoring a single channel or origin
        /// </summary>
        /// <returns>The list of metrics</returns>
        private IDictionary<string, T> GetMetrics()
        {
            return _metricsQueryable.ToDictionary(GetGuidString, m => m);
        }

        /// <summary>
        /// Get the Guid part of an metric Id
        /// </summary>
        /// <param name="metric">a channel or origin metric object</param>
        /// <returns>Metric Guid in string</returns>
        public static string GetGuidString(T metric)
        {
            var originMetric = metric as IOriginMetric;
            if (originMetric != null)
            {
                return GetGuidString(originMetric.Id);
            }

            var channelMetric = metric as IChannelMetric;
            if (channelMetric != null)
            {
                return GetGuidString(channelMetric.Id);
            }

            throw new ArgumentException("metric");
        }

        /// <summary>
        /// Remove the Id prefix and return the Guid part
        /// </summary>
        /// <param name="oid">Channel ID, Origin ID, or Metric ID</param>
        /// <returns>Guid in string</returns>
        public static string GetGuidString(string oid)
        {
            if (string.IsNullOrEmpty(oid))
            {
                throw new ArgumentNullException(oid);
            }
            var pieces = oid.Split(':');
            return pieces[pieces.Length - 1];
        }
    }
}
