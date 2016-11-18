//-----------------------------------------------------------------------
// <copyright file="AssetTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class AssetTests
    {
        private CloudMediaContext _mediaContext;
        private double _downloadProgress;
        private string _smallWmv;
        private string _bbcMp4;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv);
            _bbcMp4 = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext,
                WindowsAzureMediaServicesTestConfiguration.BBCmp4);
            _downloadProgress = 0;
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestQueries()
        {
            _mediaContext.Assets.FirstOrDefault();

            var asset1 = _mediaContext
                .Assets
                .OrderByDescending(c => c.Created)
                .Where(a => a.Name.Length > 1)
                .Skip(10)
                .Take(5)
                .First();

            var asset2 = _mediaContext
                .Assets
                .Where(a => a.Name.Length > 1)
                .Skip(10)
                .Take(5)
                .First();

            var asset3 = _mediaContext
                .Assets
                .Where(a => a.Name.Length > 1)
                .OrderByDescending(c => c.Created)
                .Skip(10)
                .Take(5)
                .First();
        }

        /// <summary>
        /// Known issue with mime-type detection
        /// </summary>
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateAssetFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.CreateAsync(Path.GetFileName(_smallWmv), CancellationToken.None).Result;
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
            bool transferCompletedFired = false;
            blobTransferClient.TransferCompleted += (sender, args) =>
            {
                transferCompletedFired = true;
                Assert.AreEqual(BlobTransferType.Upload, args.TransferType, "file.UploadAsync Transfer completed expected BlobTransferType is Upload");
            };
            file.UploadAsync(_smallWmv, blobTransferClient, locator, CancellationToken.None).Wait();
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.IsTrue(transferCompletedFired, "TransferCompleted event has not been fired");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

            Assert.AreEqual(1, asset.AssetFiles.Count());
            IAssetFile queriedAssetFile = asset.AssetFiles.First();

            Assert.IsTrue(string.Compare(Path.GetFileName(_smallWmv), queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
            FileInfo assetFileInfo = new FileInfo(_smallWmv);
            Assert.AreEqual(assetFileInfo.Length, queriedAssetFile.ContentFileSize);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateAssetFileFromEmptyStream()
        {
            var stream = new MemoryStream(new byte[0]);
            IAsset asset = _mediaContext.Assets.Create("Empty_MS", AssetCreationOptions.StorageEncrypted);

            var name = "my_custom_name.wmv";
            IAssetFile file = asset.AssetFiles.CreateAsync(name, CancellationToken.None).Result;
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
            bool transferCompletedFired = false;
            blobTransferClient.TransferCompleted += (sender, args) =>
            {
                transferCompletedFired = true;
                Assert.AreEqual(BlobTransferType.Upload, args.TransferType, "file.UploadAsync Transfer completed expected BlobTransferType is Upload");
            };
            file.UploadAsync(stream, blobTransferClient, locator, CancellationToken.None).Wait();
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.IsTrue(transferCompletedFired, "TransferCompleted event has not been fired");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

            Assert.AreEqual(1, asset.AssetFiles.Count());
            IAssetFile queriedAssetFile = asset.AssetFiles.First();

            Assert.IsTrue(string.Compare(name, queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
            Assert.AreEqual(0, queriedAssetFile.ContentFileSize);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateAssetFileFromSmallStream()
        {
            // get the path to the media file 
            // read the content into a stream 
            // upload it from the stream 
            using (var fileStream = new FileStream(_smallWmv, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IAsset asset = _mediaContext.Assets.Create("Small_FS", AssetCreationOptions.StorageEncrypted);

                var name = "my_custom_name.wmv";
                IAssetFile file = asset.AssetFiles.CreateAsync(name, CancellationToken.None).Result;
                IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
                ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
                BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
                bool transferCompletedFired = false;
                blobTransferClient.TransferCompleted += (sender, args) =>
                {
                    transferCompletedFired = true;
                    Assert.AreEqual(BlobTransferType.Upload, args.TransferType, "file.UploadAsync Transfer completed expected BlobTransferType is Upload");
                };
                file.UploadAsync(fileStream, blobTransferClient, locator, CancellationToken.None).Wait();
                Assert.IsNotNull(asset, "Asset should be non null");
                Assert.IsTrue(transferCompletedFired, "TransferCompleted event has not been fired");
                Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
                Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

                Assert.AreEqual(1, asset.AssetFiles.Count());
                IAssetFile queriedAssetFile = asset.AssetFiles.First();

                Assert.IsTrue(string.Compare(name, queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
                FileInfo assetFileInfo = new FileInfo(_smallWmv);
                Assert.AreEqual(assetFileInfo.Length, queriedAssetFile.ContentFileSize);

                VerifyAndDownloadAsset(asset, 1, _smallWmv, true, performStorageSdkDownloadVerification:false);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\BBCW_1920x1080_30sec.mp4", "Media")]
        public void ShouldCreateAssetFileFromBigStream()
        {
            // get the path to the media file 
            // read the content into a stream 
            // upload it from the stream 
            using (var fileStream = new FileStream(_bbcMp4, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IAsset asset = _mediaContext.Assets.Create("FS_Big", AssetCreationOptions.StorageEncrypted);

                var name = "my_custom_name.mp4";
                IAssetFile file = asset.AssetFiles.CreateAsync(name, CancellationToken.None).Result;
                IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
                ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
                BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
                bool transferCompletedFired = false;
                blobTransferClient.TransferCompleted += (sender, args) =>
                {
                    transferCompletedFired = true;
                    Assert.AreEqual(BlobTransferType.Upload, args.TransferType, "file.UploadAsync Transfer completed expected BlobTransferType is Upload");
                };
                file.UploadAsync(fileStream, blobTransferClient, locator, CancellationToken.None).Wait();
                Assert.IsNotNull(asset, "Asset should be non null");
                Assert.IsTrue(transferCompletedFired, "TransferCompleted event has not been fired");
                Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
                Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

                Assert.AreEqual(1, asset.AssetFiles.Count());
                IAssetFile queriedAssetFile = asset.AssetFiles.First();

                Assert.IsTrue(string.Compare(name, queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
                FileInfo assetFileInfo = new FileInfo(_bbcMp4);
                Assert.AreEqual(assetFileInfo.Length, queriedAssetFile.ContentFileSize);

                VerifyAndDownloadAsset(asset, 1, _bbcMp4, true, performStorageSdkDownloadVerification: false);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateAssetFileFromNetworkStream()
        {
            // upload file to blob
            // download file from blob into stream to pass to asset creator
            var storageAccount = CloudStorageAccount.Parse(WindowsAzureMediaServicesTestConfiguration.ClientStorageConnectionString);
            var containername = Guid.NewGuid().ToString();
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containername);
            container.CreateIfNotExists();

            var blobReference = "ShouldCreateAssetFileFromNetworkStream_" + Path.GetFileName(_smallWmv);
            var blob = container.GetBlockBlobReference(blobReference);
            blob.UploadFromFile(_smallWmv, FileMode.Open);

            var downloadBlob = container.GetBlockBlobReference(blobReference);

            IAsset asset = _mediaContext.Assets.Create("Empty_NS", AssetCreationOptions.StorageEncrypted);
            // try giving it a different name here, like a blob URI 
            var name = "my_custom_name.wmv";
            IAssetFile file = asset.AssetFiles.CreateAsync(name, CancellationToken.None).Result;
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10),
                AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
            bool transferCompletedFired = false;
            blobTransferClient.TransferCompleted += (sender, args) =>
            {
                transferCompletedFired = true;
                Assert.AreEqual(BlobTransferType.Upload, args.TransferType,
                    "file.UploadAsync Transfer completed expected BlobTransferType is Upload");
            };
            using (var stream = downloadBlob.OpenRead())
            {
                file.UploadAsync(stream, blobTransferClient, locator, CancellationToken.None).Wait();
            }

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.IsTrue(transferCompletedFired, "TransferCompleted event has not been fired");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

            Assert.AreEqual(1, asset.AssetFiles.Count());
            IAssetFile queriedAssetFile = asset.AssetFiles.First();

            Assert.IsTrue(string.Compare(name, queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
            FileInfo assetFileInfo = new FileInfo(_smallWmv);
            Assert.AreEqual(assetFileInfo.Length, queriedAssetFile.ContentFileSize);

            VerifyAndDownloadAsset(asset, 1, _smallWmv, true, false);
           
            // cleanup; what if the assert fails
            downloadBlob.Delete();
            container.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [TestCategory("Bvt")]
        [ExpectedException(typeof(ArgumentException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldThrowArgumentExceptionOnAssetUploadWhenLocalFileNameNotMatchingAssetFileName()
        {
            IAsset asset = _mediaContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString(), CancellationToken.None).Result;
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            try
            {
                file.UploadAsync(_smallWmv, _mediaContext.MediaServicesClassFactory.GetBlobTransferClient(), locator, CancellationToken.None).Wait();
            }
            catch (ArgumentException ex)
            {
                var finfo = new FileInfo(_smallWmv);
                Assert.IsTrue(ex.Message.Contains("File name mismatch detected"));
                Assert.IsTrue(ex.Message.Contains(finfo.Name.ToUpperInvariant()));
                throw;
            }
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateSingleFileAssetWithNoLocatorUsingOveloadSync()
        {
            IAsset asset = _mediaContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.Create(Path.GetFileName(_smallWmv));
            file.Upload(_smallWmv);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateSingleFileAssetFromStreamWithNoLocatorUsingOveloadSync()
        {
            var name = Path.GetFileName(_smallWmv);
            IAsset asset = _mediaContext.Assets.Create("Empty_FS", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.Create(name);
            using (var stream = new FileStream(_smallWmv, FileMode.Open, FileAccess.Read))
            {
                file.Upload(stream);
            }

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");

            Assert.AreEqual(1, asset.AssetFiles.Count());
            IAssetFile queriedAssetFile = asset.AssetFiles.First();

            Assert.IsTrue(string.Compare(name, queriedAssetFile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
            FileInfo assetFileInfo = new FileInfo(_smallWmv);
            Assert.AreEqual(assetFileInfo.Length, queriedAssetFile.ContentFileSize);

            VerifyAndDownloadAsset(asset, 1, _smallWmv, true, false);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [TestCategory("Bvt")]
        public void ShouldCreateAssetFileInfoWithoutUploadingFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            VerifyAsset(asset);
            const string name = "FileName.txt";
            IAssetFile file = asset.AssetFiles.CreateAsync(name, CancellationToken.None).Result;
            Assert.IsNotNull(file.Id);
            Assert.AreNotEqual(String.Empty, file.Id);
            Assert.AreEqual(asset.Id, file.ParentAssetId);
            Assert.AreEqual(name, file.Name);
            Assert.AreEqual(FileEncryption.SchemeName, file.EncryptionScheme);
            Assert.IsTrue(file.IsEncrypted);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [TestCategory("Bvt")]
        public void ShouldNotHaveLocatorsAfterAssetCreation()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(0, asset.Locators.Count);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateEncryptedInitilizedAsset()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.StorageEncrypted);
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(0, asset.AssetFiles.Count(), "Asset has files");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Expecting initilized state");

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);

            IAssetFile file = asset.AssetFiles.Create(Path.GetFileName(_smallWmv));

            Task task = file.UploadAsync(_smallWmv,
                                         _mediaContext.MediaServicesClassFactory.GetBlobTransferClient(),
                                         locator,
                                         CancellationToken.None);

            task.Wait();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(!task.IsFaulted);
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.AreEqual(asset.Name, refreshedAsset.Name);
            Assert.AreEqual(AssetState.Initialized, refreshedAsset.State);
            Assert.AreEqual(1, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset, 1, _smallWmv, true, false);
            ContentKeyTests.VerifyFileAndContentKeyMetadataForStorageEncryption(refreshedAsset, _mediaContext);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateEncryptedInitilizedAssetFromStream()
        {
            IAsset asset = _mediaContext.Assets.Create("Test_FS_1", AssetCreationOptions.StorageEncrypted);
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(0, asset.AssetFiles.Count(), "Asset has files");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Expecting initilized state");

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);

            var name = Path.GetFileName(_smallWmv);

            IAssetFile file = asset.AssetFiles.Create(name);

            using (var fs = new FileStream(_smallWmv, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var task = file.UploadAsync(fs,
                    _mediaContext.MediaServicesClassFactory.GetBlobTransferClient(),
                    locator,
                    CancellationToken.None);


                task.Wait();
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(!task.IsFaulted);
                locator.Delete();
                policy.Delete();
                IAsset refreshedAsset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
                Assert.AreEqual(asset.Name, refreshedAsset.Name);
                Assert.AreEqual(AssetState.Initialized, refreshedAsset.State);
                Assert.AreEqual(1, refreshedAsset.AssetFiles.Count(), "file count wrong");
                VerifyAndDownloadAsset(refreshedAsset, 1, fs, true, false);
                ContentKeyTests.VerifyFileAndContentKeyMetadataForStorageEncryption(refreshedAsset, _mediaContext);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateEmptyNoneEncryptedAssetUploadFileAndDownloadIt()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);

            VerifyAsset(asset);

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv, _mediaContext);

            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(1, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset, 1,_smallWmv,true);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [Priority(1)]
        public void ShouldCreateEmptyAssetUploadTwoFilesSetPrimaryAndDownloadFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);

            VerifyAsset(asset);

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv, _mediaContext);
            //asset = _dataContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            UploadFile(locator, asset, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, _mediaContext);

            Assert.AreEqual(2, asset.AssetFiles.Count());
            IAssetFile assetFile = asset.AssetFiles.ToList()[1];
            assetFile.IsPrimary = true;
            assetFile.Update();
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(2, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset, 2,_smallWmv,false);
        }



        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ShouldThrowCreatingAssetFileWithMissingFile()
        {
            try
            {
                CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.BadSmallWmv, AssetCreationOptions.StorageEncrypted);
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void CreateAssetAndUpload4FilesUsingSyncCall()
        {
            const int expected = 4;
            CreateAssetAndUploadNFilesSync(expected,_mediaContext,_smallWmv);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [Ignore]
        public void ShouldCreateAssetAndUpload10FilesUsingSyncCall()
        {
            const int expected = 10;
            CreateAssetAndUploadNFilesSync(expected,_mediaContext,_smallWmv);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [Ignore]
        public void ShouldCreateAssetAndUpload4FilesUsingAsyncCall()
        {
            const int expected = 4;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected,_mediaContext,_smallWmv);
            Assert.AreEqual(expected, _mediaContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [Ignore]
        public void ShouldCreateAssetAndUpload10FilesUsingAsyncCall()
        {
            const int expected = 10;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected,_mediaContext,_smallWmv);
            Assert.AreEqual(expected, _mediaContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [Ignore]
        public void ShouldCreateAssetAndUploadAndDownload10FilesUsingAsyncCall()
        {
            const int expected = 10;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected, _mediaContext,_smallWmv);
            Assert.AreEqual(expected, _mediaContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
            IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create("SdkDownload", TimeSpan.FromHours(12), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, accessPolicy);
            var blobTransfer = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();

            var downloads = new List<Task>();
            var paths = new List<string>();
            try
            {
                foreach (IAssetFile file in _mediaContext.Files.Where(c => c.ParentAssetId == asset.Id))
                {
                    string path = Guid.NewGuid().ToString();
                    paths.Add(path);
                    downloads.Add(file.DownloadAsync(path, blobTransfer, locator, CancellationToken.None));
                }
                Task.WaitAll(downloads.ToArray());

                int i = 0;
                foreach (Task download in downloads)
                {
                    Assert.IsTrue(download.IsCompleted);
                    Assert.IsFalse(download.IsCanceled);
                    Assert.IsNull(download.Exception);
                    Assert.IsTrue(File.Exists(paths[i]));
                    i++;
                }
            }
            finally
            {
                paths.ForEach(f => { if (File.Exists(f)) File.Delete(f); });
            }
        }

        [Ignore]
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        public void ShouldCreateAssetAndCreate100FilesUsingAsyncCall()
        {
            IAsset asset = _mediaContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
            VerifyAsset(asset);
            var files = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                files.Add(asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString() + ".tmp", CancellationToken.None));
            }
            Task.WaitAll(files.ToArray());
            Assert.AreEqual(100, files.Where(c => c.IsCompleted).Count());
            Assert.AreEqual(0, files.Where(c => c.IsFaulted).Count());
            Assert.AreEqual(0, files.Where(c => c.IsCanceled).Count());
            Assert.AreEqual(0, files.Where(c => c.Exception != null).Count());
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [TestCategory("Bvt")]
        public void CreateAssetWithUniqueAlternateIdAndFilterByIt()
        {
            CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset createdAsset = CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            createdAsset.AlternateId = Guid.NewGuid().ToString();
            createdAsset.Update();
            int assetCount = Enumerable.Count(_mediaContext.Assets.Where(c => c.AlternateId == createdAsset.AlternateId));

            Assert.AreEqual(1, assetCount, "Asset Count not right");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceQueryException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldNotReturnAssetsForEmptyId()
        {
            IAsset createdAsset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset foundAsset = _mediaContext.Assets.Where(c => c.Id == string.Empty).FirstOrDefault();
            Assert.IsNull(foundAsset, "should not found asset");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldQueryAssetsByNameWithContains()
        {
            IAsset createdAsset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset foundAsset = _mediaContext.Assets.Where(c => c.Name.Contains(createdAsset.Name)).FirstOrDefault();
            Assert.IsNotNull(foundAsset);

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldModifyAssetFile()
        {
            string assetId;
            {
                IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);
                Assert.IsNotNull(asset, "Asset should be non null");
                asset.Name = "New Name";
                asset.Update();
                Assert.AreEqual("New Name", asset.Name, "Name is wrong");
                assetId = asset.Id;
            }

            {
                IAsset asset = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext().Assets.Where(c => c.Id == assetId).FirstOrDefault();

                Assert.IsNotNull(asset, "Asset should be non null");
                Assert.AreEqual("New Name", asset.Name, "Name is wrong");
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadAssetFile()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);
            VerifyAndDownloadAsset(asset, 1,_smallWmv,true);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        public void ShouldDownloadSameAssetFile2TimesIdenticallyAsStorageSDK()
        {

            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            VerifyAsset(asset);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv, _mediaContext);
            UploadFile(locator, asset, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, _mediaContext);


            IAssetFile assetFile = asset.AssetFiles.FirstOrDefault();
            Assert.IsNotNull(assetFile);
            assetFile.IsPrimary = true;
            assetFile.Update();
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(2, refreshedAsset.AssetFiles.Count(), "file count wrong");


            for (int i = 0; i < 2; i++)
            {
                VerifyAndDownloadAsset(refreshedAsset, 2,_smallWmv,false);
            }

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDownloadCommonEncryptionProtectedAssetFile()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.CommonEncryptionProtected);
            VerifyAndDownloadAsset(asset, 1,_smallWmv,true);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDownloadEnvelopeEncryptionProtectedAssetFile()
        {
            IAsset asset = _mediaContext.Assets.Create(_smallWmv, AssetCreationOptions.EnvelopeEncryptionProtected);
            string name = Path.GetFileName(_smallWmv);
            IAssetFile file = asset.AssetFiles.Create(name);
            file.Upload(_smallWmv);
            VerifyAndDownloadAsset(asset, 1,_smallWmv,true);
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDownloadEnvelopeEncryptionProtectedAssetFileCreatedFromStream()
        {
            IAsset asset = _mediaContext.Assets.Create(_smallWmv, AssetCreationOptions.EnvelopeEncryptionProtected);
            string name = Path.GetFileName(_smallWmv);
            IAssetFile file = asset.AssetFiles.Create(name);
            using (var stream = new FileStream(_smallWmv, FileMode.Open, FileAccess.Read))
            {
                file.Upload(stream);
                VerifyAndDownloadAsset(asset, 1, stream, true);
            }

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadIngestEncryptedAssetFile()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);

            VerifyAndDownloadAsset(asset, 1,_smallWmv,true,false);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteAssetsForFinishedJob()
        {
            IAsset asset;
            IJob job;
            IAsset outasset = RunJobAndGetOutPutAsset("AssetTests_ShouldDeleteAssetsForFinishedJob", out asset, out job);
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }
            asset.Delete();
            outasset.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteParentAssetAndGetParentCount()
        {
            IAsset asset;
            IJob job;
            IAsset outasset = RunJobAndGetOutPutAsset("AssetTests_ShouldDeleteParentAssetAndGetParentCount", out asset, out job);
            Assert.AreEqual(1, outasset.ParentAssets.Count, "Unexpected number of parents assets");
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }
            asset.Delete();
            outasset = _mediaContext.Assets.Where(c => c.Id == outasset.Id).FirstOrDefault();
            Assert.AreEqual(1, outasset.ParentAssets.Count, "Unexpected number of parents assets");
            outasset.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldNotThrowTryingToDeleteAssetWithActiveLocators()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create("ReadOnly", TimeSpan.FromMinutes(60), AccessPermissions.Read);
            ILocator sasLocator = _mediaContext.Locators.CreateSasLocator(asset, accessPolicy);
            ILocator originLocator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, accessPolicy);

            Assert.IsNotNull(_mediaContext.Locators.Where(l => l.Id == sasLocator.Id).SingleOrDefault());
            Assert.IsNotNull(_mediaContext.Locators.Where(l => l.Id == originLocator.Id).SingleOrDefault());

            asset = _mediaContext.Assets.Where(a => a.Id == asset.Id).Single();
            Assert.AreEqual(3, asset.Locators.Count);
            Assert.IsTrue(asset.Locators.Any(l => l.Id == sasLocator.Id));
            Assert.IsTrue(asset.Locators.Any(l => l.Id == originLocator.Id));

            asset.Delete();
            Assert.IsNull(_mediaContext.Locators.Where(l => l.Id == sasLocator.Id).SingleOrDefault());
            Assert.IsNull(_mediaContext.Locators.Where(l => l.Id == originLocator.Id).SingleOrDefault());
        }

        [TestMethod]
        [DeploymentItem(@".\Media\SmallMP41.mp4", "Content")]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        public void ShouldCreateAssetWithSingleFile()
        {
            string assetFilePath = @"Content\SmallMP41.mp4";

            //In this case isPrimary is not set in the asset by passing false to CreateAsset.
            IAsset asset = CreateAsset(_mediaContext, Path.GetFullPath(assetFilePath), AssetCreationOptions.None,false);

            Assert.AreEqual(AssetState.Initialized, asset.State);
            Assert.AreEqual(1, asset.AssetFiles.Count());
            Assert.AreEqual(1, asset.Locators.Count);

            IAssetFile assetFile = asset.AssetFiles.Single();
            Assert.AreNotEqual(asset.AssetType,AssetType.Unknown);
            Assert.AreEqual(Path.GetFileName(assetFilePath), assetFile.Name);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@".\Resources\TestFiles", "TestFiles")]
        public void ShouldCreateAssetAsyncWithMultipleFiles()
        {
            string[] files = Directory.GetFiles("TestFiles");

            IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            var blobclient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();

            var tasks = new List<Task>();
            foreach (string filePath in files)
            {
                var info = new FileInfo(filePath);
                IAssetFile file = asset.AssetFiles.Create(info.Name);
                tasks.Add(file.UploadAsync(filePath, blobclient, locator, CancellationToken.None));
            }

            Task.WaitAll(tasks.ToArray());
            Assert.AreEqual(AssetState.Initialized, asset.State);
            Assert.AreEqual(files.Length, asset.AssetFiles.Count());
            Assert.AreEqual(1, asset.Locators.Count);


            foreach (IAssetFile assetFile in asset.AssetFiles)
            {
                Assert.IsTrue(files.Any(f => Path.GetFileName(f).Equals(assetFile.Name, StringComparison.OrdinalIgnoreCase)));
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldReportProgressForFile()
        {
            var fileName = _smallWmv;
            bool reportedProgress = false;
            IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            var info = new FileInfo(fileName);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            BlobTransferClient blobTransferClient = _mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
            blobTransferClient.TransferProgressChanged += (s, e) =>
                {
                    Assert.AreEqual(info.Name, e.SourceName);
                    Assert.IsTrue(e.BytesTransferred <= e.TotalBytesToTransfer);
                    reportedProgress = true;
                };

            file.UploadAsync(fileName, blobTransferClient, locator, CancellationToken.None).Wait();
            Assert.IsTrue(reportedProgress);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [TestCategory("Bvt")]
        public void ShouldUpdateAssetNameAndAlternateId()
        {
            string fileName = @"Content\interview.wmv";

            IAsset asset = CreateAsset(_mediaContext, Path.GetFullPath(fileName), AssetCreationOptions.CommonEncryptionProtected);

            Assert.IsNull(asset.AlternateId);
            string newAssetName = "New Asset Name";
            string alternateId = Guid.NewGuid().ToString();

            asset.Name = newAssetName;
            asset.AlternateId = alternateId;

            asset.Update();

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            IAsset retrievedAsset = newContext.Assets.Where(a => a.Id == asset.Id).Single();

            Assert.AreEqual(newAssetName, retrievedAsset.Name);
            Assert.AreEqual(alternateId, retrievedAsset.AlternateId);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteAsset()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);


            Assert.AreEqual(AssetState.Initialized, asset.State);
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }

            asset.Delete();

            Assert.IsNull(_mediaContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            Assert.IsNull(newContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDeleteAssetAndKeepAzureContainer()
        {
            IAsset asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);
            Assert.AreEqual(AssetState.Initialized, asset.State);
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }

            var result = asset.DeleteAsync(true).Result;

            Assert.IsNull(_mediaContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            Assert.IsNull(newContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(WindowsAzureMediaServicesTestConfiguration.ClientStorageConnectionString);
            string containername = asset.Id.Replace("nb:cid:UUID:", "asset-");
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containername);
            Assert.IsTrue(container.Exists(), "Asset container {0} can't be found", container);


        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDeleteEmptyAsset()
        {
            IAsset asset = _mediaContext.Assets.Create("ShouldDeleteEmptyAsset", AssetCreationOptions.None);


            Assert.AreEqual(AssetState.Initialized, asset.State);
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }

            asset.Delete();

            Assert.IsNull(_mediaContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            Assert.IsNull(newContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [TestCategory("Bvt")]
        public void ShouldSetContentFileSizeOnAssetFileWithoutUpload()
        {
            IAsset asset = _mediaContext.Assets.Create("test", AssetCreationOptions.None);
            IAssetFile fileInfo = asset.AssetFiles.Create("test.txt");
            int expected = 0;
            Assert.AreEqual(expected, fileInfo.ContentFileSize, "Unexpected ContentFileSize value");
            expected = 100;
            fileInfo.ContentFileSize = expected;
            fileInfo.Update();
            IAssetFile refreshedFile = _mediaContext.Files.Where(c => c.Id == fileInfo.Id).FirstOrDefault();
            Assert.IsNotNull(refreshedFile);
            Assert.AreEqual(expected, refreshedFile.ContentFileSize, "ContentFileSize Mismatch after Update");

            //Double check with new context
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            refreshedFile = _mediaContext.Files.Where(c => c.Id == fileInfo.Id).FirstOrDefault();
            Assert.IsNotNull(refreshedFile);
            Assert.AreEqual(expected, refreshedFile.ContentFileSize, "ContentFileSize Mismatch after Update");
        }


        #region Helper/utility methods

        public static IAsset CreateAsset(CloudMediaContext datacontext, string filePath, AssetCreationOptions options, bool isSetPrimary = true)
        {
            IAsset asset = datacontext.Assets.Create(Guid.NewGuid().ToString(), options);
            IAccessPolicy policy = datacontext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = datacontext.Locators.CreateSasLocator(asset, policy);
            var info = new FileInfo(filePath);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            BlobTransferClient blobTransferClient = datacontext.MediaServicesClassFactory.GetBlobTransferClient();
            blobTransferClient.NumberOfConcurrentTransfers = 5;
            blobTransferClient.ParallelTransferThreadCount = 5;
            file.UploadAsync(filePath,
                             blobTransferClient,
                             locator,
                             CancellationToken.None).Wait();
            if (isSetPrimary)
            {
                file.IsPrimary = true;
                file.Update();
            }
            return asset;
        }


        /// <summary>
        /// Verifies the and download asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="expectedFileCount">The expected file count.</param>
        /// <param name="inputFile">input file to be validated against the downloaded file</param>
        /// <param name="inputFileValidation">if set to <c>true</c> performs input and WAMS downloaded file validation.</param>
        /// <param name="performStorageSdkDownloadVerification">if set to <c>true</c> also perform storage SDK download verification.</param>
        private void VerifyAndDownloadAsset(IAsset asset, int expectedFileCount, string inputFile, bool inputFileValidation, bool performStorageSdkDownloadVerification = true)
        {
            using (var fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                VerifyAndDownloadAsset(asset, expectedFileCount, fs, inputFileValidation, performStorageSdkDownloadVerification);
            }
        }

        private void VerifyAndDownloadAsset(IAsset asset, int expectedFileCount, Stream inputStream, bool inputFileValidation, bool performStorageSdkDownloadVerification = true)
        {
            Assert.AreEqual(expectedFileCount, asset.AssetFiles.Count(), "file count wrong");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(WindowsAzureMediaServicesTestConfiguration.ClientStorageConnectionString);
            string containername = asset.Id.Replace("nb:cid:UUID:", "asset-");
            string assetUri = asset.Uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containername);
            Assert.IsTrue(container.Exists(), "Asset container {0} can't be found", container);

            foreach (var assetFile in asset.AssetFiles)
            {
                string downloadPathForWamsSdk = Guid.NewGuid().ToString();
                string downloadPathForStorageSdk = Guid.NewGuid().ToString();

                try
                {
                    var blob = container.GetBlobReferenceFromServer(assetFile.Name);
                    Assert.IsTrue(blob.Exists(), "Blob for asset file is not found in corresponding container");
                    blob.FetchAttributes();

                    //Downloading using WAMS SDK
                    assetFile.DownloadProgressChanged += AssetTests_OnDownloadProgress;
                    assetFile.Download(downloadPathForWamsSdk);
                    assetFile.DownloadProgressChanged -= AssetTests_OnDownloadProgress;
                    Assert.AreEqual(100, _downloadProgress);

                    string hashValueForWAMSSDKDownload = GetHashValueForFileMd5CheckSum(downloadPathForWamsSdk);
                    string hashValueForInputFile = GetHashValueForStreamMd5CheckSum(inputStream);
                    if (inputFileValidation)
                    {
                        Assert.AreEqual(hashValueForWAMSSDKDownload, hashValueForInputFile,
                            "MD5 CheckSums for WAMS uploaded and downloaded file are different");
                    }

                    //Comparing checksum if it is present
                    if ((asset.Options & AssetCreationOptions.StorageEncrypted) == 0 && blob.Properties.ContentMD5 != null)
                    {
                        //Assert.AreEqual(hashValueForlocalSourceFile, blob.Properties.ContentMD5, "MD5 CheckSums between blob file and source file  are different");
                        Assert.AreEqual(hashValueForWAMSSDKDownload, blob.Properties.ContentMD5, "MD5 CheckSums between blob file and wams sdk download are different");
                    }


                    if (performStorageSdkDownloadVerification)
                    {
                        //Downloading Using Storage SDK
                        var stream = File.OpenWrite(downloadPathForStorageSdk);
                        blob.DownloadToStream(stream);
                        stream.Close();
                        stream.Dispose();

                        string hashValueForStorageSdkDownload = GetHashValueForFileMd5CheckSum(downloadPathForStorageSdk);
                        Assert.AreEqual(hashValueForWAMSSDKDownload, hashValueForStorageSdkDownload, "MD5 CheckSums for wams and storage downloads are different");
                        if (blob.Properties.ContentMD5 != null)
                        {
                            Assert.AreEqual(hashValueForStorageSdkDownload, blob.Properties.ContentMD5, "MD5 CheckSums between blob file and storage sdk download are different");
                        }
                        var azuresdkDownloadInfo = new FileInfo(downloadPathForStorageSdk);
                        Assert.AreEqual(azuresdkDownloadInfo.Length, blob.Properties.Length, "Azure SDK download file length in bytes is not matching length of asset file in blob");
                    }

                    var wamssdkDownloadInfo = new FileInfo(downloadPathForWamsSdk);

                    Assert.AreEqual(wamssdkDownloadInfo.Length, blob.Properties.Length, "WAMS SDK download file length in bytes is not matching length of asset file in blob");
                }
                finally
                {
                    if (File.Exists(downloadPathForStorageSdk))
                    {
                        File.Delete(downloadPathForStorageSdk);
                    }
                    if (File.Exists(downloadPathForWamsSdk))
                    {
                        File.Delete(downloadPathForWamsSdk);
                    }
                }
            }
        }

        private static string GetHashValueForFileMd5CheckSum(string filepath)
        {
            var retrievedBuffer = File.ReadAllBytes(filepath);

            return GetHashValueMd5Checksum(retrievedBuffer);
        }

        private static string GetHashValueMd5Checksum(byte[] input)
        {
            // Validate MD5 Value
            var md5Check = MD5.Create();
            md5Check.TransformBlock(input, 0, input.Length, null, 0);
            md5Check.TransformFinalBlock(new byte[0], 0, 0);

            // Get Hash Value
            byte[] hashBytes = md5Check.Hash;
            string hashVal = Convert.ToBase64String(hashBytes);
            return hashVal;
        }

        private static string GetHashValueForStreamMd5CheckSum(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.Position = 0;    // the stream position has reached the end at this point
                stream.CopyTo(ms);
                return GetHashValueMd5Checksum(ms.ToArray());
            }
        }

        private IAsset RunJobAndGetOutPutAsset(string jobName, out IAsset asset, out IJob job)
        {
            asset = CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = JobTests.GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            job = JobTests.CreateAndSubmitOneTaskJob(_mediaContext, jobName, mediaProcessor, JobTests.GetWamePreset(mediaProcessor), asset, TaskOptions.None);
            JobTests.WaitForJob(job.Id, JobState.Finished, JobTests.VerifyAllTasksFinished);
            Assert.IsTrue(job.OutputMediaAssets.Count > 0);
            IAsset outasset = job.OutputMediaAssets[0];
            Assert.IsNotNull(outasset);
            return outasset;
        }

        private static void UploadFile(ILocator locator, IAsset asset, string filePath, CloudMediaContext mediaContext)
        {
            var info = new FileInfo(filePath);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            BlobTransferClient blobTransferClient = mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
            Task task = file.UploadAsync(filePath, blobTransferClient, locator, CancellationToken.None);
            task.Wait();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(!task.IsFaulted);
        }

        private IAsset RefreshedAsset(IAsset asset)
        {
            IAsset refreshedAsset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.AreEqual(asset.Name, refreshedAsset.Name);
            Assert.AreEqual(AssetState.Initialized, refreshedAsset.State);
            return refreshedAsset;
        }

        public static void CreateAssetAndUploadNFilesSync(int expected,CloudMediaContext mediaContext, string sourceFileName)
        {
            IAsset asset = mediaContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
            VerifyAsset(asset);

            DirectoryInfo info = null;
            try
            {
                info = Directory.CreateDirectory(Guid.NewGuid().ToString());

                for (int i = 0; i < expected; i++)
                {
                    string fullFilePath = null;
                    string fileName;
                    fullFilePath = CreateNewFileFromOriginal(info,sourceFileName,out fileName);
                    IAssetFile file = asset.AssetFiles.Create(fileName);
                    file.Upload(fullFilePath);
                }
                Assert.AreEqual(expected, mediaContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
            }
            finally
            {
                if (info != null)
                {
                    info.Delete(recursive: true);
                }
            }
        }

        public static string CreateNewFileFromOriginal(DirectoryInfo info, string sourceFileName, out string fileName)
        {
            string fullFilePath = Path.Combine(info.FullName, Guid.NewGuid().ToString() + ".wmv");
            File.Copy(sourceFileName, fullFilePath);
            fileName = Path.GetFileName(fullFilePath);
            return fullFilePath;
        }

        private void AssetTests_OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            _downloadProgress = e.Progress;
            Trace.WriteLine(_downloadProgress);
        }

        public static bool CompareFiles(string fileName1, string fileName2)
        {
            var file1 = new FileInfo(fileName1);
            var file2 = new FileInfo(fileName2);

            if (file1.Length != file2.Length)
            {
                return false;
            }
            using (Stream stream1 = file1.OpenRead())
            {
                using (Stream stream2 = file2.OpenRead())
                {
                    return CompareStreams(stream1, stream2);
                }
            }
        }

        private static bool CompareStreams(Stream stream1, Stream stream2)
        {
            const int bufferSize = 1024;
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            while (true)
            {
                int size1 = stream1.Read(buffer1, 0, bufferSize);
                int size2 = stream2.Read(buffer2, 0, bufferSize);

                if (size1 != size2)
                {
                    return false;
                }

                if (size1 == 0)
                {
                    return true;
                }

                for (int i = 0; i < size1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                    {
                        return false;
                    }
                }
            }
        }

        public static void VerifyAsset(IAsset asset)
        {
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.IsNotNull(asset.Uri);
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");
        }

        public static IAsset CreateAssetAndUploadNFilesUsingAsyncCall(int expected, CloudMediaContext mediaContext,string sourceFileName)
        {
            IAsset asset = mediaContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
            VerifyAsset(asset);
            DirectoryInfo info = null;
            try
            {
                info = Directory.CreateDirectory(Guid.NewGuid().ToString());

                var files = new List<Task>();
                BlobTransferClient blobTransferClient = mediaContext.MediaServicesClassFactory.GetBlobTransferClient();
                IAccessPolicy policy = mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(20), AccessPermissions.Write);
                ILocator locator = mediaContext.Locators.CreateSasLocator(asset, policy);

                for (int i = 0; i < expected; i++)
                {
                    string fileName;
                    string fullFilePath = CreateNewFileFromOriginal(info,sourceFileName, out fileName);
                    IAssetFile file = asset.AssetFiles.Create(fileName);
                    files.Add(file.UploadAsync(fullFilePath, blobTransferClient, locator, CancellationToken.None));
                }
                Task.WaitAll(files.ToArray());
                foreach (Task task in files)
                {
                    Assert.IsTrue(task.IsCompleted);
                    Assert.IsFalse(task.IsFaulted);
                    Assert.IsNull(task.Exception);
                }
            }
            finally
            {
                if (info != null)
                {
                    info.Delete(recursive: true);
                }
            }
            return asset;
        }

        #endregion
    }
}