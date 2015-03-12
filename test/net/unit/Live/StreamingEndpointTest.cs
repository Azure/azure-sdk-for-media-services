using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{
    [TestClass]
    public class StreamingEndpointTest
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestStreamingEndpointCreateRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup(ctxt => ctxt.AddObject("StreamingEndpoints", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.StreamingEndpoints.Create("unittest", 0);
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }
            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestStreamingEndpointCreateFailedRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup(ctxt => ctxt.AddObject("StreamingEndpoints", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.StreamingEndpoints.Create("unittest", 0);
            }
            catch (WebException x)
            {
                dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestDeleteRetry()
        {
            var data = new StreamingEndpointData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup(ctxt => ctxt.AttachTo("StreamingEndpoints", data));
            dataContextMock.Setup(ctxt => ctxt.DeleteObject(data));

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

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestSendDeleteOperationRetry()
        {
            var data = new StreamingEndpointData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup(ctxt => ctxt.AttachTo("StreamingEndpoints", data));
            dataContextMock.Setup(ctxt => ctxt.DeleteObject(data));

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

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
    }
}