//-----------------------------------------------------------------------
// <copyright file="AccessPoliciesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Moq;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class AccessPoliciesTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        public void ShouldReturnAccessPolicyWhenCreateCalled()
        {
            // Arrange
            string name = "TestPolicy";
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.List | AccessPermissions.Read;

            // Act
            IAccessPolicy actual = _mediaContext.AccessPolicies.Create(name, duration, permissions);

            // Assert
            Assert.AreEqual(name, actual.Name);
            Assert.AreEqual(duration, actual.Duration);
            Assert.AreEqual(permissions, actual.Permissions);
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyWhenCreateCalled()
        {
            // Arrange

            string name = "TestPolicy " + Guid.NewGuid().ToString("N");
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.Write | AccessPermissions.Delete;

            // Act
            IAccessPolicy expected = _mediaContext.AccessPolicies.Create(name, duration, permissions);

            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            IAccessPolicy actual = context2.AccessPolicies.Where(x => x.Name == expected.Name).FirstOrDefault();

            // Assert
            Assert.AreEqual(name, actual.Name);
            Assert.AreEqual(duration, actual.Duration);
            Assert.AreEqual(permissions, actual.Permissions);
        }

        [TestMethod]
        public void ShouldCreateAccessPolicyAsyncWhenCreateAsyncCalled()
        {
            // Arrange         

            string name = "TestPolicy";
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.List | AccessPermissions.Read;

            // Act
            Task<IAccessPolicy> task = _mediaContext.AccessPolicies.CreateAsync(name, duration, permissions);
            task.Wait();

            IAccessPolicy actual = task.Result;

            // Assert
            Assert.AreEqual(name, actual.Name);
            Assert.AreEqual(duration, actual.Duration);
            Assert.AreEqual(permissions, actual.Permissions);
        }

        [TestMethod]
        public void ShouldRemoveAccessPolicyFromCollectionWhenDeleteCalled()
        {
            // Arrange

            string name = "TestPolicy " + Guid.NewGuid().ToString("N");
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.List | AccessPermissions.Read;

            IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create(name, duration, permissions);

            // Act
            accessPolicy.Delete();

            IAccessPolicy actual = _mediaContext.AccessPolicies.Where(x => x.Name == name).FirstOrDefault();

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ShouldDeleteAccessPolicyWhenDeleteCalled()
        {
            // Arrange

            string name = "TestPolicy " + Guid.NewGuid().ToString("N");
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.List | AccessPermissions.Read;

            _mediaContext.AccessPolicies.Create(name, duration, permissions);

            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IAccessPolicy accessPolicy = context2.AccessPolicies.Where(x => x.Name == name).FirstOrDefault();

            // Act
            accessPolicy.Delete();

            CloudMediaContext context3 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IAccessPolicy actual = context2.AccessPolicies.Where(x => x.Name == name).FirstOrDefault();

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ShouldDeleteAccessPolicyAsyncWhenDeleteAsyncCalled()
        {
            // Arrange

            string name = "TestPolicy " + Guid.NewGuid().ToString("N");
            var duration = new TimeSpan(1, 0, 0);
            AccessPermissions permissions = AccessPermissions.List | AccessPermissions.Read;

            _mediaContext.AccessPolicies.Create(name, duration, permissions);

            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IAccessPolicy accessPolicy = context2.AccessPolicies.Where(x => x.Name == name).FirstOrDefault();

            // Act
            Task task = accessPolicy.DeleteAsync();
            task.Wait();

            CloudMediaContext context3 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IAccessPolicy actual = context2.AccessPolicies.Where(x => x.Name == name).FirstOrDefault();

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        [Priority(0)]
        public void TestAccessPolicyCreateRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var expected = new AccessPolicyData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = expected };
            var fakeException = new WebException("testException", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("AccessPolicies", It.IsAny<object>()));
            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(It.IsAny<object>()))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Empty", TimeSpan.FromSeconds(1), AccessPermissions.None);
            Assert.AreEqual(expected.Name, policy.Name);
            Assert.AreEqual(0, exceptionCount);
        }

        [TestMethod]
        [Priority(0)]
        public void TestAccessPolicyDeleteRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var policy = new AccessPolicyData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = policy };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Files", policy));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(policy));

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(policy))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            policy.SetMediaContext(_mediaContext);

            policy.Delete();

            Assert.AreEqual(0, exceptionCount);
        }
    }
}