//-----------------------------------------------------------------------
// <copyright file="MonitoringConfigurationE2ETests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class MonitoringConfigurationE2ETests
    {
        private MediaContextBase _mediaContext;
        
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestMonitoringConfiguration()
        {
            // Get the current monitoring configuration settings
            var monitoringConfigurations = _mediaContext.MonitoringConfigurations.ToArray();
            var originalCount = monitoringConfigurations.Length;
            // at most one monitoring configuration settings is allowed
            Assert.IsTrue(originalCount <= 1);
            // only testing when there is one monitoring configuration settings, but it can cover
            // all CRUD operations of monitoring configuration settings
            if (originalCount == 1)
            {
                // test update operation
                var monitoringConfiguration = monitoringConfigurations[0];
                monitoringConfiguration.Settings.ElementAt(0).Level = MonitoringLevel.Verbose;
                monitoringConfiguration.Update();

                // test update and read operatoin
                monitoringConfigurations = _mediaContext.MonitoringConfigurations.ToArray();
                var monitoringConfiguration2 = monitoringConfigurations[0];
                Assert.AreEqual(monitoringConfiguration2.Settings.ElementAt(0).Level, MonitoringLevel.Verbose);
                // test delete operation
                monitoringConfiguration2.Delete();
                // test create operation
                _mediaContext.MonitoringConfigurations.Create(
                    monitoringConfiguration.NotificationEndPointId,
                    new List<ComponentMonitoringSetting>()
                    {
                        new ComponentMonitoringSetting(MonitoringComponent.Channel, MonitoringLevel.Normal),
                        new ComponentMonitoringSetting(MonitoringComponent.StreamingEndpoint, MonitoringLevel.Normal)
                    });
                VerifyMonitoringSettings();
            }
        }

        private void VerifyMonitoringSettings()
        {
            var monitoringConfigurations = _mediaContext.MonitoringConfigurations.ToArray();
            Assert.IsNotNull(monitoringConfigurations);
            Assert.AreEqual(monitoringConfigurations.Length, 1);
            Assert.IsNotNull(monitoringConfigurations[0].Id);
            Assert.IsNotNull(monitoringConfigurations[0].Created);
            Assert.IsNotNull(monitoringConfigurations[0].LastModified);
            var componentMonitoringSettings = monitoringConfigurations[0].Settings;
            Assert.AreEqual(componentMonitoringSettings.Count, 2);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(0).Component, MonitoringComponent.Channel);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(0).Level, MonitoringLevel.Normal);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(1).Component, MonitoringComponent.StreamingEndpoint);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(1).Level, MonitoringLevel.Normal);
        }
    }
}
