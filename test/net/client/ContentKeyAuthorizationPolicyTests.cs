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
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ContentKeyAuthorizationPolicyTests
    {
        private CloudMediaContext _mediaContext;
        private string testRun;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            testRun = "integrationtest_" + Guid.NewGuid();
        }

        [TestMethod]
        public void CreateContentKeyAuthorizationPolicy()
        {
            ContentKeyAuthorizationPolicyCollection collection = _mediaContext.ContentKeyAuthorizationPolicies;
            IContentKeyAuthorizationPolicy policy = collection.CreateAsync("Test").Result;
            Assert.IsNotNull(policy.Id);
            Assert.IsNotNull(policy.Name);
            Assert.IsNotNull(policy.Options);
        }

        [TestMethod]
        public void CreateAndDeleteContentKeyAuthorizationPolicy()
        {
            ContentKeyAuthorizationPolicyCollection collection = _mediaContext.ContentKeyAuthorizationPolicies;
            IContentKeyAuthorizationPolicy policy = collection.CreateAsync(testRun).Result;
            Assert.IsNotNull(policy.Id);
            Assert.IsNotNull(policy.Name);
            Assert.IsNotNull(policy.Options);
            IMediaDataServiceResponse response = policy.DeleteAsync().Result;
        }


        [TestMethod]
        public void ExecuteFirstOrDefaultAuthorizationPolicy()
        {
            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(testRun).Result;
            IContentKeyAuthorizationPolicy first = _mediaContext.ContentKeyAuthorizationPolicies.FirstOrDefault();
        }

        [TestMethod]
        public void ExecuteCountForAuthorizationPolicy()
        {
            int count = _mediaContext.ContentKeyAuthorizationPolicies.Count();
        }

        [TestMethod]
        public void ExecuteTopForAuthorizationPolicy()
        {
            List<IContentKeyAuthorizationPolicy> policies = _mediaContext.ContentKeyAuthorizationPolicies.Take(5).ToList();
        }

        [TestMethod]
        public void FilterPolicyById()
        {
            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.Where(p => p.Id == "nb:ckpid:UUID:" + Guid.NewGuid().ToString()).FirstOrDefault();
        }

        [TestMethod]
        public void FilterPolicyByName()
        {
            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.Where(p => p.Name == testRun).FirstOrDefault();
        }

        [TestMethod]
        public void CreateAndUpdateContentKeyAuthorizationPolicy()
        {
            ContentKeyAuthorizationPolicyCollection collection = _mediaContext.ContentKeyAuthorizationPolicies;
            string name = testRun + "_CreateAndUpdateContentKeyAuthorizationPolicy_OriginalName";
            string updatedname = testRun + "_CreateAndUpdateContentKeyAuthorizationPolicy_UpdatedName";
            IContentKeyAuthorizationPolicy policy = collection.CreateAsync(name).Result;
            Assert.IsNotNull(policy.Id);
            Assert.IsNotNull(policy.Name);
            Assert.IsNotNull(policy.Options);
            Assert.AreEqual(name, policy.Name);
            policy.Name = updatedname;
            IContentKeyAuthorizationPolicy policyAfterUpdate = policy.UpdateAsync().Result;
            Assert.AreEqual(updatedname, policyAfterUpdate.Name);
            Assert.AreEqual(policy.Id, policyAfterUpdate.Id);
        }

        [TestMethod]
        public void UpdateContentKeyAuthorizationPolicyId()
        {
            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(testRun).Result;
            IContentKey contentKey = _mediaContext.ContentKeys.CreateAsync(Guid.NewGuid(), new byte[16]).Result;
            contentKey.AuthorizationPolicyId = policy.Id;
            IContentKey updated = contentKey.UpdateAsync().Result;

            IContentKey updatedContentKey = _mediaContext.ContentKeys.Where(c => c.Id == contentKey.Id).FirstOrDefault();

            //var updatedWithPolicy = _dataContext.ContentKeys.Where(c => c.AuthorizationPolicyId == policy.Id).FirstOrDefault();
            Assert.IsNotNull(updatedContentKey.AuthorizationPolicyId);
            Assert.AreEqual(policy.Id, updatedContentKey.AuthorizationPolicyId);

            contentKey.AuthorizationPolicyId = null;
            updated = contentKey.UpdateAsync().Result;
            Assert.IsNull(contentKey.AuthorizationPolicyId);
            updatedContentKey = _mediaContext.ContentKeys.Where(c => c.Id == contentKey.Id).FirstOrDefault();
            Assert.IsNull(updatedContentKey.AuthorizationPolicyId);
            contentKey.Delete();
        }

        [TestMethod]
        public void QueryContentKeyBYAuthorizationPolicyId()
        {
            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(testRun).Result;
            IContentKey contentKey = _mediaContext.ContentKeys.CreateAsync(Guid.NewGuid(), new byte[16]).Result;
            contentKey.AuthorizationPolicyId = policy.Id;
            contentKey.Update();
            IContentKey updatedKey = _mediaContext.ContentKeys.Where(c => c.AuthorizationPolicyId == policy.Id).FirstOrDefault();
            Assert.IsNotNull(updatedKey.AuthorizationPolicyId);

            contentKey.Delete();
        }

        [TestMethod]
        public void AddingOptionsToCreatedPolicy()
        {
             string optionName = "integrationtest-crud-749";
            string requirements = "somerequirements";
            string configuration = "someconfiguration";

            ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.IPRestricted;

            IContentKeyAuthorizationPolicy policy = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(testRun).Result;
            var option1 = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, optionName, requirements, configuration, restrictionType);
            policy.Options.Add(option1);

        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyCreateRetry()
        {
            var expected = new ContentKeyAuthorizationPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var task = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(expected.Name);
            task.Wait();
            IContentKeyAuthorizationPolicy actual = task.Result;

            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestContentKeyAuthorizationPolicyCreateFailedRetry()
        {
            var expected = new ContentKeyAuthorizationPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(expected.Name).Wait();
            }
            catch (AggregateException ax)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                WebException x = (WebException)ax.GetBaseException();
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestContentKeyAuthorizationPolicyCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new ContentKeyAuthorizationPolicyData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(expected.Name).Wait();
            }
            catch (AggregateException ax)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(1));
                WebException x = (WebException)ax.GetBaseException();
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyUpdateRetry()
        {
            var data = new ContentKeyAuthorizationPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestContentKeyAuthorizationPolicyDeleteRetry()
        {
            var data = new ContentKeyAuthorizationPolicyData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
        #endregion Retry Logic tests
    }
}