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
    public class OriginMetricsTest
    {
        private CloudMediaContext _dataContext;
        private int _notificationCount;

        private const string ApiServerAddress = "https://shelirest.cloudapp.net/api";
        private const string AccountName = "streamingdev";
        private const string AccountKey = "vUeuvDU3MIgHuFZCU3cX+24wWg6r4qho594cRcEr5fU=";
        private const string AccountAcsScope = "urn:Nimbus";
        private const string AcsBaseAddress = "https://nimbustestaccounts.accesscontrol.windows.net";
        private const string OriginServiceName = "shelitest1";

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = CreateCloudMediaContext();
            _notificationCount = 0;
        }

        /// <summary>
        /// Get all origin metrics
        /// </summary>
        [TestMethod]
        public void GetAllMetricsTest()
        {
            var metrics = _dataContext.OriginMetrics;

            Assert.IsNotNull(metrics);
            Assert.IsTrue(metrics.Count() > 0);
        }

        /// <summary>
        /// Get single origin metrics
        /// </summary>
        [TestMethod]
        public void GetSingleMetricTest()
        {
            var metric = _dataContext.OriginMetrics.Where(m => m.ServiceName.Contains(OriginServiceName)).SingleOrDefault();
            var origin = _dataContext.Origins.Where(o => o.HostName.Contains(OriginServiceName)).SingleOrDefault();
            var metric2 = origin.GetMetric();

            Assert.IsNotNull(metric);
            Assert.IsNotNull(metric2);
            Assert.AreEqual(metric.EgressMetrics.Count, metric2.EgressMetrics.Count);
        }

        /// <summary>
        /// Subscribe to all origin metrics monitor
        /// </summary>
        [TestMethod]
        public void SubscribeAllMetricsMonitorTest()
        {
            var monitor = _dataContext.OriginMetrics.Monitor;

            monitor.MetricReceived += OnMetricsReceived;
            monitor.Start();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            monitor.Stop();

            Assert.AreEqual(_notificationCount, 2);
        }

        /// <summary>
        /// Subscribe to a signle origin metric monitor
        /// </summary>
        [TestMethod]
        public void SubscribeSingleMetricMonitorTest()
        {
            var origin = _dataContext.Origins.Where(o => o.HostName.Contains(OriginServiceName)).SingleOrDefault();
            var monitor = origin.MetricsMonitor;

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

        private void OnMetricsReceived(object sender, OriginMetricsEventArgs eventArgs)
        {
            Assert.IsNotNull(eventArgs.OriginMetrics);
            Assert.IsTrue(eventArgs.OriginMetrics.Count > 0);
            _notificationCount ++;
        }
    }
}
