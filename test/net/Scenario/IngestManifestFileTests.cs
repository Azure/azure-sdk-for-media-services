using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Moq;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class IngestManifestFileTests
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
        public void TestIngestManifestFileCreateRetry()
        {
            var expected = new IngestManifestFileData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("IngestManifestFiles", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var parent = new IngestManifestAssetData { Asset = new AssetData {} };
            var ingestManifestFiles = new IngestManifestFileCollection(_mediaContext, parent);

            var tempFile = "a:\\wherever\\whatever.mp3";
            IIngestManifestFile actual = ingestManifestFiles.Create(tempFile);

            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestIngestManifestFileCreateFailedRetry()
        {
            var expected = new IngestManifestFileData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("IngestManifestFiles", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var parent = new IngestManifestAssetData { Asset = new AssetData { } };
            var ingestManifestFiles = new IngestManifestFileCollection(_mediaContext, parent);

            var tempFile = "a:\\wherever\\whatever.mp3";
            try
            {
                IIngestManifestFile actual = ingestManifestFiles.Create(tempFile);
            }
            catch (AggregateException ax)
            {
                WebException x = (WebException)ax.GetBaseException();
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestIngestManifestFileCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new IngestManifestFileData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("IngestManifestFiles", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var parent = new IngestManifestAssetData { Asset = new AssetData { } };
            var ingestManifestFiles = new IngestManifestFileCollection(_mediaContext, parent);

            var tempFile = "a:\\wherever\\whatever.mp3"; 
            try
            {
                IIngestManifestFile actual = ingestManifestFiles.Create(tempFile);

                Assert.AreEqual(expected.Name, actual.Name);
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
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
        public void TestIngestManifestFileDeleteRetry()
        {
            var data = new IngestManifestFileData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("IngestManifestFiles", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
        #endregion Retry Logic tests
    }
}