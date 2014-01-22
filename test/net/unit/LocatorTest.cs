//-----------------------------------------------------------------------
// <copyright file="LocatorTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class LocatorTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void ShouldVerifyParametersInLocatorCreate()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            bool assetNullverified = false;
            try
            {
                _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, null, policy);
            }
            catch (ArgumentNullException)
            {
                assetNullverified = true;
            }

            bool policyNullverified = false;
            try
            {
                _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, null);
            }
            catch (ArgumentNullException)
            {
                policyNullverified = true;
            }

            Assert.IsTrue(assetNullverified, "Expecting ArgumentNullException passing asset as null");
            Assert.IsTrue(policyNullverified, "Expecting ArgumentNullException passing policy as null");
        }

        [TestMethod]
        public void LocatorCRUD()
        {
            ILocator stubed = _mediaContext.Locators.FirstOrDefault();
            Assert.IsNotNull(stubed);
            Assert.IsNotNull(stubed.Asset);
            Assert.IsNotNull(stubed.AccessPolicy);

            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, policy);
            Assert.IsNotNull(locator.AccessPolicy);
            Assert.IsNotNull(locator.Asset);
            locator.Update(DateTime.UtcNow.AddDays(5));
            locator.Update(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5));
            locator.UpdateAsync(DateTime.UtcNow.AddDays(5));
            locator.Delete();
            Assert.IsNull(_mediaContext.Locators.Where(c => c.Id == locator.Id).FirstOrDefault());

            ILocator sas = _mediaContext.Locators.CreateSasLocator(asset, policy);
            sas.Delete();

            sas = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy).Result;
            sas.Delete();
            ILocator sasAsync = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy, DateTime.UtcNow, "Name").Result;
            sasAsync.DeleteAsync().Wait();
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ValidateAccessPolicyInCreateLocatorAsync()
        {
            IAsset asset = _mediaContext.Assets.FirstOrDefault();
            Assert.IsNotNull(asset);
            IAccessPolicy policy = _mediaContext.AccessPolicies.FirstOrDefault();
            Assert.IsNotNull(policy);
            _mediaContext.Locators.CreateLocatorAsync(LocatorType.None, asset, null);
        }

        [TestMethod]
        public void CreateLocatorAsync()
        {
            IAsset asset = _mediaContext.Assets.FirstOrDefault();
            Assert.IsNotNull(asset);
            IAccessPolicy policy = _mediaContext.AccessPolicies.FirstOrDefault();
            Assert.IsNotNull(policy);
            _mediaContext.Locators.CreateLocatorAsync(LocatorType.None, asset, policy);
        }

        [TestMethod]
        public void AccessPolicyCRUD()
        {
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            Assert.AreEqual(AccessPermissions.Read, policy.Permissions);
            Assert.AreEqual(TimeSpan.FromDays(1), policy.Duration);
            Assert.AreEqual("AccessPolicyCRUD", policy.Name);
            policy.Delete();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            policy.DeleteAsync();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(-1), AccessPermissions.Read);
            policy.DeleteAsync();
        }

        [TestMethod]
        [ExpectedException(typeof (System.InvalidOperationException))]
        public void OnlyOriginLocatorCanBeUpdated()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.Sas, asset, policy);
            locator.Update(DateTime.UtcNow.AddDays(5));
        }
    }
}