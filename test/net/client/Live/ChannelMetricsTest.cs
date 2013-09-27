//-----------------------------------------------------------------------
// <copyright file="AssetFilesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Live
{
    [TestClass]
    public class ChannelMetricsTest
    {
        private CloudMediaContext _dataContext;
        private int _notificationCount;

        [TestInitialize]
        public void SetupTest()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;

            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _notificationCount = 0;
        }

        /// <summary>
        /// Get all channel metrics
        /// </summary>
        [TestMethod]
        public void GetAllMetricsTest()
        {
            var metrics = _dataContext.ChannelMetrics;
            Assert.IsNotNull(metrics);

            var channels = _dataContext.Channels;

            if (channels.Count() > 0)
            {
                Assert.IsTrue(metrics.Count() > 0);
            }
        }

        /// <summary>
        /// Get single channel metrics
        /// </summary>
        [TestMethod]
        public void GetSingleMetricTest()
        {
            var metrics = _dataContext.ChannelMetrics.ToDictionary(m => GetGuid(m.Id), m => m);
            foreach (var channel in _dataContext.Channels)
            {
                var id = GetGuid(channel.Id);

                IChannelMetric metric1 = null;
                if (metrics.ContainsKey(id))
                {
                    metric1 = metrics[id];
                }

                if (metric1 == null) continue;

                var metric2 = channel.GetMetric();

                Assert.IsNotNull(metric2);
                if (metric1.IngestMetrics != null)
                {
                    Assert.IsNotNull(metric2.IngestMetrics);
                    Assert.AreEqual(metric1.IngestMetrics.Count, metric2.IngestMetrics.Count);
                }

                if (metric1.ProgramMetrics != null)
                {
                    Assert.IsNotNull(metric2.ProgramMetrics);
                    Assert.AreEqual(metric1.ProgramMetrics.Count, metric2.ProgramMetrics.Count);
                }
            }          
        }

        /// <summary>
        /// Subscribe to all channel metrics monitor
        /// </summary>
        [TestMethod]
        public void SubscribeAllMetricsMonitorTest()
        {
            var monitor = _dataContext.ChannelMetrics.Monitor;

            monitor.MetricReceived += OnMetricsReceived;
            monitor.Start();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            monitor.Stop();

            Assert.AreEqual(_notificationCount, 2);
        }

        /// <summary>
        /// Subscribe to a signle channel metric monitor
        /// </summary>
        [TestMethod]
        public void SubscribeSingleMetricMonitorTest()
        {
            var channels = _dataContext.Channels.ToList();
            if (channels.Count < 1) return;

            var channel = channels[channels.Count - 1];

            var monitor = channel.MetricsMonitor;

            monitor.MetricReceived += OnMetricsReceived;
            monitor.Start();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            monitor.Stop();

            Assert.AreEqual(_notificationCount, 2);
        }

        private void OnMetricsReceived(object sender, ChannelMetricsEventArgs eventArgs)
        {
            Assert.IsNotNull(eventArgs.ChannelMetrics);
            Assert.IsTrue(eventArgs.ChannelMetrics.Count > 0);
            _notificationCount++;
        }

        public static Guid GetGuid(string oid)
        {
            if (string.IsNullOrEmpty(oid))
            {
                throw new ArgumentNullException(oid);
            }
            var pieces = oid.Split(':');
            return new Guid(pieces[pieces.Length - 1]);
        }
    }
}
