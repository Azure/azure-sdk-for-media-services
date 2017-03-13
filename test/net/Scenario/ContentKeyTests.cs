//-----------------------------------------------------------------------
// <copyright file="ContentKeyTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ContentKeyTests
    {
        private CloudMediaContext _mediaContext;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateAssetFileWithEncryption()
        {
            var filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallWmv };
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(WindowsAzureMediaServicesTestConfiguration.SmallWmv), AssetCreationOptions.StorageEncrypted);

            // Associate an access policy with the asset so we can download the files associated with it
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromMinutes(10), AccessPermissions.Read);
            _mediaContext.Locators.CreateSasLocator(asset, policy);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID should not be null");
            Assert.AreEqual(1, asset.AssetFiles.Count(), "Child files count wrong");
            Assert.IsTrue(asset.Options == AssetCreationOptions.StorageEncrypted, "AssetCreationOptions did not have the expected value");

            VerifyFileAndContentKeyMetadataForStorageEncryption(asset, _mediaContext);
            VerifyStorageEncryptionOnFiles(asset, filePaths);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWMV2.wmv", "Media")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateAssetFileArrayWithEncryption()
        {
            var filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallWmv, WindowsAzureMediaServicesTestConfiguration.SmallWmv2 };
            IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            var blobclient = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = 5,
                    ParallelTransferThreadCount = 5
                };


            foreach (string filePath in filePaths)
            {
                var info = new FileInfo(filePath);
                IAssetFile file = asset.AssetFiles.Create(info.Name);
                file.UploadAsync(filePath, blobclient, locator, CancellationToken.None).Wait();
            }

            // Associate an access policy with the asset so we can download the files associated with it
            policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromMinutes(10), AccessPermissions.Read);
            _mediaContext.Locators.CreateSasLocator(asset, policy);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID should not be null");
            Assert.IsTrue(asset.Options == AssetCreationOptions.StorageEncrypted, "AssetCreationOptions did not have the expected value");

            VerifyFileAndContentKeyMetadataForStorageEncryption(asset, _mediaContext);
            VerifyStorageEncryptionOnFiles(asset, filePaths);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        [TestCategory("Bvt")]
        public void ShouldCreateAssetFileWithPlayReadyEncryption()
        {
            // Note that this file is not really PlayReady encrypted.  For the purposes of this test that is okay.
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(WindowsAzureMediaServicesTestConfiguration.SmallWmv), AssetCreationOptions.CommonEncryptionProtected);

            Guid keyId = Guid.NewGuid();
            byte[] contentKey = GetRandomBuffer(16);

            IContentKey key = _mediaContext.ContentKeys.Create(keyId, contentKey);
            asset.ContentKeys.Add(key);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID should not be null");
            Assert.AreEqual(1, asset.AssetFiles.Count(), "Child files count wrong");
            Assert.IsTrue(asset.Options == AssetCreationOptions.CommonEncryptionProtected, "AssetCreationOptions did not have the expected value");

            VerifyFileAndContentKeyMetadataForCommonEncryption(asset);
            VerifyContentKeyVersusExpectedValue2(asset, contentKey, keyId);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        [Priority(0)]
        public void TestQueries()
        {
            var keys = _mediaContext
                .ContentKeys
                .Where(a => a.LastModified < DateTime.UtcNow)
                .OrderByDescending(c => c.Created)
                .Skip(1)
                .Take(5)
                .First();

            keys = _mediaContext
                .ContentKeys
                .OrderBy(c => c.Created)
                .Where(a => a.LastModified < DateTime.UtcNow)
                .Skip(1)
                .Take(5)
                .First();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        [DeploymentItem(@"Media\SmallWMV2.wmv", "Media")]
        [Priority(0)]
        public void ShouldCreateAssetFileArrayWithPlayReadyEncryption()
        {
            // Note that these files are not really PlayReady encrypted.  For the purposes of this test that is okay.
            IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.CommonEncryptionProtected);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Write", TimeSpan.FromMinutes(5), AccessPermissions.Write);
            ILocator locator = _mediaContext.Locators.CreateSasLocator(asset, policy);
            var blobclient = new BlobTransferClient
                {
                    NumberOfConcurrentTransfers = 5,
                    ParallelTransferThreadCount = 5
                };


            foreach (string filePath in new[] { WindowsAzureMediaServicesTestConfiguration.SmallWmv, WindowsAzureMediaServicesTestConfiguration.SmallWmv2 })
            {
                var info = new FileInfo(filePath);
                IAssetFile file = asset.AssetFiles.Create(info.Name);
                file.UploadAsync(filePath, blobclient, locator, CancellationToken.None).Wait();
            }

            Guid keyId = Guid.NewGuid();
            byte[] contentKey = GetRandomBuffer(16);

            IContentKey key = _mediaContext.ContentKeys.Create(keyId, contentKey);
            asset.ContentKeys.Add(key);

            Assert.IsNotNull(asset, "Asset should be non null");
            Assert.AreNotEqual(Guid.Empty, asset.Id, "Asset ID should not be null");
            Assert.IsTrue(asset.Options == AssetCreationOptions.CommonEncryptionProtected, "AssetCreationOptions did not have the expected value");

            VerifyFileAndContentKeyMetadataForCommonEncryption(asset);
            VerifyContentKeyVersusExpectedValue2(asset, contentKey, keyId);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldCreateContentKeyWithTrackIdentifers()
        {
            IContentKey key = null;
            try
            {
                Guid keyId = Guid.NewGuid();
                byte[] contentKeyBytes = GetRandomBuffer(16);

                key = _mediaContext.ContentKeys.Create(
                    keyId,
                    contentKeyBytes,
                    "TrackIdentifer",
                    ContentKeyType.CommonEncryption,
                    new List<string> {"mp4a", "aacl"});

                string keyIdentifier = key.Id;

                var createdKey =
                    _mediaContext.ContentKeys.Where(k => k.Id.Equals(keyIdentifier, StringComparison.OrdinalIgnoreCase)).Single();

                Assert.AreEqual(createdKey.TrackIdentifiers, "mp4a,aacl");
            }
            finally
            {
                key?.Delete();
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldDeleteContentKey()
        {
            Guid keyId = Guid.NewGuid();
            byte[] contentKeyBytes = GetRandomBuffer(16);

            IContentKey key = _mediaContext.ContentKeys.Create(keyId, contentKeyBytes);

            string keyIdentifier = key.Id;
            key.Delete();

            foreach (IContentKey contentKey in _mediaContext.ContentKeys)
            {
                Assert.IsFalse(contentKey.Id == keyIdentifier);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldDeleteContentKeyWithDifferentContexts()
        {
            Guid keyId = Guid.NewGuid();
            byte[] contentKeyBytes = GetRandomBuffer(16);
            IContentKey key = _mediaContext.ContentKeys.Create(keyId, contentKeyBytes);

            string keyIdentifier = key.Id;
            key.Delete();

            foreach (IContentKey contentKey in _mediaContext.ContentKeys)
            {
                Assert.IsFalse(contentKey.Id == keyIdentifier);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldCreateTaskUsingStorageEncryptedAsset()
        {
            var filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallWmv };
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(WindowsAzureMediaServicesTestConfiguration.SmallWmv), AssetCreationOptions.StorageEncrypted);
            IMediaProcessor processor = JobTests.GetEncoderMediaProcessor(_mediaContext);
            IJob job = _mediaContext.Jobs.Create("Encode Job with encrypted asset");
            ITask task = job.Tasks.AddNew("Task 1", processor, JobTests.GetWamePreset(processor), TaskOptions.None);
            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Encrypted Output", AssetCreationOptions.StorageEncrypted);
            job.Submit();
            JobTests.WaitForJob(job.Id, JobState.Finished, JobTests.VerifyAllTasksFinished);

            CloudMediaContext context2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IJob job2 = context2.Jobs.Where(c => c.Id == job.Id).Single();

            foreach (IAsset outputAsset in job2.Tasks[0].OutputAssets)
            {
                VerifyFileAndContentKeyMetadataForStorageEncryption(outputAsset, _mediaContext);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\PlayReady Protection.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        [Priority(0)]
        public void ShouldGetClearConfigurationFromTask()
        {
            var filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallIsm, WindowsAzureMediaServicesTestConfiguration.SmallIsmc, WindowsAzureMediaServicesTestConfiguration.SmallIsmv };
            IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            foreach (string filePath in filePaths)
            {
                string filename = Path.GetFileName(filePath);
                IAssetFile file = asset.AssetFiles.Create(filename);
                file.Upload(filePath);
                if (WindowsAzureMediaServicesTestConfiguration.SmallIsm == filePath)
                {
                    file.IsPrimary = true;
                    file.Update();
                }
            }

            string originalConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfig);
            IMediaProcessor processor = JobTests.GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);

            IJob job = _mediaContext.Jobs.Create("PlayReady protect a smooth streaming asset for GetClearConfigurationFromTask");
            ITask task = job.Tasks.AddNew("SmoothProtectTask", processor, originalConfiguration, TaskOptions.ProtectedConfiguration);

            task.InputAssets.Add(asset);
            task.OutputAssets.AddNew("Job OutPut", AssetCreationOptions.None);

            // Verify that we can get the configuration back before we create a job template.  Note that is the point that things actually get encrypted
            Assert.AreEqual(String.CompareOrdinal(task.GetClearConfiguration(), originalConfiguration), 0);
            job.Submit();
            Assert.AreEqual(job.Tasks.Count, 1);
            Assert.AreEqual(TaskOptions.ProtectedConfiguration, job.Tasks[0].Options);
            Assert.IsNotNull(job.Tasks[0].InitializationVector);
            Assert.IsFalse(String.IsNullOrEmpty(job.Tasks[0].EncryptionKeyId));
            Assert.AreEqual(ConfigurationEncryption.SchemeName, job.Tasks[0].EncryptionScheme);
            Assert.AreEqual(ConfigurationEncryption.SchemeVersion, job.Tasks[0].EncryptionVersion);

            JobTests.WaitForJob(job.Id, JobState.Finished, JobTests.VerifyAllTasksFinished);

            // Verify that the configuration isn't clear
            Assert.AreNotEqual(String.CompareOrdinal(job.Tasks[0].Configuration, originalConfiguration), 0);

            // Verify that we can decrypt the configuration and get the correct clear value
            string decryptedConfiguration = job.Tasks[0].GetClearConfiguration();
            Assert.AreEqual(String.CompareOrdinal(decryptedConfiguration, originalConfiguration), 0);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [TestCategory("Bvt")]
        public void ShouldDeleteAssetWithCommonEncryptionContentKey()
        {
            var dataContext2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            // Note that this file is not really PlayReady encrypted.  For the purposes of this test that is okay.
            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(WindowsAzureMediaServicesTestConfiguration.SmallWmv), AssetCreationOptions.CommonEncryptionProtected);
            string assetId = asset.Id;
            string fileId = asset.AssetFiles.ToList()[0].Id;

            Guid keyId = Guid.NewGuid();
            byte[] contentKeyBytes = GetRandomBuffer(16);
            IContentKey key = _mediaContext.ContentKeys.Create(keyId, contentKeyBytes);
            asset.ContentKeys.Add(key);

            string keyIdentifier = key.Id;
            asset.Delete();

            IAsset resultAsset = dataContext2.Assets.Where(a => a.Id == assetId).FirstOrDefault();
            Assert.IsNull(resultAsset, "Asset was deleted we should not be able to query it by identifier.");

            IAssetFile resultFile = dataContext2.Files.Where(f => f.Id == fileId).FirstOrDefault();
            Assert.IsNull(resultFile, "Asset was deleted we should not be able to query its associated File by identifier.");

            // The content key should not exists
            IContentKey resultContentKey = dataContext2.ContentKeys.Where(c => c.Id == keyIdentifier).FirstOrDefault();
            Assert.IsNull(resultContentKey, "Common Encryption Content Key should be deleted by deleting the asset");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        [TestCategory("Bvt")]
        public void ShouldDeleteAssetWithStorageEncryptionContentKey()
        {
            // Use two contexts to cover the case where the content key needs to be internally attached to
            // the data context.  This simulates deleting a content key that we haven't just created.
            var dataContext2 = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            IAsset asset = AssetTests.CreateAsset(_mediaContext, Path.GetFullPath(WindowsAzureMediaServicesTestConfiguration.SmallWmv), AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(1, asset.ContentKeys.Count, "Expected 1 content key associated with the asset for storage encryption");
            Assert.AreEqual(ContentKeyType.StorageEncryption, asset.ContentKeys[0].ContentKeyType);

            // Get the ids to make sure they are no longer in the system after deleting the asset
            string assetId = asset.Id;
            string fileId = asset.AssetFiles.ToList()[0].Id;
            string keyId = asset.ContentKeys[0].Id;

            foreach (ILocator locator in asset.Locators)
            {
                locator.Delete();
            }

            // Now delete the asset and ensure that the content key and file are also deleted
            asset.Delete();

            foreach (IAsset assetFromRest in dataContext2.Assets)
            {
                Assert.IsFalse(assetFromRest.Id == assetId, "Asset was deleted we should not be able to query it by identifier.");
            }

            foreach (IAssetFile fileFromRest in dataContext2.Files)
            {
                Assert.IsFalse(fileFromRest.Id == fileId, "Asset was deleted we should not be able to query its associated File by identifier.");
            }

            foreach (IContentKey keyFromRest in dataContext2.ContentKeys)
            {
                Assert.IsFalse(keyFromRest.Id == keyId, "Asset was deleted we should not be able to query its associated storage encryption key by identifier.");
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [TestCategory("Bvt")]
        public void TestAssetFileDeleteRetry()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();

            int exceptionCount = 2;

            var contentKey = new ContentKeyData { Name = "testData" };
            var fakeResponse = new TestMediaDataServiceResponse { AsyncState = contentKey };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeys", contentKey));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(contentKey));

            dataContextMock.Setup((ctxt) => ctxt
                .SaveChangesAsync(contentKey))
                .Returns(() => Task.Factory.StartNew<IMediaDataServiceResponse>(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                }));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            contentKey.SetMediaContext(_mediaContext);

            contentKey.Delete();

            Assert.AreEqual(0, exceptionCount);
        }

        #region Helper/utility methods

        private static void VerifyContentKeyExists(IAssetFile file, IAsset assetFromServer, ContentKeyType expectedKeyType)
        {
            bool keyFound = false;

            foreach (IContentKey contentKeyFromServer in assetFromServer.ContentKeys)
            {
                if (file.EncryptionKeyId == contentKeyFromServer.Id)
                {
                    keyFound = true;
                    Assert.IsNotNull(contentKeyFromServer.ProtectionKeyType, "ProtectionKeyType should not be null.");
                    Assert.IsNotNull(contentKeyFromServer.ProtectionKeyId, "ProtectionKeyId should not be null.");
                    Assert.IsNotNull(contentKeyFromServer.EncryptedContentKey, "EncryptedContentKey should not be null.");

                    Assert.IsTrue(contentKeyFromServer.ContentKeyType == expectedKeyType, "ContentKeyType does not match expected value");
                    break;
                }
            }

            Assert.IsTrue(keyFound, "The expected key identifier was not found in the IAsset.ContentKeys collection queried from the server.");
        }

        private static void VerifyEncryptionSettingsMatch(IAssetFile file, IAsset assetFromServer, ContentKeyType expectedKeyType)
        {
            bool fileFound = false;

            foreach (IAssetFile fileFromServer in assetFromServer.AssetFiles)
            {
                if (fileFromServer.Id == file.Id)
                {
                    fileFound = true;
                    Assert.IsTrue(file.IsEncrypted == fileFromServer.IsEncrypted, "IsEncrypted doesn't match");
                    Assert.IsTrue(file.InitializationVector == fileFromServer.InitializationVector, "InitializationVector doesn't match");
                    Assert.IsTrue(file.EncryptionKeyId == fileFromServer.EncryptionKeyId, "EncryptionKeyId doesn't match");
                    Assert.IsTrue(file.EncryptionScheme == fileFromServer.EncryptionScheme, "EncryptionScheme doesn't match");
                    Assert.IsTrue(file.EncryptionVersion == fileFromServer.EncryptionVersion, "EncryptionVersion doesn't match");

                    if (!string.IsNullOrEmpty(fileFromServer.EncryptionKeyId))
                    {
                        VerifyContentKeyExists(file, assetFromServer, expectedKeyType);
                    }

                    break;
                }
            }

            Assert.IsTrue(fileFound, "The expected file identifier was not found in the IAsset.Files collection queried from the server.");
        }

        public static void VerifyFileAndContentKeyMetadataForStorageEncryption(IAsset asset, CloudMediaContext dataContext)
        {
            IAsset assetFromServer = Enumerable.First(dataContext.Assets.Where(c => c.Id == asset.Id));

            Assert.IsTrue(assetFromServer.Options == AssetCreationOptions.StorageEncrypted);

            foreach (IAssetFile file in asset.AssetFiles)
            {
                // ensure that the file is marked as encrypted and has data for
                // the encryption fields
                string fileInfo = string.Format("File {0}, Asset {1}", file.Id, file.Asset.Id);
                Assert.IsTrue(file.IsEncrypted, "IsEncrypted is not set." + fileInfo);
                Assert.IsNotNull(file.InitializationVector, "InitializationVector is not set" + fileInfo);
                Assert.IsNotNull(file.EncryptionKeyId, "EncryptionKeyId is not set" + fileInfo);
                Assert.IsTrue(file.EncryptionScheme == FileEncryption.SchemeName, "EncryptionScheme does not match expected value" + fileInfo);
                Assert.IsTrue(file.EncryptionVersion == FileEncryption.SchemeVersion, "EncryptionVersion does not match expected" + fileInfo);

                // ensure that the local settings match those stored on the server
                VerifyEncryptionSettingsMatch(file, assetFromServer, ContentKeyType.StorageEncryption);
            }
        }

        private void VerifyFileAndContentKeyMetadataForCommonEncryption(IAsset asset)
        {
            IAsset assetFromServer = Enumerable.First(_mediaContext.Assets.Where(c => c.Id == asset.Id));

            Assert.IsTrue(assetFromServer.Options == AssetCreationOptions.CommonEncryptionProtected);

            foreach (IAssetFile file in asset.AssetFiles)
            {
                // ensure that the file is marked as encrypted and has data for
                // the encryption fields
                Assert.IsTrue(file.IsEncrypted, "IsEncrypted is not set");
                Assert.IsNull(file.InitializationVector, "InitializationVector should not be set");
                Assert.IsNull(file.EncryptionKeyId, "EncryptionKeyId should not be set");
                Assert.IsTrue(file.EncryptionScheme == CommonEncryption.SchemeName, "EncryptionScheme does not match expected value");
                Assert.IsTrue(file.EncryptionVersion == CommonEncryption.SchemeVersion, "EncryptionVersion does not match expected");

                // ensure that the local settings match those stored on the server
                VerifyEncryptionSettingsMatch(file, assetFromServer, ContentKeyType.CommonEncryption);
            }

            Assert.IsTrue(asset.ContentKeys.Count > 0, "No content keys associated with the PlayReady protected asset");
        }

        private static void EnsureBuffersMatch(byte[] original, byte[] modified, int bytesToCompare)
        {
            for (int i = 0; i < bytesToCompare; i++)
            {
                if (original[i] != modified[i])
                {
                    throw new ArgumentException("Buffer data do not match");
                }
            }
        }

        private static void EnsureBuffersMatch(byte[] original, byte[] modified)
        {
            if (original.Length != modified.Length)
            {
                throw new ArgumentException("Buffer lengths do not match");
            }

            EnsureBuffersMatch(original, modified, original.Length);
        }

        private static void EnsureBufferTransformed(byte[] original, byte[] modified, int bytesToCompare)
        {
            int bytesThatMatch = 0;
            for (int i = 0; i < bytesToCompare; i++)
            {
                if (original[i] == modified[i])
                {
                    bytesThatMatch++;
                }
            }

            if (bytesThatMatch > (bytesToCompare / 100))
            {
                throw new ArgumentException("It is acceptable for some bytes to match in an encrypted data buffer but the percentage should be very small.  Further investigation needed.");
            }
        }

        private static void EnsureBufferTransformed(byte[] original, byte[] modified)
        {
            if (original.Length != modified.Length)
            {
                throw new ArgumentException("Buffer lengths do not match");
            }

            EnsureBufferTransformed(original, modified, original.Length);
        }

        public static byte[] GetRandomBuffer(int length)
        {
            var returnValue = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(returnValue);
            }

            return returnValue;
        }

        private void VerifyContentKeyVersusExpectedValue2(IAsset asset, byte[] contentKey, Guid keyId)
        {
            Assert.IsTrue(asset.ContentKeys.Count == 1, "Only one content key expected on the test asset");

            byte[] clearContentKey = asset.ContentKeys[0].GetClearKeyValue();
            Guid keyIdFromServer = EncryptionUtils.GetKeyIdAsGuid(asset.ContentKeys[0].Id);

            Assert.AreEqual(keyIdFromServer, keyId);
            EnsureBuffersMatch(contentKey, clearContentKey);

            X509Certificate2 selfSignedCert = SelfSignedCertificateFactory.Create();
            byte[] encryptedContentKey = asset.ContentKeys[0].GetEncryptedKeyValue(selfSignedCert);
            byte[] clearContentKey2 = EncryptionUtils.DecryptSymmetricKey(selfSignedCert, encryptedContentKey);

            EnsureBuffersMatch(contentKey, clearContentKey2);
        }

        private string GetFilePathFromArray(string[] originalFilePaths, IAssetFile file)
        {
            string returnValue = null;

            for (int i = 0; i < originalFilePaths.Length; i++)
            {
                if (file.Name == Path.GetFileName(originalFilePaths[i]))
                {
                    returnValue = originalFilePaths[i];
                    break;
                }
            }

            return returnValue;
        }

        private void VerifyStorageEncryptionOnFiles(IAsset asset, string[] originalFilePaths)
        {
            Assert.IsTrue(asset.AssetFiles.Count() == originalFilePaths.Length, "The number of files on the asset does not match the expected number.");
            Assert.IsTrue(asset.ContentKeys.Count == 1, "Only one content key expected on the test asset");

            Assert.IsTrue(asset.Locators.Count >= 1, "Expected an access policy to already be set on the asset");

            foreach (IAssetFile file in asset.AssetFiles)
            {
                // Get the iv from the IFileInfo
                ulong iv = Convert.ToUInt64(file.InitializationVector);

                // Figure out the path to the original asset
                string originalFilePath = GetFilePathFromArray(originalFilePaths, file);


                string tempFile = Guid.NewGuid().ToString();
                try
                {
                    file.Download(tempFile);

                    using (var originalFile = new FileStream(originalFilePath, FileMode.Open, FileAccess.Read))
                    using (Stream fileFromServer = File.Open(tempFile, FileMode.Open))
                    {
                        long fileOffset = 0;
                        var dataFromOriginalFile = new byte[1024];
                        var dataFromServerFile = new byte[dataFromOriginalFile.Length];
                        bool fExit = false;
                        while (!fExit)
                        {
                            int bytesRead = fileFromServer.Read(dataFromServerFile, 0, dataFromServerFile.Length);

                            if (0 == bytesRead)
                            {
                                fExit = true;
                            }
                            else
                            {
                                fileOffset += bytesRead;

                                int bytesRead2 = originalFile.Read(dataFromOriginalFile, 0, bytesRead);
                                Assert.IsTrue(bytesRead == bytesRead2);

                                EnsureBuffersMatch(dataFromOriginalFile, dataFromServerFile, bytesRead);
                            }
                        }

                        Assert.IsTrue(originalFile.Length == fileOffset, "Did not process the expected file length");
                    }
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
        }

        #endregion
    }
}