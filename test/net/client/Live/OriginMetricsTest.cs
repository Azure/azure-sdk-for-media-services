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
    public class OriginMetricsTest
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
        /// Get all origin metrics
        /// </summary>
        [TestMethod]
        public void GetAllMetricsTest()
        {
            var metricCount = _dataContext.OriginMetrics.Count();
            var originCount = _dataContext.Origins.Count();

            Assert.AreEqual(metricCount, originCount);
        }

        /// <summary>
        /// Get single origin metrics
        /// </summary>
        [TestMethod]
        public void GetSingleMetricTest()
        {
            var metrics = _dataContext.OriginMetrics.ToDictionary(
                m => MetricsMonitor<IOriginMetric>.GetGuidString(m.Id), 
                m => m);

            foreach (var origin in _dataContext.Origins)
            {
                var id = MetricsMonitor<IOriginMetric>.GetGuidString(origin.Id);

                IOriginMetric metric1 = null;
                if (metrics.ContainsKey(id))
                {
                    metric1 = metrics[id];
                }

                if (metric1 == null) continue;

                var metric2 = origin.GetMetric();

                Assert.IsNotNull(metric2);
                Assert.AreEqual(metric1.EgressMetrics.Count, metric2.EgressMetrics.Count);
            }          
        }

        /// <summary>
        /// Get single origin metric using origin name
        /// </summary>
        [TestMethod]
        public void QueryMetricUsingOriginNameTest()
        {
            foreach (var origin in _dataContext.Origins)
            {
                var originName = origin.Name.Split('.')[0];
                var metric1 = _dataContext.OriginMetrics.Where(m => m.OriginName.Contains(originName)).SingleOrDefault();

                if (metric1 == null) continue;
                var metric2 = origin.GetMetric();

                Assert.IsNotNull(metric2);
                Assert.AreEqual(metric1.EgressMetrics.Count, metric2.EgressMetrics.Count);
            }
        }

        /// <summary>
        /// Subscribe to all origin metrics monitor
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SubscribeAllMetricsMonitorTest()
        {
            _dataContext.OriginMetrics.MetricsReceived += OnMetricsReceived;

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            _dataContext.OriginMetrics.MetricsReceived -= OnMetricsReceived;

            Assert.AreEqual(_notificationCount, 2);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            Assert.AreEqual(_notificationCount, 2);
        }

        /// <summary>
        /// Subscribe to a signle origin metric monitor
        /// </summary>
        [TestMethod]
        [Ignore]
        public void SubscribeSingleMetricMonitorTest()
        {
            var origins = _dataContext.Origins.ToList();
            if (origins.Count < 1) return;
            
            var origin = origins[origins.Count-1];

            origin.MetricsReceived += OnMetricsReceived;

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            origin.MetricsReceived -= OnMetricsReceived;

            Assert.AreEqual(_notificationCount, 2);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(61));

            Assert.AreEqual(_notificationCount, 2);
        }

        private void OnMetricsReceived(object sender,MetricsEventArgs<IOriginMetric> eventArgs)
        {
            Assert.IsNotNull(eventArgs.Metrics);
            Assert.IsTrue(eventArgs.Metrics.Count > 0);
            _notificationCount ++;
        }
    }
}
