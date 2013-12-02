using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    ///This is a test class for LinkCollectionTest and is intended
    ///to contain all LinkCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinkCollectionTest
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod()]
        [Priority(0)]
        [TestCategory("DailyBvtRun")]
        public void LinkCollectionTestInsertRetry()
        {
            var data = new AssetData { };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);
            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var target = new LinkCollection<IAsset, AssetData>(dataContextMock.Object, data, "", new IAsset[] { });

            target.Add(data);

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod()]
        [Priority(0)]
        [TestCategory("DailyBvtRun")]
        public void LinkCollectionTestRemoveRetry()
        {
            var data = new AssetData { };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);
            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var target = new LinkCollection<IAsset, AssetData>(dataContextMock.Object, data, "", new IAsset[] { data });

            target.RemoveAt(0);

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }
    }
}
