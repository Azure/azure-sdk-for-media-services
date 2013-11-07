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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using System.Collections.Generic;
using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ContentKeyAuthorizationPolicyOptionTests
    {
        private CloudMediaContext _dataContext;
        private IContentKeyAuthorizationPolicyOption _testOption;

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            string optionName = "integrationtest-crud-749";
            string requirements = "somerequirements";
            string configuration = "someconfiguration";
            ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.IPRestricted;

            _testOption = CreateOption(_dataContext,optionName, requirements, configuration, restrictionType);
        }

        /*[TestCleanup] enable when rest layer bug is fixed
        public void CleanupTest()
        {
            _testOption.Delete();
            var policyOptions = _dataContext.ContentKeyAuthorizationPolicyOptions;
            var deleted = !policyOptions.Where(o => o.Id == _testOption.Id).Any();
            Assert.IsTrue(deleted, "ContentKeyAuthorizationPolicyOption was not deleted");
        }*/

        [TestMethod]
        public void ContentKeyAuthorizationPolicyOptionTestUpdate()
        {
            var createdOption = GetOption(_testOption.Id);

            Assert.AreEqual(_testOption.Name, createdOption.Name);
            Assert.AreEqual(_testOption.Restrictions[0].Requirements, createdOption.Restrictions[0].Requirements);
            Assert.AreEqual(_testOption.Restrictions[0].KeyRestrictionType, createdOption.Restrictions[0].KeyRestrictionType);

            string newName = "somenewname";
            _testOption.Name = newName;
            _testOption.Update();

            var updated = GetOption(_testOption.Id);
            Assert.AreEqual(newName, updated.Name);
        }

        [TestMethod]
        [Ignore] //enable when rest layer bug is fixed
        public void ContentKeyAuthorizationPolicyOptionTestEnumQuery()
        {
            var policyOptions = _dataContext.ContentKeyAuthorizationPolicyOptions;

            string optionName = "integrationtest-crud-746";
            string requirements = "somerequirements";
            string configuration = "someconfiguration";
            ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.IPRestricted;

            IContentKeyAuthorizationPolicyOption option = CreateOption(_dataContext,optionName, requirements, configuration, restrictionType);

            var ok = policyOptions.Where(o => o.KeyDeliveryType == ContentKeyDeliveryType.PlayReadyLicense).AsEnumerable().Any();

            Assert.IsTrue(ok, "Can not find option by DeliveryType");
        }

        public static IContentKeyAuthorizationPolicyOption CreateOption(CloudMediaContext dataContext,string optionName, string requirements, string configuration, ContentKeyRestrictionType restrictionType)
        {
            var restrictions = new List<ContentKeyAuthorizationPolicyRestriction>
                {
                    new ContentKeyAuthorizationPolicyRestriction { Requirements = requirements, Name = "somename" }
                };

            restrictions[0].SetKeyRestrictionTypeValue(restrictionType);

            IContentKeyAuthorizationPolicyOption option = dataContext.ContentKeyAuthorizationPolicyOptions.Create(
                optionName,
                ContentKeyAuthorization.ContentKeyDeliveryType.PlayReadyLicense,
                restrictions,
                configuration);
            return option;
        }

        private IContentKeyAuthorizationPolicyOption GetOption(string id)
        {
            return _dataContext.ContentKeyAuthorizationPolicyOptions.Where(o => o.Id == id).AsEnumerable().SingleOrDefault();
        }

        private IContentKey CreateTestKey()
        {
            byte[] key = new byte[16];
            new Random().NextBytes(key);
            var contentKey = _dataContext.ContentKeys.Create(Guid.NewGuid(), key, "unit-testkey-340");

            return contentKey;
        }
    }
}
