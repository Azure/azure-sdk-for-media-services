using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Net;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    ///This is a test class for IngestManifestAssetCollectionTest and is intended
    ///to contain all IngestManifestAssetCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IngestManifestAssetTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        #region Retry Logic tests

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [TestCategory("Bvt")]
        public void TestIngestManifestAssetCreateRetry()
        {
            var asset = new AssetData { Name = "testData", Id = "testId" };
            var expected = new IngestManifestAssetData { Asset = asset };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("IngestManifestAssets", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var parent = new IngestManifestData { };
            var target = new IngestManifestAssetCollection(_mediaContext, parent);

            var actual = target.CreateAsync(asset, CancellationToken.None).Result;

            Assert.AreEqual(expected.Asset.Name, actual.Asset.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [TestCategory("Bvt")]
        public void TestIngestManifestFileDeleteRetry()
        {
            var data = new IngestManifestAssetData { };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("IngestManifestAssets", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        #endregion Retry Logic tests
    }
}
