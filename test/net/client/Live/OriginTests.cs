//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class OriginTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Priority(0)]
        public void TestOriginCreate()
        {
            string testOriginName = Guid.NewGuid().ToString().Substring(0, 30);
            var actual = _mediaContext.Origins.Create(testOriginName, 0);
            Assert.AreEqual(testOriginName, actual.Name);
        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestOriginCreateRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Origins", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                var actual = _mediaContext.Origins.Create("unittest", 0);
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestOriginCreateFailedRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Origins", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                var actual = _mediaContext.Origins.Create("unittest", 0);
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
        public void TestDeleteRetry()
        {
            var data = new OriginData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Origins", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.Delete();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestSendDeleteOperationRetry()
        {
            var data = new OriginData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Origins", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.SendDeleteOperation();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestGetMetric()
        {
            var data = new OriginData { Name = "testData", Id = "1" };

            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var fakeResponse = new OriginMetricData[] { new OriginMetricData() { OriginName = "test"} };
            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute<OriginMetricData>(It.IsAny<Uri>()))
                .Returns(() => 
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var result = data.GetMetric();
            Assert.AreEqual("test", result.OriginName);

            dataContextMock.Verify((ctxt) => ctxt.Execute<OriginMetricData>(It.IsAny<Uri>()), Times.Exactly(2));
        }

        #endregion Retry Logic tests
    }
}