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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    class TestRestEntity : RestEntity<StreamingEndpointData>
    {
        public TestRestEntity(MediaContextBase context)
        {
            SetMediaContext(context);
        }

        public void ExecuteActionAsyncTest()
        {
            ExecuteActionAsync(new Uri("http://whatever"), TimeSpan.FromMilliseconds(1)).Wait();
        }

        public void RefreshTest()
        {
            Refresh();
        }

        public IOperation SendOperationTest()
        {
            return SendOperation(new Uri("http://whatever"));
        }

        protected override string EntitySetName
        {
            get { return "StreamingEndpoints"; }
        }
    }

    [TestClass]
    [Ignore] //TODO: enable when the streaming endpoint is deployed in the test environment
    public class RestEntityTests
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
        public void TestRestEntityUpdateRetry()
        {
            RestEntity<StreamingEndpointData> data = new StreamingEndpointData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("StreamingEndpoints", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.Update();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestRestEntityDeleteRetry()
        {
            RestEntity<ProgramData> data = new ProgramData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Origins", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestRestEntityExecuteActionAsync()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute(It.IsAny<Uri>(), "POST"))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    throw new NotImplementedException(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage);
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var target = new TestRestEntity(_mediaContext);

            try
            {
                target.ExecuteActionAsyncTest();
            }
            catch (AggregateException ax)
            {
                NotImplementedException x = ax.InnerException as NotImplementedException;
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.Execute(It.IsAny<Uri>(), "POST"), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestRestEntityRefresh()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var fakeResponse = new[] {new StreamingEndpointData {Name = "test"}};
            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute<StreamingEndpointData>(It.IsAny<Uri>()))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var target = new TestRestEntity(_mediaContext);

            target.Refresh();

            dataContextMock.Verify((ctxt) => ctxt.Execute<StreamingEndpointData>(It.IsAny<Uri>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestRestEntitySendOperation()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute(It.IsAny<Uri>(), "POST"))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    throw new NotImplementedException(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage);
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var target = new TestRestEntity(_mediaContext);

            try
            {
                target.SendOperationTest();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.Execute(It.IsAny<Uri>(), "POST"), Times.Exactly(2));
        }

        #endregion Retry Logic tests

    }
}