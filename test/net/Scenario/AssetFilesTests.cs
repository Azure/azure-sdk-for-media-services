//-----------------------------------------------------------------------
// <copyright file="AssetFilesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class AssetFilesTests
    {

        private CloudMediaContext _mediaContext;
        private string _smallWmv;
        private string _largeFile;
        private string _emptyFile;
        private double _downloadProgress;
        private string _outputDirectory;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext,
                WindowsAzureMediaServicesTestConfiguration.SmallWmv);
            _largeFile = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, "largeFile");
            _emptyFile = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, "emptyFile");
            _outputDirectory = "MediaDownloads";
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldNotThrowWhenSavingFileInfoIfTheAssetIsInPublishedState()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, _smallWmv, AssetCreationOptions.None);


            IAssetFile assetFile = asset.AssetFiles.First();

            assetFile.IsPrimary = false;
            assetFile.MimeType = String.Empty;

            assetFile.Update();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(ArgumentException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldThrowArgumentExceptionWhenUploadSyncFileNameNotEqualToAssetFileName()
        {
            IAsset asset = _mediaContext.Assets.Create("test", AssetCreationOptions.None);
            string fileUploaded = _smallWmv;
            IAssetFile fileInfo = asset.AssetFiles.Create("test.txt");
            try
            {
                fileInfo.Upload(fileUploaded);
            }
            catch (ArgumentException ex)
            {
                AssertFileMismatchException(fileUploaded, ex);
                throw;
            }
        }



        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(ArgumentException))]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldThrowArgumentExceptionWhenUploadAsyncFileNameNotEqualToAssetFileName()
        {
            IAsset asset = _mediaContext.Assets.Create("test", AssetCreationOptions.None);
            string fileUploaded = _smallWmv;
            IAssetFile fileInfo = asset.AssetFiles.Create("test.txt");
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(1),
                AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.Sas, asset, policy);
            try
            {
                fileInfo.UploadAsync(fileUploaded,
                    new BlobTransferClient
                    {
                        NumberOfConcurrentTransfers = 5,
                        ParallelTransferThreadCount = 5
                    },
                    locator,
                    CancellationToken.None);
            }
            catch (ArgumentException ex)
            {
                AssertFileMismatchException(fileUploaded, ex);
                throw;
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        public void When_Uploading_Multiple_Files_The_Progress_Event_Should_Only_Be_For_The_Bound_AssetFile()
        {
            IAsset asset = _mediaContext.Assets.Create("test", AssetCreationOptions.None);
            string fileUploaded = _smallWmv;
            var file = new FileInfo(fileUploaded);
            IAssetFile fileInfo = asset.AssetFiles.Create(Path.GetFileName(_smallWmv));
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(10),
                AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.Sas, asset, policy);
            var btc = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = 5,
                ParallelTransferThreadCount = 5
            };

            int allProgressEventsFiredCount = 0;

            btc.TransferProgressChanged += (sender, args) => { allProgressEventsFiredCount++; };

            bool progressFired = false;
            bool wrongFileSize = true;
            int fileProgressEventsCount = 0;

            fileInfo.UploadProgressChanged += (s, e) =>
            {
                progressFired = true;
                wrongFileSize = e.TotalBytes != file.Length;
                fileProgressEventsCount++;
            };

            Task uploadTask = fileInfo.UploadAsync(fileUploaded, btc, locator, CancellationToken.None);

            string competingFile = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext,
                WindowsAzureMediaServicesTestConfiguration.SmallMp41);

            var retryPolicy =
                _mediaContext.MediaServicesClassFactory.GetBlobStorageClientRetryPolicy()
                    .AsAzureStorageClientRetryPolicy();

            btc.UploadBlob(CreateUrl(locator, Path.GetFileName(competingFile)), competingFile, null, null,
                CancellationToken.None, retryPolicy).Wait();

            uploadTask.Wait();

            Assert.IsTrue(progressFired, "No upload progress event fired");
            Assert.IsFalse(wrongFileSize, "Received the wrong file size from the upload progress event");
            Assert.IsTrue(condition: fileProgressEventsCount < allProgressEventsFiredCount,
                message:
                    "Unexpected number of fired events, it should be more than the events fired for the uploaded file.");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadToFileFromAsset()
        {
            string fileUploaded = _smallWmv;

            IAsset asset = AssetTests.CreateAsset(_mediaContext, fileUploaded, AssetCreationOptions.None);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(AssetCreationOptions.None, asset.Options);
            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            Assert.AreEqual(1, asset.Locators.Count);

            VerifyAndDownloadAssetFile(assetFile, asset, fileUploaded);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDownloadToFileFromCommonEncryptionProtectedAsset()
        {
            string fileUploaded = _smallWmv;
            string outputDirectory = "Download" + Guid.NewGuid();

            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.CommonEncryptionProtected);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(AssetCreationOptions.CommonEncryptionProtected, asset.Options);
            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            Assert.AreEqual(1, asset.Locators.Count);

            VerifyAndDownloadAssetFile(assetFile, asset, fileUploaded);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldDownloadToFileFromStorageEncryptedAsset()
        {
            string fileUploaded = _smallWmv;

            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);
            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            Assert.AreEqual(1, asset.Locators.Count);

            VerifyAndDownloadAssetFile(assetFile, asset, fileUploaded);

        }

        [TestMethod]
        [Timeout(60000)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldDownloadManyConcurrentSmallFiles()
        {
            string fileUploaded = _smallWmv;
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            Assert.AreEqual(1, asset.Locators.Count);
            VerifyAndDownloadAssetFileNTimes(assetFile, asset,10,0,true);

        }
        [TestMethod]
        [Timeout(60000)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(StorageException))]
        public void ShouldThrowForbiddenExceptionWhenExpired()
        {
            string fileUploaded = _smallWmv;
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();
            Assert.AreEqual(assetFile.Asset.Id, asset.Id);           
            try
            {
                VerifyAndDownloadAssetFileNTimes(assetFile, asset, 1,1,true,true);
            }
            catch (AggregateException exception)
            {
                Assert.IsTrue(exception.InnerException.Message.Contains("The remote server returned an error: (403) Forbidden."));
                throw exception.InnerException;
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [ExpectedException(typeof(System.IO.DirectoryNotFoundException))]
        public void ShouldThrowFileIOExceptionWhenAccessToLocalFolderisNotThere()
        {
            string fileUploaded = _smallWmv;
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            if (Directory.Exists(_outputDirectory))
            {
                Directory.Delete(_outputDirectory, true);
            }
            try
            {
                VerifyAndDownloadAssetFileNTimes(assetFile, asset, 1,0,false,true);
            }
            catch (AggregateException exception)
            {
                Assert.IsTrue(exception.InnerException.Message.Contains("Could not find a part of the path"));
                throw exception.InnerException;
            }
            Assert.Fail();
        }
        [TestMethod]
        [Timeout(600000)]
        public void ShouldDownloadManyConcurrentLargeFiles()
        {
            string fileUploaded = _largeFile;
            CreateFileWithRandomData(fileUploaded, 300);
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded),
                AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();
            Assert.AreEqual(assetFile.Asset.Id, asset.Id);

            VerifyAndDownloadAssetFileNTimes(assetFile, asset, 4,0,true);
        }
        [TestMethod]
        [Timeout(60000)]
        public void ShouldDownloadEmptyFile()
        {
            string fileUploaded = _emptyFile;
            CreateFileWithRandomData(fileUploaded, 0);
            IAsset asset = AssetTests.CreateAssetAndUploadNFilesUsingAsyncCall(1, _mediaContext, fileUploaded);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            VerifyAndDownloadAssetFileNTimes(assetFile, asset, 10,0,true);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Timeout(60000)]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ShouldCancelDownloadToFileAsyncTaskAfter50Milliseconds()
        {
            string fileUploaded = _smallWmv;
            string outputDirectory = "Download" + Guid.NewGuid();
            string fileDownloaded = Path.Combine(outputDirectory, Path.GetFileName(fileUploaded));

            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(fileUploaded), AssetCreationOptions.StorageEncrypted);
            IAssetFile assetFile = asset.AssetFiles.First();

            Assert.AreEqual(assetFile.Asset.Id, asset.Id);
            Assert.AreEqual(1, asset.Locators.Count);

            CleanDirectory(outputDirectory);

            var source = new CancellationTokenSource();
            IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create("SdkDownload", TimeSpan.FromHours(12), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, accessPolicy);
            var blobTransfer = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = _mediaContext.NumberOfConcurrentTransfers,
                ParallelTransferThreadCount = _mediaContext.ParallelTransferThreadCount
            };

            Exception canceledException = null;
            Task downloadToFileTask = null;
            try
            {
                downloadToFileTask = assetFile.DownloadAsync(fileDownloaded, blobTransfer, locator, source.Token);

                // Send a cancellation signal after 2 seconds.
                Thread.Sleep(50);
                source.Cancel();

                // Block the thread waiting for the job to finish.
                downloadToFileTask.Wait();
            }
            catch (AggregateException exception)
            {
                Assert.AreEqual(1, exception.InnerExceptions.Count);
                canceledException = exception.InnerException;
            }
            finally
            {
                if (File.Exists(fileDownloaded))
                {
                    File.Delete(fileDownloaded);
                }
            }

            Assert.IsNotNull(canceledException);
            Assert.IsInstanceOfType(canceledException, typeof(OperationCanceledException));

            // The async task ends in a Canceled state.
            Assert.AreEqual(TaskStatus.Canceled, downloadToFileTask.Status);

            CloudMediaContext newContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            IAsset retreivedAsset = newContext.Assets.Where(a => a.Id == asset.Id).Single();

            Assert.AreEqual(2, retreivedAsset.Locators.Count);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestAssetFileCreateRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var expected = new AssetFileData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = expected };
            var fakeException = new WebException("testException", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Files", It.IsAny<object>()));
            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(It.IsAny<object>()))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var asset = new AssetData { Name = "testData" };

            asset.SetMediaContext(_mediaContext);
            IAssetFile file = ((IAsset)asset).AssetFiles.Create("test");
            Assert.AreEqual(expected.Name, file.Name);
            Assert.AreEqual(0, exceptionCount);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestAssetFileUpdateRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var asset = new AssetData { Name = "testData" };
            var file = new AssetFileData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = file };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Files", file));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(file));

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(file))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            file.SetMediaContext(_mediaContext);
            SetFileAsset(file, asset);

            file.Update();

            Assert.AreEqual(0, exceptionCount);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestAssetFileDeleteRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var asset = new AssetData { Name = "testData" };
            var file = new AssetFileData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = asset };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Files", file));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(file));

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(file))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            file.SetMediaContext(_mediaContext);

            file.Delete();

            Assert.AreEqual(0, exceptionCount);
        }

        #region Helper/utility methods

        private static void AssertFileMismatchException(string fileUploaded, ArgumentException ex)
        {
            Assert.IsTrue(ex.Message.Contains(Path.GetFileName(fileUploaded).ToUpperInvariant()));
            Assert.IsTrue(ex.Message.Contains("File name mismatch detected"));
        }

        private Uri CreateUrl(ILocator locator, string fileName)
        {
            var url = new UriBuilder(locator.Path);
            url.Path += "/" + fileName;

            return url.Uri;
        }

        #endregion

        private void SetFileAsset(AssetFileData file, IAsset asset)
        {
            typeof(AssetFileData)
                .GetField("_asset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(file, asset);
        }

        public static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);
        }

        public static void CreateFileWithRandomData(string fileName, uint sizeInMb)
        {
            byte[] data = new byte[8192];
            Random rng = new Random();
            using (FileStream stream = File.OpenWrite(fileName))
            {
                for (int i = 0; i < sizeInMb * 128; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        /// <summary>
        /// Verifies the and download asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="expectedFileCount">The expected file count.</param>
        private void VerifyAndDownloadAssetFile(IAssetFile assetFile, IAsset asset, string uploadedFile)
        {
            string downloadPathForWamsSdk = Guid.NewGuid().ToString();
            try
            {
                _downloadProgress = 0;
                //Downloading using WAMS SDK
                assetFile.DownloadProgressChanged += AssetTests_OnDownloadProgress;
                assetFile.Download(downloadPathForWamsSdk);
                assetFile.DownloadProgressChanged -= AssetTests_OnDownloadProgress;
                Assert.AreEqual(100, _downloadProgress);
                var fileUploadedInfo = new FileInfo(uploadedFile);
                var fileDownloadedInfo = new FileInfo(downloadPathForWamsSdk);
                Assert.AreEqual(fileUploadedInfo.Length, fileDownloadedInfo.Length);

            }
            finally
            {
                if (File.Exists(downloadPathForWamsSdk))
                {
                    File.Delete(downloadPathForWamsSdk);
                }
            }
        }
        /// <summary>
        /// Verifies the and download asset.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="expectedFileCount">The expected file count.</param>
        private void VerifyAndDownloadAssetFileNTimes(IAssetFile assetFile, IAsset asset,int count,int expirationTime, bool cleanOutputDirectory,bool expectFailure = false)
        {

            var source = new CancellationTokenSource();
            if (expirationTime == 0)
            {
                expirationTime = 1000;
            }
            IAccessPolicy accessPolicy = _mediaContext.AccessPolicies.Create("SdkDownload", TimeSpan.FromSeconds(expirationTime),
                AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, accessPolicy);
            var blobTransfer = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = _mediaContext.NumberOfConcurrentTransfers,
                ParallelTransferThreadCount = _mediaContext.ParallelTransferThreadCount
            };

            var downloads = new List<Task>();
            var paths = new List<string>();
            if (cleanOutputDirectory)
            {
                CleanDirectory(_outputDirectory);
            }
            try
            {
                for (int i = 0; i < count; i++)
                {
                    foreach (IAssetFile file in _mediaContext.Files.Where(c => c.ParentAssetId == asset.Id))
                    {
                        string path = Path.Combine(_outputDirectory, Guid.NewGuid().ToString());
                        paths.Add(path);
                        downloads.Add(assetFile.DownloadAsync(path, blobTransfer, locator, source.Token));
                    }
                }
                Task.WaitAll(downloads.ToArray());
            }
            finally
            {
                foreach (var fileDownloaded in paths)
                {
                    if (File.Exists(fileDownloaded))
                    {
                        File.Delete(fileDownloaded);
                    }
                    else
                    {
                        if (!expectFailure)
                        {
                            Assert.Fail();
                        }
                    }
                }
            }
        }
        private void AssetTests_OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            _downloadProgress = e.Progress;
            Trace.WriteLine(_downloadProgress);
        }
    }
}
