//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyOptionTests.cs" company="Microsoft">Copyright 2013 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ContentKeyAuthorizationPolicyOptionTests
    {
        private CloudMediaContext _mediaContext;
        private IContentKeyAuthorizationPolicyOption _testOption;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            responseTemplate.LicenseTemplates.Add(new PlayReadyLicenseTemplate());

            TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate();
            tokenRestrictionTemplate.PrimaryVerificationKey = new SymmetricVerificationKey(); // the default constructor automatically generates a random key
            tokenRestrictionTemplate.Audience = new Uri("http://sampleIssuerUrl");
            tokenRestrictionTemplate.Issuer = new Uri("http://sampleAudience");

            string optionName = "integrationtest-crud-749";
            string requirements = TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
            string configuration = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
            ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.TokenRestricted;

            _testOption = CreateOption(_mediaContext, optionName, ContentKeyDeliveryType.PlayReadyLicense, requirements, configuration, restrictionType);
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
        public void ContentKeyAuthorizationPolicyOptionTestEnumQuery()
        {
            var policyOptions = _mediaContext.ContentKeyAuthorizationPolicyOptions;

            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            responseTemplate.LicenseTemplates.Add(new PlayReadyLicenseTemplate());

            string optionName = "integrationtest-crud-746";
            string requirements = null;
            string configuration = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);
            ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.Open;

            IContentKeyAuthorizationPolicyOption option = CreateOption(_mediaContext, optionName, ContentKeyDeliveryType.PlayReadyLicense, requirements, configuration, restrictionType);

            var ok = policyOptions.Where(o => o.KeyDeliveryType == ContentKeyDeliveryType.PlayReadyLicense).AsEnumerable().Any();

            Assert.IsTrue(ok, "Can not find option by DeliveryType");
        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyOptionCreateRetry()
        {
            var expected = new ContentKeyAuthorizationPolicyOptionData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicyOptions", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            IContentKeyAuthorizationPolicyOption actual = _mediaContext.ContentKeyAuthorizationPolicyOptions.Create("Empty", ContentKeyDeliveryType.None, null, null);

            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestContentKeyAuthorizationPolicyOptionCreateFailedRetry()
        {
            var expected = new ContentKeyAuthorizationPolicyOptionData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicyOptions", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                IContentKeyAuthorizationPolicyOption actual = _mediaContext.ContentKeyAuthorizationPolicyOptions.Create("Empty", ContentKeyDeliveryType.None, null, null);
            }
            catch (WebException x)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestContentKeyAuthorizationPolicyOptionCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new ContentKeyAuthorizationPolicyOptionData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicyOptions", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                IContentKeyAuthorizationPolicyOption actual = _mediaContext.ContentKeyAuthorizationPolicyOptions.Create("Empty", ContentKeyDeliveryType.None, null, null);
            }
            catch (WebException x)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(1));
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyOptionUpdateRetry()
        {
            var data = new ContentKeyAuthorizationPolicyOptionData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicyOptions", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyOptionDeleteRetry()
        {
            var data = new ContentKeyAuthorizationPolicyOptionData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicyOptions", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
        #endregion Retry Logic tests

        public static IContentKeyAuthorizationPolicyOption CreateOption(CloudMediaContext dataContext, string optionName, ContentKeyDeliveryType deliveryType, string requirements, string configuration, ContentKeyRestrictionType restrictionType)
        {
            var restrictions = new List<ContentKeyAuthorizationPolicyRestriction>
                {
                    new ContentKeyAuthorizationPolicyRestriction { Requirements = requirements, Name = "somename" }
                };

            restrictions[0].SetKeyRestrictionTypeValue(restrictionType);

            IContentKeyAuthorizationPolicyOption option = dataContext.ContentKeyAuthorizationPolicyOptions.Create(
                optionName,
                deliveryType,
                restrictions,
                configuration);

            return option;
        }

        private IContentKeyAuthorizationPolicyOption GetOption(string id)
        {
            return _mediaContext.ContentKeyAuthorizationPolicyOptions.Where(o => o.Id == id).AsEnumerable().SingleOrDefault();
        }
    }
}
