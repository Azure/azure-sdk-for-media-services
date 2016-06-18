//-----------------------------------------------------------------------
// <copyright file="AssetUnitTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
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
        public void AssetFileQueryable()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAssetFile file = asset.AssetFiles.Create("test");
            var fetched = _mediaContext.Files.Where(c => c.Id == file.Id).FirstOrDefault();
            Assert.IsNotNull(fetched);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AssetFileCreateWithoutParentShouldFail()
        {
            var fetched = _mediaContext.Files.Create("AssetFileCreateWithoutParentShouldFail");
            Assert.Fail("Expecting exception: asset with id {0} has been created",fetched.Id);
        }

        [TestMethod]
        
        public void AssetFileCreateStorageEncryptedFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.StorageEncrypted);
            var file = asset.AssetFiles.Create("AssetFileCreateStorageEncryptedFile");
            CallUpdateUploadDownloadAndDelete(file, "AssetFileCreateStorageEncryptedFile");
        }

        private static void CallUpdateUploadDownloadAndDelete(IAssetFile file, string name)
        {
            file.Update();
            file.UpdateAsync();  
            var uploadFile = Path.Combine(Path.GetTempPath(), name);
            try
            {
              
                File.CreateText(uploadFile).Close();
                file.Upload(uploadFile);
            }
            finally
            {
                File.Delete(uploadFile);
            }
            file.Download(Path.GetTempFileName());
            file.Delete();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AssetFileCreateShouldThrowOnEmptyFileName()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.CommonEncryptionProtected);
            asset.AssetFiles.Create(String.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AssetFileCreateShouldThrowNullFileName()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.CommonEncryptionProtected);
            asset.AssetFiles.Create(null);
        }

        [TestMethod]
        public void AssetFileCreateCommonEncryptedFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.CommonEncryptionProtected);
            var file = asset.AssetFiles.Create("AssetFileCreateCommonEncryptedFile");

            CallUpdateUploadDownloadAndDelete(file, "AssetFileCreateCommonEncryptedFile");
        }
        [TestMethod]
        public void AssetFileCreateEnvelopeEncryptedFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.EnvelopeEncryptionProtected);
            var file = asset.AssetFiles.Create("AssetFileCreateEnvelopeEncryptedFile");
            CallUpdateUploadDownloadAndDelete(file, "AssetFileCreateEnvelopeEncryptedFile");
        }

        [TestMethod]
        public void AssetFileDownloadUploadThrowsExceptionForFragblob()
        {
            var fragblob= new Mock<AssetFileData>().Object;
            fragblob.Options = 1;

            try
            {
                fragblob.Upload("/foo/bar");
                Assert.Fail();
            }
            catch (NotSupportedException) { }

            try
            {
                fragblob.Download("foo.bar");
                Assert.Fail();
            }
            catch (NotSupportedException) { }
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
            Assert.IsNotNull(asset.Uri);
            IAsset refreshed = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(refreshed);
            //simulating refresh of asset
            ((AssetData)asset).SetMediaContext(_mediaContext);
            Assert.IsNotNull(asset.Locators);
            Assert.IsNotNull(asset.AssetFiles);
            Assert.AreEqual(AssetState.Initialized, asset.State);
            Assert.IsNotNull(asset.ParentAssets);
            Assert.IsNotNull(asset.StorageAccount);
            Assert.IsNotNull(asset.Uri);
            
        }

        [TestMethod]
        public void CreateFileForStorageEncryptedAssetWithMissingStorageKey()
        {
            Task<IAsset> assetTask = _mediaContext.Assets.CreateAsync("Test", AssetCreationOptions.StorageEncrypted, CancellationToken.None);
            IAsset asset = assetTask.Result;
            Assert.AreEqual(1,asset.ContentKeys.Count);
            var key = asset.ContentKeys[0];
            Assert.AreEqual(ContentKeyType.StorageEncryption, key.ContentKeyType);
            //this call will not remove contentkey from collection. Collection need to be converted to iquaryable
            key.Delete();
            //simulating refresh
            ((AssetData)asset).SetMediaContext(_mediaContext);
            Assert.AreEqual(0, asset.ContentKeys.Count);
            bool exception = false;
            try
            {
                var file = asset.AssetFiles.CreateAsync("test", CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
              exception = true;
              Assert.AreEqual(ex.Flatten().InnerException.Message, String.Format(CultureInfo.InvariantCulture, StringTable.StorageEncryptionContentKeyIsMissing, asset.Id));
            }
            Assert.IsTrue(exception, "Expected InvalidOperationException");

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

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void CancelOnAssetFileCreate()
        {
            var mediaContext = Helper.GetMediaDataServiceContextForUnitTests(1000);
            var asset = mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            var numberofFiles = asset.AssetFiles.Count();
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            try
            {
                var task = asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString(), token);
                source.Cancel();
                var result = task.Result;
            }
            catch (AggregateException)
            {
                
                Assert.AreEqual(numberofFiles, asset.AssetFiles.Count());
                throw;
            }
        }

        /// <summary>
        /// Adding tasks using Parallel.For. making sure that task is returned immediately and cancellable
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void ShouldCancelAllAssetFileCreateWhenParallelIsUsed()
        {
            var mediaContext = Helper.GetMediaDataServiceContextForUnitTests(1000);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var asset = mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            var numberofFiles = asset.AssetFiles.Count();
            List<Task> tasks = new List<Task>();
            //Since we have delay on save operation 
            try
            {
                var result = Parallel.For(0,
                    5,
                    i =>
                    {
                        tasks.Add(asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString(), token));
                    });
            }
            catch (AggregateException)
            {
                Assert.Fail("Not expecting to fail in  Parallel.For");
            }

            
            source.Cancel();

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
                //Assert.AreEqual(tasks.Count,ex.InnerExceptions.Count);
                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.IsCanceled);
                }
                Assert.AreEqual(numberofFiles, asset.AssetFiles.Count());
                throw;
            }

        }

        /// <summary>
        /// In this test we are creating tasks within a task.
        /// Previous implementation of AssetFileCreate was passing
        /// </summary>
        [TestMethod]
        public void ShouldCancelWithinATask()
        {
            var mediaContext = Helper.GetMediaDataServiceContextForUnitTests(1000);
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var asset = mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            var numberofFiles = asset.AssetFiles.Count();
            List<Task> tasks = new List<Task>();
            Task.Factory.StartNew(() => {
                                            for (int i = 0; i < 5; i++)
                                            {
                                                tasks.Add(asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString(), token));
                                            }
                                            
            });
            source.Cancel();

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(tasks.Count, ex.InnerExceptions.Count);
                foreach (var task in tasks)
                {
                    Assert.IsTrue(task.IsCanceled);
                }
                Assert.AreEqual(numberofFiles, asset.AssetFiles.Count());
                throw;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateDefaultStorageAccountTryingToCreateAsset()
        {
            var context = Helper.GetMockContextWithNullDefaultStorage();

            AssetCollection collection = new AssetCollection(context);
           
            try
            {
                collection.Create("NullStorage", AssetCreationOptions.StorageEncrypted);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(StringTable.DefaultStorageAccountIsNull,ex.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateDefaultStorageAccountTryingToCreateAssetAsync()
        {
            var context = Helper.GetMockContextWithNullDefaultStorage();

            AssetCollection collection = new AssetCollection(context);

            try
            {
                var task = collection.CreateAsync("NullStorage", AssetCreationOptions.StorageEncrypted,CancellationToken.None);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(StringTable.DefaultStorageAccountIsNull, ex.Message);
                throw;
            }

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ValidateDefaultStorageAccountTryingToAddAssetToOutPutAssetCollection()
        {
            var context = Helper.GetMockContextWithNullDefaultStorage();

            OutputAssetCollection collection = new OutputAssetCollection(Mock.Of<TaskData>(),new List<IAsset>(), context);
            try
            {
                collection.AddNew("NullStorage", AssetCreationOptions.StorageEncrypted);
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(StringTable.DefaultStorageAccountIsNull, ex.Message);
                throw;
            }

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
        public void TestAssetDeleteRetryWithKeepAzureContainerOption()
        {
            var data = new AssetData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Assets", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var result = data.DeleteAsync(true).Result;

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