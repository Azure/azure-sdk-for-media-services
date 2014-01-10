//-----------------------------------------------------------------------
// <copyright file="NotificationEndPoint.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class NotificationEndPointTests
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestNotificationEndPointCreateRetry()
        {
            var expected = new NotificationEndPoint { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("NotificationEndPoints", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            INotificationEndPoint actual = _mediaContext.NotificationEndPoints.Create("Empty", NotificationEndPointType.AzureQueue, "127.0.0.1");
            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestNotificationEndPointCreateFailedRetry()
        {
            var expected = new NotificationEndPoint { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("NotificationEndPoints", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.NotificationEndPoints.Create("Empty", NotificationEndPointType.AzureQueue, "127.0.0.1");
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
        public void TestNotificationEndPointCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new NotificationEndPoint { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("NotificationEndPoints", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.NotificationEndPoints.Create("Empty", NotificationEndPointType.AzureQueue, "127.0.0.1");
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
        public void TestNotificationEndPointUpdateRetry()
        {
            var data = new NotificationEndPoint { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("NotificationEndPoints", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestNotificationEndPointDeleteRetry()
        {
            var data = new NotificationEndPoint { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("NotificationEndPoints", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
        #endregion Retry Logic tests
    }
}