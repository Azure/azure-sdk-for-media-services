//-----------------------------------------------------------------------
// <copyright file="MonitoringConfigurationTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class MonitoringConfigurationTests
    {
        private MediaContextBase _mediaContext;

        private const string NotificationId = "testNotificationPoint";

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void TestMonitoringConfiguration()
        {
            var monitoringConfiguration = _mediaContext.MonitoringConfigurations.Create(NotificationId,
                new List<ComponentMonitoringSetting>()
                {
                    new ComponentMonitoringSetting(MonitoringComponent.Channel, MonitoringLevel.Normal),
                    new ComponentMonitoringSetting(MonitoringComponent.StreamingEndpoint, MonitoringLevel.Normal)
                });

            var monitoringConfigurationCollection = _mediaContext.MonitoringConfigurations.ToArray();
            Assert.AreEqual(monitoringConfigurationCollection.Length, 1);
            Assert.IsNotNull(monitoringConfiguration.Id);
            Assert.IsNotNull(monitoringConfiguration.Created);
            Assert.IsNotNull(monitoringConfiguration.LastModified);

            monitoringConfiguration.Settings.ElementAt(0).Level = MonitoringLevel.Verbose;
            monitoringConfiguration.Update();

            monitoringConfigurationCollection = _mediaContext.MonitoringConfigurations.ToArray();
            Assert.AreEqual(monitoringConfigurationCollection.Length, 1);
            Assert.AreEqual(monitoringConfigurationCollection[0].Settings.ElementAt(0).Level, MonitoringLevel.Verbose);

            monitoringConfiguration.Settings.ElementAt(0).Level = MonitoringLevel.Normal;
            monitoringConfiguration.Update();

            monitoringConfigurationCollection = _mediaContext.MonitoringConfigurations.ToArray();
            Assert.IsNotNull(monitoringConfigurationCollection);
            Assert.AreEqual(monitoringConfigurationCollection.Length, 1);
            Assert.AreEqual(monitoringConfigurationCollection[0].NotificationEndPointId, NotificationId);
            var componentMonitoringSettings = monitoringConfigurationCollection[0].Settings;
            Assert.AreEqual(componentMonitoringSettings.Count, 2);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(0).Component, MonitoringComponent.Channel);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(0).Level, MonitoringLevel.Normal);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(1).Component, MonitoringComponent.StreamingEndpoint);
            Assert.AreEqual(componentMonitoringSettings.ElementAt(1).Level, MonitoringLevel.Normal);

            monitoringConfiguration.Delete();
            monitoringConfigurationCollection = _mediaContext.MonitoringConfigurations.ToArray();
            Assert.AreEqual(monitoringConfigurationCollection.Length, 0);
        }
    }
}
