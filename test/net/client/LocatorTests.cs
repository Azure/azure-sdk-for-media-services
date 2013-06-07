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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class LocatorTests
    {
        private CloudMediaContext _dataContext;
        private string _smallWmv;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv);
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowIfLocatorIdIsInvalidWithoutPrefixWhenCreateOriginLocator()
        {
            IAsset asset = AssetTests.CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("Read", TimeSpan.FromMinutes(5), AccessPermissions.Read);
            string locatorId = "invalid-locator-id";

            _dataContext.Locators.CreateLocator(locatorId, LocatorType.OnDemandOrigin, asset, accessPolicy);
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowIfLocatorIdIsInvalidWithPrefixWhenCreateOriginLocator()
        {
            IAsset asset = AssetTests.CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("Read", TimeSpan.FromMinutes(5), AccessPermissions.Read);
            string locatorId = string.Concat(LocatorBaseCollection.LocatorIdentifierPrefix, "invalid-locator-id");

            _dataContext.Locators.CreateLocator(locatorId, LocatorType.OnDemandOrigin, asset, accessPolicy);
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateOriginLocator()
        {
            IAsset asset = AssetTests.CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("Read", TimeSpan.FromMinutes(5), AccessPermissions.Read);

            ILocator locator = _dataContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, accessPolicy);

            Assert.IsNotNull(locator);

            string locatorIdWithoutPrefix = locator.Id.Remove(0, LocatorBaseCollection.LocatorIdentifierPrefix.Length);
            Assert.AreEqual(locator.ContentAccessComponent, locatorIdWithoutPrefix, true);
            Assert.IsTrue(locator.Path.TrimEnd('/').EndsWith(locatorIdWithoutPrefix, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSetLocatorIdWithoutPrefixWhenCreateOriginLocator()
        {
            IAsset asset = AssetTests.CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("Read", TimeSpan.FromMinutes(5), AccessPermissions.Read);
            string locatorIdWithoutPrefix = Guid.NewGuid().ToString();

            ILocator locator = _dataContext.Locators.CreateLocator(locatorIdWithoutPrefix, LocatorType.OnDemandOrigin, asset, accessPolicy);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locator.ContentAccessComponent, locatorIdWithoutPrefix, true);
            Assert.IsTrue(locator.Path.TrimEnd('/').EndsWith(locatorIdWithoutPrefix, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldSetLocatorIdWithPrefixWhenCreateOriginLocator()
        {
            IAsset asset = AssetTests.CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("Read", TimeSpan.FromMinutes(5), AccessPermissions.Read);
            string locatorIdWithoutPrefix = Guid.NewGuid().ToString();
            string locatorIdWithPrefix = string.Concat(LocatorBaseCollection.LocatorIdentifierPrefix, locatorIdWithoutPrefix);

            ILocator locator = _dataContext.Locators.CreateLocator(locatorIdWithPrefix, LocatorType.OnDemandOrigin, asset, accessPolicy);

            Assert.IsNotNull(locator);
            Assert.AreEqual(locator.ContentAccessComponent, locatorIdWithoutPrefix, true);
            Assert.IsTrue(locator.Path.TrimEnd('/').EndsWith(locatorIdWithoutPrefix, StringComparison.OrdinalIgnoreCase));
        }
    }
}