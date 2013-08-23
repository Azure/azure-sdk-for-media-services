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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class AssetTests
    {
        private CloudMediaContext _dataContext;
        private double _downloadProgress;
        private string _smallWmv;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv);
            _downloadProgress = 0;
        }


        /// <summary>
        /// Known issue with mime-type detection
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateAssetFile()
        {
            IAsset asset = _dataContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.CreateAsync(Path.GetFileName(_smallWmv), CancellationToken.None).Result;
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);
            BlobTransferClient blobTransferClient = new BlobTransferClient();
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

            foreach (IAssetFile ifile in asset.AssetFiles)
            {
                if (ifile.IsPrimary)
                {
                    Assert.IsTrue(string.Compare(Path.GetFileName(_smallWmv), ifile.Name, StringComparison.InvariantCultureIgnoreCase) == 0, "Main file is wrong");
                    break;
                }
            }
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("PullRequestValidation")]
        [ExpectedException(typeof (ArgumentException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldThrowArgumentExceptionOnAssetUploadWhenLocalFileNameNotMatchingAssetFileName()
        {
            IAsset asset = _dataContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.CreateAsync(Guid.NewGuid().ToString(), CancellationToken.None).Result;
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);
            try
            {
                file.UploadAsync(_smallWmv, new BlobTransferClient(), locator, CancellationToken.None).Wait();
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
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCreateSingleFileAssetWithNoLocatorUsingOveloadSync()
        {
            IAsset asset = _dataContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
            IAssetFile file = asset.AssetFiles.Create(Path.GetFileName(_smallWmv));
            file.Upload(_smallWmv);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");
        }


        [TestMethod]
        [Priority(1)]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateAssetFileInfoWithoutUploadingFile()
        {
            IAsset asset = _dataContext.Assets.Create("Empty", AssetCreationOptions.StorageEncrypted);
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
        [Priority(1)]
        [TestCategory("PullRequestValidation")]
        public void ShouldNotHaveLocatorsAfterAssetCreation()
        {
            IAsset asset = _dataContext.Assets.Create("Test", AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(0, asset.Locators.Count);
        }

        [TestMethod]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateEncryptedInitilizedAsset()
        {
            IAsset asset = _dataContext.Assets.Create("Test", AssetCreationOptions.StorageEncrypted);
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.AreEqual(0, asset.AssetFiles.Count(), "Asset has files");
            Assert.AreEqual(AssetState.Initialized, asset.State, "Expecting initilized state");

            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);

            IAssetFile file = asset.AssetFiles.Create(Path.GetFileName(_smallWmv));

            Task task = file.UploadAsync(_smallWmv,
                                         new BlobTransferClient
                                             {
                                                 NumberOfConcurrentTransfers = 10,
                                                 ParallelTransferThreadCount = 10
                                             },
                                         locator,
                                         CancellationToken.None);

            task.Wait();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(!task.IsFaulted);
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = _dataContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.AreEqual(asset.Name, refreshedAsset.Name);
            Assert.AreEqual(AssetState.Initialized, refreshedAsset.State);
            Assert.AreEqual(1, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset,1,false);
            ContentKeyTests.VerifyFileAndContentKeyMetadataForStorageEncryption(refreshedAsset, _dataContext);
        }

        [TestMethod]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateEmptyNoneEncryptedAssetUploadFileAndDownloadIt()
        {
            IAsset asset = _dataContext.Assets.Create("Test", AssetCreationOptions.None);

            VerifyAsset(asset);

            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv);

            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(1, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset, 1);
        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [Priority(1)]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateEmptyAssetUploadTwoFilesSetPrimaryAndDownloadFile()
        {
            IAsset asset = _dataContext.Assets.Create("Test", AssetCreationOptions.None);

            VerifyAsset(asset);

            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv);
            //asset = _dataContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            UploadFile(locator, asset, WindowsAzureMediaServicesTestConfiguration.SmallWmv2);

            Assert.AreEqual(2, asset.AssetFiles.Count());
            IAssetFile assetFile = asset.AssetFiles.ToList()[1];
            assetFile.IsPrimary = true;
            assetFile.Update();
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(2, refreshedAsset.AssetFiles.Count(), "file count wrong");
            VerifyAndDownloadAsset(refreshedAsset, 2);
        }



        [TestMethod]
        [ExpectedException(typeof (FileNotFoundException))]
        public void ShouldThrowCreatingAssetFileWithMissingFile()
        {
            try
            {
                CreateAsset(_dataContext, WindowsAzureMediaServicesTestConfiguration.BadSmallWmv, AssetCreationOptions.StorageEncrypted);
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        [TestMethod]
        [Priority(1)]
        public void CreateAssetAndUpload4FilesUsingSyncCall()
        {
            const int expected = 4;
            CreateAssetAndUploadNFilesSync(expected);
        }

        [TestMethod]
        [Priority(0)]
        public void ShouldCreateAssetAndUpload10FilesUsingSyncCall()
        {
            const int expected = 10;
            CreateAssetAndUploadNFilesSync(expected);
        }

        [TestMethod]
        [Priority(1)]
        public void ShouldCreateAssetAndUpload4FilesUsingAsyncCall()
        {
            const int expected = 4;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected);
            Assert.AreEqual(expected, _dataContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
        }

        [TestMethod]
        [Priority(0)]
        public void ShouldCreateAssetAndUpload10FilesUsingAsyncCall()
        {
            const int expected = 10;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected);
            Assert.AreEqual(expected, _dataContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
        }

        [TestMethod]
        [Priority(1)]
        public void ShouldCreateAssetAndUploadAndDownload10FilesUsingAsyncCall()
        {
            const int expected = 10;
            IAsset asset = CreateAssetAndUploadNFilesUsingAsyncCall(expected);
            Assert.AreEqual(expected, _dataContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("SdkDownload", TimeSpan.FromHours(12), AccessPermissions.Read);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, accessPolicy);
            var blobTransfer = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = _dataContext.NumberOfConcurrentTransfers,
                    ParallelTransferThreadCount = _dataContext.ParallelTransferThreadCount
                };

            var downloads = new List<Task>();
            var paths = new List<string>();
            foreach (IAssetFile file in _dataContext.Files.Where(c => c.ParentAssetId == asset.Id))
            {
                string path = Path.GetTempFileName();
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

        [TestMethod]
        [Priority(1)]
        public void ShouldCreateAssetAndCreate100FilesUsingAsyncCall()
        {
            IAsset asset = _dataContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
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
        [DeploymentItem(@"Media\SmallWmv2.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void CreateAssetWithUniqueAlternateIdAndFilterByIt()
        {
            CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset createdAsset = CreateAsset(_dataContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv2, AssetCreationOptions.StorageEncrypted);
            createdAsset.AlternateId = Guid.NewGuid().ToString();
            createdAsset.Update();
            int assetCount = Enumerable.Count(_dataContext.Assets.Where(c => c.AlternateId == createdAsset.AlternateId));

            Assert.AreEqual(1, assetCount, "Asset Count not right");
        }

        [TestMethod]
        [ExpectedException(typeof (DataServiceQueryException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldNotReturnAssetsForEmptyId()
        {
            IAsset createdAsset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset foundAsset = _dataContext.Assets.Where(c => c.Id == string.Empty).FirstOrDefault();
            Assert.IsNull(foundAsset, "should not found asset");
        }

        [TestMethod]
        [TestCategory("PullRequestValidation")]
        public void ShouldQueryAssetsByNameWithContains()
        {
            IAsset createdAsset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IAsset foundAsset = _dataContext.Assets.Where(c => c.Name.Contains(createdAsset.Name)).FirstOrDefault();
            Assert.IsNotNull(foundAsset);

        }

        [TestMethod]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldModifyAssetFile()
        {
            string assetId;
            {
                IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
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
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldDownloadAssetFile()
        {
            IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);

            
            VerifyAndDownloadAsset(asset, 1);
        }

        [TestMethod]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadSameAssetFile20TimesIdenticallyAsStorageSDK()
        {
            
            IAsset asset = _dataContext.Assets.Create("Test", AssetCreationOptions.None);
            VerifyAsset(asset);
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("temp", TimeSpan.FromMinutes(10), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);

            UploadFile(locator, asset, _smallWmv);
            UploadFile(locator, asset, WindowsAzureMediaServicesTestConfiguration.SmallWmv2);


            IAssetFile assetFile = asset.AssetFiles.FirstOrDefault();
            Assert.IsNotNull(assetFile);
            assetFile.IsPrimary = true;
            assetFile.Update();
            locator.Delete();
            policy.Delete();
            IAsset refreshedAsset = RefreshedAsset(asset);
            Assert.AreEqual(2, refreshedAsset.AssetFiles.Count(), "file count wrong");
           

            for (int i = 0; i < 20; i++)
            {
                VerifyAndDownloadAsset(refreshedAsset, 2);
            }
           
        }

        [TestMethod]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldDownloadCommonEncryptionProtectedAssetFile()
        {
            IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.CommonEncryptionProtected);
            VerifyAndDownloadAsset(asset, 1);
        }

        [TestMethod]
        [Priority(1)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldDownloadEnvelopeEncryptionProtectedAssetFile()
        {
            IAsset asset = _dataContext.Assets.Create(_smallWmv, AssetCreationOptions.EnvelopeEncryptionProtected);
            string name = Path.GetFileName(_smallWmv);
            IAssetFile file = asset.AssetFiles.Create(name);
            file.Upload(_smallWmv);
            VerifyAndDownloadAsset(asset, 1);
        }

        [TestMethod]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadIngestEncryptedAssetFile()
        {
            IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            VerifyAndDownloadAsset(asset, 1,false);
        }

        [TestMethod]
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
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
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
            outasset = _dataContext.Assets.Where(c => c.Id == outasset.Id).FirstOrDefault();
            Assert.AreEqual(1, outasset.ParentAssets.Count, "Unexpected number of parents assets");
            outasset.Delete();
        }

        [TestMethod]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldNotThrowTryingToDeleteAssetWithActiveLocators()
        {
            IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);
            IAccessPolicy accessPolicy = _dataContext.AccessPolicies.Create("ReadOnly", TimeSpan.FromMinutes(60), AccessPermissions.Read);
            ILocator sasLocator = _dataContext.Locators.CreateSasLocator(asset, accessPolicy);
            ILocator originLocator = _dataContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, accessPolicy);

            Assert.IsNotNull(_dataContext.Locators.Where(l => l.Id == sasLocator.Id).SingleOrDefault());
            Assert.IsNotNull(_dataContext.Locators.Where(l => l.Id == originLocator.Id).SingleOrDefault());

            asset = _dataContext.Assets.Where(a => a.Id == asset.Id).Single();
            Assert.AreEqual(3, asset.Locators.Count);
            Assert.IsTrue(asset.Locators.Any(l => l.Id == sasLocator.Id));
            Assert.IsTrue(asset.Locators.Any(l => l.Id == originLocator.Id));

            asset.Delete();
            Assert.IsNull(_dataContext.Locators.Where(l => l.Id == sasLocator.Id).SingleOrDefault());
            Assert.IsNull(_dataContext.Locators.Where(l => l.Id == originLocator.Id).SingleOrDefault());
        }

        [TestMethod]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateAssetWithSingleFile()
        {
            string assetFilePath = @"Content\interview.wmv";

            IAsset asset = CreateAsset(_dataContext, Path.GetFullPath(assetFilePath), AssetCreationOptions.None);

            Assert.AreEqual(AssetState.Initialized, asset.State);
            Assert.AreEqual(1, asset.AssetFiles.Count());
            Assert.AreEqual(1, asset.Locators.Count);

            IAssetFile assetFile = asset.AssetFiles.Single();
            Assert.IsTrue(assetFile.IsPrimary);
            Assert.AreEqual(Path.GetFileName(assetFilePath), assetFile.Name);
        }

        [TestMethod]
        [DeploymentItem(@".\Resources\TestFiles", "TestFiles")]
        [TestCategory("PullRequestValidation")]
        public void ShouldCreateAssetAsyncWithMultipleFiles()
        {
            string[] files = Directory.GetFiles("TestFiles");

            IAsset asset = _dataContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.None);
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);
            var blobclient = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = 5,
                    ParallelTransferThreadCount = 5
                };

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
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldReportProgressForFile()
        {
            string fileName = _smallWmv;
            bool reportedProgress = false;
            IAsset asset = _dataContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);
            var info = new FileInfo(fileName);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            var blobclient = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = 5,
                    ParallelTransferThreadCount = 5
                };
            blobclient.TransferProgressChanged += (s, e) =>
                {
                    Assert.AreEqual(fileName, e.LocalFile);
                    Assert.IsTrue(e.BytesTransferred <= e.TotalBytesToTransfer);
                    reportedProgress = true;
                };

            file.UploadAsync(fileName, blobclient, locator, CancellationToken.None).Wait();
            Assert.IsTrue(reportedProgress);
        }

        [TestMethod]
        [DeploymentItem(@".\Resources\interview.wmv", "Content")]
        [TestCategory("PullRequestValidation")]
        public void ShouldUpdateAssetNameAndAlternateId()
        {
            string fileName = @"Content\interview.wmv";

            IAsset asset = CreateAsset(_dataContext, Path.GetFullPath(fileName), AssetCreationOptions.CommonEncryptionProtected);

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
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("PullRequestValidation")]
        public void ShouldDeleteAsset()
        {
            IAsset asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.None);


            Assert.AreEqual(AssetState.Initialized, asset.State);
            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }

            asset.Delete();

            Assert.IsNull(_dataContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            Assert.IsNull(newContext.Assets.Where(a => a.Id == asset.Id).SingleOrDefault());
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("PullRequestValidation")]
        public void ShouldSetContentFileSizeOnAssetFileWithoutUpload()
        {
            IAsset asset = _dataContext.Assets.Create("test", AssetCreationOptions.None);
            IAssetFile fileInfo = asset.AssetFiles.Create("test.txt");
            int expected = 0;
            Assert.AreEqual(expected, fileInfo.ContentFileSize, "Unexpected ContentFileSize value");
            expected = 100;
            fileInfo.ContentFileSize = expected;
            fileInfo.Update();
            IAssetFile refreshedFile = _dataContext.Files.Where(c => c.Id == fileInfo.Id).FirstOrDefault();
            Assert.IsNotNull(refreshedFile);
            Assert.AreEqual(expected, refreshedFile.ContentFileSize, "ContentFileSize Mismatch after Update");

            //Double check with new context
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            refreshedFile = _dataContext.Files.Where(c => c.Id == fileInfo.Id).FirstOrDefault();
            Assert.IsNotNull(refreshedFile);
            Assert.AreEqual(expected, refreshedFile.ContentFileSize, "ContentFileSize Mismatch after Update");
        }

        #region Helper/utility methods

        public static IAsset CreateAsset(CloudMediaContext datacontext, string filePath, AssetCreationOptions options)
        {
            IAsset asset = datacontext.Assets.Create(Guid.NewGuid().ToString(), options);
            IAccessPolicy policy = datacontext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = datacontext.Locators.CreateSasLocator(asset, policy);
            var info = new FileInfo(filePath);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            file.UploadAsync(filePath,
                             new BlobTransferClient
                                 {
                                     NumberOfConcurrentTransfers = 5,
                                     ParallelTransferThreadCount = 5
                                 },
                             locator,
                             CancellationToken.None).Wait();
            file.IsPrimary = true;
            file.Update();

            return asset;
        }

       
        /// <summary>
        /// Verifies the and download asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="expectedFileCount">The expected file count.</param>
        /// <param name="performStorageSdkDownloadVerification">if set to <c>true</c> also perform storage SDK download verification.</param>
        private void VerifyAndDownloadAsset(IAsset asset, int expectedFileCount,bool performStorageSdkDownloadVerification = true)
        {
            Assert.AreEqual(expectedFileCount, asset.AssetFiles.Count(), "file count wrong");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(WindowsAzureMediaServicesTestConfiguration.ClientStorageConnectionString);
            string containername = asset.Id.Replace("nb:cid:UUID:", "asset-");
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containername);
            Assert.IsTrue(container.Exists(), "Asset container {0} can't be found", container);
          
            foreach (var assetFile in asset.AssetFiles)
            {
                string downloadPathForWamsSdk = Path.GetTempFileName();
                string downloadPathForStorageSdk = Path.GetTempFileName();
                
                var blob = container.GetBlobReferenceFromServer(assetFile.Name);
                Assert.IsTrue(blob.Exists(),"Blob for asset file is not found in corresponding container");
                blob.FetchAttributes();
                
                //Downloading using WAMS SDK
                assetFile.DownloadProgressChanged += AssetTests_OnDownloadProgress;
                assetFile.Download(downloadPathForWamsSdk);
                assetFile.DownloadProgressChanged -= AssetTests_OnDownloadProgress;
                Assert.AreEqual(100, _downloadProgress); 
                
                string hashValueForWAMSSDKDownload = GetHashValueForFileMd5CheckSum(downloadPathForWamsSdk);

                 //Comapring checksum if it is present
                if (blob.Properties.ContentMD5 != null)
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
                    File.Delete(downloadPathForStorageSdk);
                }

                var wamssdkDownloadInfo = new FileInfo(downloadPathForWamsSdk);
                
                Assert.AreEqual(wamssdkDownloadInfo.Length, blob.Properties.Length, "WAMS SDK download file length in bytes is not matching length of asset file in blob");
                
                
                File.Delete(downloadPathForWamsSdk);
                
            }
           
         
        }

        private static string GetHashValueForFileMd5CheckSum(string filepath)
        {
            byte[] retrievedBuffer = File.ReadAllBytes(filepath);

            // Validate MD5 Value
            var md5Check = System.Security.Cryptography.MD5.Create();
            md5Check.TransformBlock(retrievedBuffer, 0, retrievedBuffer.Length, null, 0);
            md5Check.TransformFinalBlock(new byte[0], 0, 0);

            // Get Hash Value
            byte[] hashBytes = md5Check.Hash;
            string hashVal = Convert.ToBase64String(hashBytes);
            return hashVal;
        }

        private IAsset RunJobAndGetOutPutAsset(string jobName, out IAsset asset, out IJob job)
        {
            asset = CreateAsset(_dataContext, _smallWmv, AssetCreationOptions.StorageEncrypted);
            IMediaProcessor mediaProcessor = JobTests.GetMediaProcessor(_dataContext, WindowsAzureMediaServicesTestConfiguration.MpEncoderName);
            job = JobTests.CreateAndSubmitOneTaskJob(_dataContext, jobName, mediaProcessor, JobTests.GetWamePreset(mediaProcessor), asset, TaskOptions.None);
            JobTests.WaitForJob(job.Id, JobState.Finished, JobTests.VerifyAllTasksFinished);
            Assert.IsTrue(job.OutputMediaAssets.Count > 0);
            IAsset outasset = job.OutputMediaAssets[0];
            Assert.IsNotNull(outasset);
            return outasset;
        }

        private static void UploadFile(ILocator locator, IAsset asset, string filePath)
        {
            var info = new FileInfo(filePath);
            IAssetFile file = asset.AssetFiles.Create(info.Name);
            Task task = file.UploadAsync(filePath,
                                         new BlobTransferClient
                                             {
                                                 NumberOfConcurrentTransfers = 10,
                                                 ParallelTransferThreadCount = 10
                                             },
                                         locator,
                                         CancellationToken.None);
            task.Wait();
            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(!task.IsFaulted);
        }

        private IAsset RefreshedAsset(IAsset asset)
        {
            IAsset refreshedAsset = _dataContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.AreEqual(asset.Name, refreshedAsset.Name);
            Assert.AreEqual(AssetState.Initialized, refreshedAsset.State);
            return refreshedAsset;
        }

        private void CreateAssetAndUploadNFilesSync(int expected)
        {
            IAsset asset = _dataContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
            VerifyAsset(asset);
            DirectoryInfo info = Directory.CreateDirectory(Guid.NewGuid().ToString());

            for (int i = 0; i < expected; i++)
            {
                string fileName;
                string fullFilePath = CreateNewFileFromOriginal(info, out fileName);
                IAssetFile file = asset.AssetFiles.Create(fileName);
                file.Upload(fullFilePath);
            }
            Assert.AreEqual(expected, _dataContext.Files.Where(c => c.ParentAssetId == asset.Id).Count());
        }

        private  string CreateNewFileFromOriginal(DirectoryInfo info, out string fileName)
        {
            string fullFilePath = Path.Combine(info.FullName, Guid.NewGuid().ToString() + ".wmv");
            File.Copy(_smallWmv, fullFilePath);
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

        private static void VerifyAsset(IAsset asset)
        {
            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID shuold not be null");
            Assert.IsNotNull(asset.Uri);
            Assert.AreEqual(AssetState.Initialized, asset.State, "Asset state wrong");
        }

        private IAsset CreateAssetAndUploadNFilesUsingAsyncCall(int expected)
        {
            IAsset asset = _dataContext.Assets.Create("TestWithMultipleFiles", AssetCreationOptions.None);
            VerifyAsset(asset);
            DirectoryInfo info = Directory.CreateDirectory(Guid.NewGuid().ToString());

            var files = new List<Task>();
            var client = new BlobTransferClient();
            IAccessPolicy policy = _dataContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(20), AccessPermissions.Write);
            ILocator locator = _dataContext.Locators.CreateSasLocator(asset, policy);


            for (int i = 0; i < expected; i++)
            {
                string fileName;
                string fullFilePath = CreateNewFileFromOriginal(info, out fileName);
                IAssetFile file = asset.AssetFiles.Create(fileName);
                files.Add(file.UploadAsync(fullFilePath, client, locator, CancellationToken.None));
            }
            Task.WaitAll(files.ToArray());
            foreach (Task task in files)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsFalse(task.IsFaulted);
                Assert.IsNull(task.Exception);
            }
            return asset;
        }

        #endregion
    }
}