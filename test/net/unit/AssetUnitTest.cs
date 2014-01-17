//-----------------------------------------------------------------------
// <copyright file="TestMediaServicesClassFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.UnitTests
{
    [TestClass]
    public class AssetUnitTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void AssetCRUDWithEmptyFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAssetFile file = asset.AssetFiles.Create("test");
            Assert.IsNotNull(asset);
            Assert.IsNotNull(asset.AssetFiles);
            Assert.AreEqual(1, asset.AssetFiles.Count());
            file.ContentFileSize = 100;
            file.Update();
            file.Delete();
            Assert.IsNotNull(asset.AssetFiles);
            Assert.AreEqual(0, asset.AssetFiles.Count());
            asset.Delete();
            Assert.IsNull(_mediaContext.Assets.Where(c=>c.Id == asset.Id).FirstOrDefault());

        }

        [TestMethod]
        public void AssetCreateAsync()
        {
            Task<IAsset> assetTask = _mediaContext.Assets.CreateAsync("Test", AssetCreationOptions.None, CancellationToken.None);
            IAsset asset = assetTask.Result;
            Assert.IsNotNull(asset);
            Assert.IsNotNull(asset.Locators);
            Assert.IsNotNull(asset.AssetFiles);
            Assert.AreEqual(AssetState.Initialized, asset.State);
            Assert.IsNotNull(asset.ParentAssets);
            Assert.IsNotNull(asset.StorageAccount);
            IAsset refreshed = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(refreshed);
        }

        [TestMethod]
        public void AssetCreateAsyncStorageEncrypted()
        {
            Task<IAsset> assetTask = _mediaContext.Assets.CreateAsync("Test", AssetCreationOptions.StorageEncrypted, CancellationToken.None);
            IAsset asset = assetTask.Result;
            Assert.IsNotNull(asset);
        }

        [TestMethod]
        public void AssetSelectAll()
        {
            List<IAsset> asset = _mediaContext.Assets.ToList();
            Assert.IsNotNull(asset);
        }

        [TestMethod]
        public void VerifyStubedData()
        {
            var asset = _mediaContext.Assets.FirstOrDefault();
            Assert.IsNotNull(asset);
            Assert.IsNotNull(asset.Locators);
            Assert.AreEqual(1,asset.Locators.Count);
            Assert.IsNotNull(asset.AssetFiles);
            Assert.AreEqual(1, asset.AssetFiles.Count());
        }

        #region Retry Logic tests

        [TestMethod]
        public void TestAssetCreateRetry()
        {
            var expected = new AssetData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Assets", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            IAsset asset = _mediaContext.Assets.Create("Empty", "some storage", AssetCreationOptions.None);
            Assert.AreEqual(expected.Name, asset.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void TestAssetCreateFailedRetry()
        {
            var expected = new AssetData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Assets", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                IAsset asset = _mediaContext.Assets.Create("Empty", "some storage", AssetCreationOptions.None);
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
        [ExpectedException(typeof(WebException))]
        public void TestAssetCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new AssetData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Assets", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                IAsset asset = _mediaContext.Assets.Create("Empty", "some storage", AssetCreationOptions.None);
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
        public void TestAssetUpdateRetry()
        {
            var data = new AssetData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Assets", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        public void TestAssetDeleteRetry()
        {
            var data = new AssetData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Assets", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        public void TestAssetGetContentKeysRetry()
        {
            var data = new AssetData { Name = "testData", Id = "testId" };

            var dataContextMock = TestMediaServicesClassFactory.CreateLoadPropertyMockConnectionClosed(2, data);

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var keys = ((IAsset)data).ContentKeys;

            dataContextMock.Verify((ctxt) => ctxt.LoadProperty(data, "ContentKeys"), Times.Exactly(2));
        }

        #endregion Retry Logic tests
       
    }
}