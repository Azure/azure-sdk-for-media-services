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

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Live
{
    [TestClass]
    public class ChannelMetricsTest
    {
        private CloudMediaContext _dataContext;
        private int _notificationCount;

        private const string ApiServerAddress = "https://shelirest.cloudapp.net/api";
        private const string AccountName = "streamingdev";
        private const string AccountKey = "vUeuvDU3MIgHuFZCU3cX+24wWg6r4qho594cRcEr5fU=";
        private const string AccountAcsScope = "urn:Nimbus";
        private const string AcsBaseAddress = "https://nimbustestaccounts.accesscontrol.windows.net";
        private const string ChannelServiceName = "channel1";

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = CreateCloudMediaContext();
            _notificationCount = 0;
        }

        /// <summary>
        /// Get all channel sink metrics
        /// </summary>
        [TestMethod]
        [Ignore]
        public void GetAllMetricsTest()
        {
            var metrics = _dataContext.ChannelMetrics;

            Assert.IsNotNull(metrics);
            Assert.IsTrue(metrics.Count() > 0);
        }

        /// <summary>
        /// Get single channel sink metrics
        /// </summary>
        [TestMethod]
        [Ignore]
        public void GetSingleMetricTest()
        {
            var metric = _dataContext.ChannelMetrics.Where(m => m.ServiceName.Contains(ChannelServiceName)).SingleOrDefault();
            var channel = _dataContext.Channels.Where(c => c.Name.Contains(ChannelServiceName)).SingleOrDefault();
            var metric2 = channel.GetMetric();

            Assert.IsNotNull(metric);
            Assert.IsNotNull(metric2);
            Assert.AreEqual(metric.IngestMetrics.Count, metric2.IngestMetrics.Count);
            Assert.AreEqual(metric.ProgramMetrics.Count, metric2.ProgramMetrics.Count);
        }

        /// <summary>
        /// Subscribe to all channel sink metrics monitor
        /// </summary>
        [TestMethod]
        [Ignore]
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
        /// Subscribe to a signle channel sink metric monitor
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SubscribeSingleMetricMonitorTest()
        {
            var channel = _dataContext.Channels.Where(c => c.Name.Contains(ChannelServiceName)).SingleOrDefault();
            var monitor = channel.MetricsMonitor;

            monitor.MetricReceived += OnMetricsReceived;
            monitor.Start();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            monitor.Stop();

            Assert.AreEqual(_notificationCount, 2);
        }

        private CloudMediaContext CreateCloudMediaContext()
        {
            return new CloudMediaContext(
                new Uri(ApiServerAddress),
                AccountName,
                AccountKey,
                AccountAcsScope,
                AcsBaseAddress);
        }

        private void OnMetricsReceived(object sender, ChannelMetricsEventArgs eventArgs)
        {
            Assert.IsNotNull(eventArgs.ChannelMetrics);
            Assert.IsTrue(eventArgs.ChannelMetrics.Count > 0);
            _notificationCount++;
        }
    }
}
