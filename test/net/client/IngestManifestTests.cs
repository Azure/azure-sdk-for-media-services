using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class IngestManifestTests
    {
        public TestContext TestContext { get; set; }

        private const string TestFile1 = @".\Resources\TestFiles\File0.txt";
        private const string TestFile2 = @".\Resources\TestFiles\File1.txt";
        private const string InterviewWmv = @".\Resources\interview.wmv";

        private const string DeploymentFolder1 = @".\Resources\TestFiles";
        private const string DeploymentFolder2 = @".\Resources";

        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }


        [TestMethod]
        public void ShouldBeAbleToGetManifests()
        {
            _mediaContext.IngestManifests.Take(5).ToList();
        }

        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [TestMethod]
        [Priority(1)]
        public void ListAssetsAndFilesForNewlyCreatedManifests()
        {

            IIngestManifest ingestManifest = CreateEmptyManifestAndVerifyIt();

            IAsset asset = _mediaContext.Assets.Create("name", AssetCreationOptions.None);
            Assert.IsNotNull(asset);
            IIngestManifestAsset ingestManifestAsset = ingestManifest.IngestManifestAssets.Create(asset, new[] { TestFile1 });
            VerifyManifestAsset(ingestManifestAsset);
            IIngestManifestAsset firstAsset = ingestManifest.IngestManifestAssets.FirstOrDefault();
            VerifyManifestAsset(firstAsset);
            Assert.AreEqual(ingestManifestAsset.Id, firstAsset.Id);

            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest sameIngestManifest = _mediaContext.IngestManifests.Where(c => c.Id == ingestManifest.Id).FirstOrDefault();
            Assert.IsNotNull(sameIngestManifest);
            Assert.AreEqual(1, sameIngestManifest.IngestManifestAssets.Count(), "Manifest asset count is not matching expecting value 1");
            firstAsset = sameIngestManifest.IngestManifestAssets.FirstOrDefault();
            VerifyManifestAsset(firstAsset);
            Assert.AreEqual(1, firstAsset.IngestManifestFiles.Count(), "Manifest file count is not matching expecting value 1");
            IIngestManifestFile firstFile = firstAsset.IngestManifestFiles.FirstOrDefault();
            Assert.AreEqual("text/plain", firstFile.MimeType, "IngestManifestFile's MimeType is wrong");
            VerifyManifestFile(firstFile);
        }

        private static void VerifyManifestFile(IIngestManifestFile ingestManifestFile)
        {
            Assert.IsNotNull(ingestManifestFile);
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifestFile.Id), "File Is is null or empty");
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifestFile.Name), "File Is is null or empty");
        }

        private static void VerifyManifestAsset(IIngestManifestAsset ingestManifestAsset)
        {
            Assert.IsNotNull(ingestManifestAsset);
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifestAsset.Id), "Manifest asset id is is null or empty");
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifestAsset.ParentIngestManifestId), "ParentManifestId is null or empty");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowExceptionWhenAttemptingToCreateManifestAssetFromContextCollection()
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IAsset asset1 = context.Assets.Create("Asset1", AssetCreationOptions.CommonEncryptionProtected);
            context.IngestManifestAssets.Create(asset1, new[] { "C:\\temp.txt" });
        }

        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        [Priority(1)]
        public void DeleteManifestShouldDeleteAllManifestAssetsAndFiles()
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest ingestManifest = CreateManifestWithAssetsAndVerifyIt(context);

            VerifyExistenceofAssetsAndFilesForManifest(ingestManifest, context);
            ingestManifest.Delete();
            context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            int assetsCount = context.IngestManifestAssets.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();
            int filescount = context.IngestManifestFiles.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();

            Assert.AreEqual(0, assetsCount, "There are assets belonging to manifest after manifest deletion");
            Assert.AreEqual(0, filescount, "There are files belonging to manifest assets after manifest deletion");
        }

        private static void VerifyExistenceofAssetsAndFilesForManifest(IIngestManifest ingestManifest, CloudMediaContext context)
        {
            int assetsCount = context.IngestManifestAssets.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();
            int filescount = context.IngestManifestFiles.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();
            Assert.IsTrue(assetsCount > 0, "When manifest is empty we are expecting to have associated assets");
            Assert.IsTrue(filescount > 0, "When manifest is empty we are expecting to have associated files");
        }

        [TestMethod]
        public void DeleteAssetShouldDeleteAllManifestAssetsFiles()
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest ingestManifest = CreateManifestWithAssetsAndVerifyIt(context);
            VerifyExistenceofAssetsAndFilesForManifest(ingestManifest, context);

            IIngestManifestAsset asset = ingestManifest.IngestManifestAssets.FirstOrDefault();
            VerifyManifestAsset(asset);
            int filescount = context.IngestManifestFiles.Where(c => c.ParentIngestManifestAssetId == asset.Id).Count();
            Assert.IsTrue(filescount > 0, "Expecting to have files for given asset");
            asset.Delete();

            context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            VerifyAssetIsNotExist(asset, context);

            filescount = context.IngestManifestFiles.Where(c => c.ParentIngestManifestAssetId == asset.Id).Count();
            Assert.AreEqual(0, filescount, "There are files belonging to manifest assets after asset deletion");
        }

        private void VerifyAssetIsNotExist(IIngestManifestAsset asset, CloudMediaContext context)
        {
            Assert.AreEqual(0, context.IngestManifestAssets.Where(c => c.Id == asset.Id).Count(), "Manifest Asset exists.Expected result that asset is not returned by REST API");
        }

        /// <summary>
        /// Creating empty manifest
        /// </summary>
        [TestMethod]
        public void CreateEmptyManifest()
        {
            CreateEmptyManifestAndVerifyIt();
        }

        [TestMethod]
        public void ShouldCreateEmptymanifestWithDefaultStorageAccountName()
        {
            const string manifestName = "TestManifest";
            _mediaContext.IngestManifests.Create(manifestName, _mediaContext.DefaultStorageAccount.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void ShouldThrowTryingToCreateEmptyManifestWithNonExistentAccountName()
        {
            const string manifestName = "TestManifest";
            try
            {
                _mediaContext.IngestManifests.Create(manifestName, Guid.NewGuid().ToString());
            }
            catch (DataServiceRequestException ex)
            {
                var response = ex.Response.FirstOrDefault();
                Assert.IsNotNull(response, "DataServiceRequestException Response is Null");
                Assert.IsTrue(response.Error.Message.Contains("Cannot find the storage account"));
                throw;
            }
        }

        /// <summary>
        /// Known issue in REST API.
        /// </summary>
        [Ignore]
        [TestMethod]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void ShouldThrowTryingToCreateEmptyManifestWithEmptyAccountName()
        {
            const string manifestName = "TestManifest";
            try
            {
                _mediaContext.IngestManifests.Create(manifestName, String.Empty);
            }
            catch (DataServiceRequestException ex)
            {
                var response = ex.Response.FirstOrDefault();
                Assert.IsNotNull(response, "DataServiceRequestException Response is Null");
            }
        }

        /// <summary>
        /// Deleting empty manifest
        /// </summary>
        [TestMethod]
        [TestCategory("DailyBvtRun")]
        public void CreateEmptyManifestAndDeleteIt()
        {
            IIngestManifest ingestManifest = CreateEmptyManifestAndVerifyIt();
            string id = ingestManifest.Id;
            ingestManifest.Delete();
            VerifyManifestDeletion(id);
            ingestManifest = CreateEmptyManifestAndVerifyIt();
            id = ingestManifest.Id;
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifest.Name));
            Task t = ingestManifest.DeleteAsync();
            t.Wait();
            VerifyManifestDeletion(id);
        }

        private static void VerifyManifestDeletion(string id)
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest expectedNull = context.IngestManifests.Where(c => c.Id == id).FirstOrDefault();
            Assert.IsNull(expectedNull, "Manifest has not been deleted as expected");
        }

        /// <summary>
        /// Creating empty manifest and updating it
        /// </summary>
        [TestMethod]
        [TestCategory("DailyBvtRun")]
        public void CreateEmptyManifestAndUpdateIt()
        {
            IIngestManifest ingestManifest = CreateEmptyManifestAndVerifyIt();
            string newName = "New Name 1";
            string id = ingestManifest.Id;
            ingestManifest.Name = newName;
            ingestManifest.Update();
            VerifyNameForExitingManifest(ingestManifest, newName, id);

            //Async Update
            newName = "New Name 2";
            ingestManifest.Name = newName;
            Task t = ingestManifest.UpdateAsync();
            t.Wait();
            VerifyNameForExitingManifest(ingestManifest, newName, id);
        }


        [TestMethod]
        public void CreateEmptyBulkIngestManifestAsync()
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            const string manifestName = "";
            Task<IIngestManifest> taskManifest = context.IngestManifests.CreateAsync(manifestName);
            IIngestManifest ingestManifest = taskManifest.Result;
            Assert.IsTrue(String.IsNullOrEmpty(ingestManifest.Name));
        }


        /// <summary>
        /// We should be able to add additional asset infoes into active manifest.
        /// </summary>
        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        [DeploymentItem(InterviewWmv, DeploymentFolder2)]
        public void AddingAdditionalAssetInfoesToExistingManifest()
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest ingestManifestCreated = CreateManifestWithAssetsAndVerifyIt(context);
            context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            IIngestManifest ingestManifest = context.IngestManifests.Where(c => c.Id == ingestManifestCreated.Id).FirstOrDefault();
            Assert.AreNotSame(0, ingestManifest.IngestManifestAssets.Count());
            IAsset asset1 = context.Assets.Create("Asset1", AssetCreationOptions.StorageEncrypted);
            Task<IIngestManifestAsset> task1 = ingestManifest.IngestManifestAssets.CreateAsync(asset1, new string[1] { InterviewWmv }, CancellationToken.None);

            IIngestManifestAsset assetInfo1 = task1.Result;
            Assert.AreEqual(1, assetInfo1.IngestManifestFiles.Count());
        }

        /// <summary>
        /// We should be able to add additional files to existing asset infoes of active manifest.
        /// </summary>
        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        [DeploymentItem(InterviewWmv, DeploymentFolder2)]
        public void AddingAdditionalFilesToAssetInManifest()
        {

            IIngestManifest ingestManifestCreated = CreateManifestWithAssetsAndVerifyIt(_mediaContext);
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            var ingestManifestRefreshed = _mediaContext.IngestManifests.Where(c => c.Id == ingestManifestCreated.Id).FirstOrDefault();
            Assert.IsNotNull(ingestManifestRefreshed.Statistics);
            Assert.IsNotNull(ingestManifestCreated.Statistics);
            Assert.AreEqual(2, ingestManifestRefreshed.Statistics.PendingFilesCount);

            AddFileToExistingManifestAssetInfo(_mediaContext, ingestManifestCreated.Id);
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            ingestManifestRefreshed = _mediaContext.IngestManifests.Where(c => c.Id == ingestManifestCreated.Id).FirstOrDefault();
            Assert.AreEqual(3, ingestManifestRefreshed.Statistics.PendingFilesCount);
        }


        /// <summary>
        /// Encrypting manifest files
        /// </summary>
        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        public void CheckIfOnlyStorageEncryptedFilesExistsInEncryptionFolderAnfterEncrypt()
        {
            List<IIngestManifestFile> files;
            IIngestManifest ingestManifestCreated;
            var path = CreateManifestEncryptFiles(out files, out ingestManifestCreated);

            foreach (var manifestAssetFile in files)
            {
                if (manifestAssetFile.EncryptionScheme == CommonEncryption.SchemeName)
                {
                    Assert.IsFalse(File.Exists(Path.Combine(path, manifestAssetFile.Name)));
                }
                if (manifestAssetFile.EncryptionScheme == FileEncryption.SchemeName)
                {
                    Assert.IsTrue(File.Exists(Path.Combine(path, manifestAssetFile.Name)));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowKeyNotFoundExceptionDuringEncryptIfKeyIsMissing()
        {
            var sourcePath =DeploymentFolder1;
            Assert.IsTrue(Directory.Exists(sourcePath));
            List<string> files = Directory.EnumerateFiles(sourcePath, "*.txt").ToList();

            //Creating empty manifest
            const string manifestName = "Manifest 1";
            IIngestManifest ingestManifestCreated = _mediaContext.IngestManifests.Create(manifestName);

            //Adding manifest asset info with multiple file
            IAsset emptyAsset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            IIngestManifestAsset ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.CreateAsync(emptyAsset, files.ToArray(), CancellationToken.None).Result;
            Assert.IsNotNull(ingestManifestAsset);

            //According to last REST implementation breaking a link 
            //also deleting a key on server side if no other links are found
            emptyAsset.ContentKeys.RemoveAt(0);

            var path = @".\Resources\TestFiles\" + Guid.NewGuid();
            Directory.CreateDirectory(path);
            try
            {
                ingestManifestCreated.EncryptFiles(path);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                throw ex.InnerExceptions[0];
            }
        }

        [TestMethod]
        public void ShouldThrowAggregateExceptionWithMultipleKetNotFoundExceptionDuringEncryptIfKeyIsMissing()
        {
            var sourcePath = @".\Resources\TestFiles\";
            var path = @".\Resources\TestFiles\" + Guid.NewGuid();
            Assert.IsTrue(Directory.Exists(sourcePath));
            List<string> files = Directory.EnumerateFiles(sourcePath, "File0.txt").ToList();

            //Creating empty manifest
            const string manifestName = "Manifest 1";
            IIngestManifest ingestManifestCreated = _mediaContext.IngestManifests.Create(manifestName);

            //Adding manifest asset info with multiple file
            IAsset emptyAsset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            IIngestManifestAsset ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.CreateAsync(emptyAsset, files.ToArray(), CancellationToken.None).Result;
            Assert.IsNotNull(ingestManifestAsset);
            emptyAsset.ContentKeys.RemoveAt(0);

            files = Directory.EnumerateFiles(sourcePath, "File1.txt").ToList();
            emptyAsset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.CreateAsync(emptyAsset, files.ToArray(), CancellationToken.None).Result;
            Assert.IsNotNull(ingestManifestAsset);
            //According to last REST implementation breaking a link 
            //also deleting a key on server side if no other links are found
            emptyAsset.ContentKeys.RemoveAt(0);

            
            Directory.CreateDirectory(path);
            try
            {
                ingestManifestCreated.EncryptFiles(path);
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.IsTrue(ex.InnerExceptions[0] is InvalidOperationException);
                Assert.IsTrue(ex.InnerExceptions[1] is InvalidOperationException);
            }
        }

       

        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        public void EncryptManifestWithFewSmalFiles()
        {
            var sourcePath = DeploymentFolder1;
            Assert.IsTrue(Directory.Exists(sourcePath));
            List<string> files = Directory.EnumerateFiles(sourcePath, "*.txt").ToList();
            EncryptFilesDecryptAndCompare(files);
        }

        private static void EncryptFilesDecryptAndCompare(List<string> files)
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            //Creating empty manifest
            const string manifestName = "Manifest 1";
            IIngestManifest ingestManifestCreated = context.IngestManifests.Create(manifestName);

            //Adding manifest asset info with multiple file
            IAsset emptyAsset = context.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            IIngestManifestAsset ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.CreateAsync(emptyAsset, files.ToArray(), CancellationToken.None).Result;

            var path = @".\Resources\TestFiles\" + Guid.NewGuid();
            Directory.CreateDirectory(path);
            ingestManifestCreated.EncryptFiles(path);

            Dictionary<string, string> filePaths = new Dictionary<string, string>();
            foreach (var filePath in files)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                filePaths.Add(fileInfo.Name, filePath);
            }


            foreach (var assetFile in ingestManifestAsset.IngestManifestFiles)
            {
                var encryptedPath = Path.Combine(path, assetFile.Name);
                Assert.IsTrue(File.Exists(encryptedPath));
                var decryptedPath = DecryptedFile(assetFile, encryptedPath, context);
                Assert.IsTrue(AssetTests.CompareFiles(decryptedPath, filePaths[assetFile.Name]), "Original file and Decrypted are not same");
            }
        }

        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        [Priority(0)]
        [TestCategory("DailyBvtRun")]
        public void EncryptManifestFilesAndVerifyThemAfterDeencryption()
        {


            List<IIngestManifestFile> files;
            IIngestManifest ingestManifestCreated;
            var path = CreateManifestEncryptFiles(out files, out ingestManifestCreated);
            IIngestManifestAsset ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.ToList().Where(c => c.Asset.Options == AssetCreationOptions.StorageEncrypted).FirstOrDefault();
            IIngestManifestFile mFile = ingestManifestAsset.IngestManifestFiles.Where(c => c.Name == "File0.txt").FirstOrDefault();

            Dictionary<string, string> filePaths = new Dictionary<string, string>();
            foreach (var filePath in new[] { TestFile1, TestFile2 })
            {
                FileInfo fileInfo = new FileInfo(filePath);
                filePaths.Add(fileInfo.Name, filePath);
            }

            var encryptedPath = Path.Combine(path, mFile.Name);
            Assert.IsTrue(File.Exists(encryptedPath));
            var decryptedPath = DecryptedFile(mFile, encryptedPath, _mediaContext);
            Assert.IsTrue(AssetTests.CompareFiles(decryptedPath, filePaths[mFile.Name]), "Original file and Decrypted are not same");

        }

        private static string DecryptedFile(IIngestManifestFile ingestManifestFile, string encryptedPath, CloudMediaContext context)
        {
            IIngestManifestAsset ingestManifestAsset = context.IngestManifestAssets.Where(a => a.Id == ingestManifestFile.ParentIngestManifestAssetId).FirstOrDefault();
            Assert.IsNotNull(ingestManifestAsset);

            IList<IContentKey> keys = ingestManifestAsset.Asset.ContentKeys.Where(c => c.ContentKeyType == ContentKeyType.StorageEncryption).ToList();
            Assert.AreEqual(1, keys.Count, "Expecting only one storage key per asset");
            IContentKey key = keys.FirstOrDefault();
            Assert.IsNotNull(ingestManifestAsset);


            Guid keyId = EncryptionUtils.GetKeyIdAsGuid(key.Id);
            FileEncryption fileEncryption = new FileEncryption(key.GetClearKeyValue(), keyId);

            ulong iv = Convert.ToUInt64(ingestManifestFile.InitializationVector, CultureInfo.InvariantCulture);
            var decryptedPath = @".\Resources\TestFiles\Decrypted" + Guid.NewGuid();
            if (!Directory.Exists(decryptedPath))
            {
                Directory.CreateDirectory(decryptedPath);
            }

            decryptedPath = Path.Combine(decryptedPath, ingestManifestFile.Name);
            FileInfo fileInfo = new FileInfo(encryptedPath);
            var maxblocksize = GetBlockSize(fileInfo.Length);
            List<string> blockList = new List<string>();
            int numberOfthreads = 1;
            var queue = PreapreDownloadQueue(maxblocksize, fileInfo.Length, ref numberOfthreads, out blockList);

            using (var fs = new FileStream(decryptedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                KeyValuePair<int, int> block;
                while (queue.TryDequeue(out block))
                {
                    fs.Seek(block.Key * maxblocksize, SeekOrigin.Begin);
                    using (FileStream stream = File.OpenRead(encryptedPath))
                    {
                        byte[] buffer = new byte[block.Value];
                        stream.Seek(block.Key * maxblocksize, SeekOrigin.Begin);
                        int read = stream.Read(buffer, 0, (int)block.Value);
                        if (fileEncryption != null)
                        {
                            lock (fileEncryption)
                            {
                                using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(iv, block.Key * maxblocksize))
                                {
                                    encryptor.TransformBlock(inputBuffer: buffer, inputOffset: 0, inputCount: buffer.Length, outputBuffer: buffer, outputOffset: 0);
                                }
                            }
                        }

                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            return decryptedPath;
        }

        private static string CreateManifestEncryptFiles(out List<IIngestManifestFile> files, out IIngestManifest ingestManifestCreated)
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            ingestManifestCreated = CreateManifestWithAssetsAndVerifyIt(context);
            
            var destination = @".\Resources\TestFiles\" + Guid.NewGuid();
            Directory.CreateDirectory(destination);
            ingestManifestCreated.EncryptFilesAsync(destination, CancellationToken.None).Wait();

            var manifestid = ingestManifestCreated.Id;
            //returning all encrypted files
            files = context.IngestManifestFiles.Where(c => c.ParentIngestManifestId == manifestid && c.IsEncrypted == true).ToList();
            Assert.AreEqual(2, files.Count);
            return destination;
        }

        private static void AddFileToExistingManifestAssetInfo(CloudMediaContext context, string id)
        {
            IIngestManifest ingestManifest = context.IngestManifests.Where(c => c.Id == id).FirstOrDefault();
            Assert.IsNotNull(ingestManifest);
            Assert.IsNotNull(ingestManifest.IngestManifestAssets);
            int expectedFilesCount = context.IngestManifestFiles.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();


            foreach (IIngestManifestAsset assetInfo in ingestManifest.IngestManifestAssets)
            {
                Assert.IsNotNull(assetInfo.IngestManifestFiles);
                //Enumerating through all files
                foreach (IIngestManifestFile file in assetInfo.IngestManifestFiles)
                {
                    VerifyManifestFile(file);
                }

                //Adding new file to collection


            }
            var firstOrDefault = ingestManifest.IngestManifestAssets.FirstOrDefault();
            Assert.IsNotNull(firstOrDefault);

            IIngestManifestFile addedFile = firstOrDefault.IngestManifestFiles.Create(InterviewWmv);
            VerifyManifestFile(addedFile);
            expectedFilesCount++;
            int filesCountFinal = context.IngestManifestFiles.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count();

            Assert.AreEqual(expectedFilesCount, filesCountFinal);
        }

        [TestMethod]
        public void DeleteActiveExistingManifest()
        {
            IIngestManifest ingestManifest = CreateManifestWithAssetsAndVerifyIt(_mediaContext);
            VerifyAssetStateAndDelete(IngestManifestState.Active, ingestManifest.Id);
        }

        [TestMethod]
        public void DeleteInactiveExistingManifest()
        {
            IIngestManifest ingestManifest = CreateEmptyManifestAndVerifyIt();
            VerifyAssetStateAndDelete(IngestManifestState.Inactive, ingestManifest.Id);
        }

        private static void VerifyAssetStateAndDelete(IngestManifestState expectedState, string id)
        {
            IIngestManifest ingestManifest = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext().IngestManifests.Where(c => c.Id == id).FirstOrDefault();
            Assert.IsNotNull(ingestManifest);
            Assert.AreEqual(expectedState, ingestManifest.State);
            ingestManifest.Delete();
        }

        [TestMethod]
        [DeploymentItem(InterviewWmv, DeploymentFolder2)]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        public void CreateEmptyBulkIngestAndAttachFiles()
        {
            CreateManifestWithAssetsAndVerifyIt(_mediaContext);
        }

        [TestMethod]
        [DeploymentItem(TestFile1, DeploymentFolder1)]
        [DeploymentItem(TestFile2, DeploymentFolder1)]
        [ExpectedException(typeof(IOException))]
        public void EncryptManifestTestDisableOverwriteExistingFile()
        {

            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            var sourcePath = DeploymentFolder1;
            Assert.IsTrue(Directory.Exists(sourcePath));
            List<string> files = Directory.EnumerateFiles(sourcePath, "*.txt").ToList();
            const string manifestName = "Manifest 1";
            IIngestManifest ingestManifestCreated = context.IngestManifests.Create(manifestName);

            //Adding manifest asset info with multiple file
            IAsset emptyAsset = context.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);

            IIngestManifestAsset ingestManifestAsset = ingestManifestCreated.IngestManifestAssets.Create(emptyAsset, files.ToArray());

            var path = @".\Resources\TestFiles\" + Guid.NewGuid();

            try
            {
                Directory.CreateDirectory(path);
                string dupFileName = Path.Combine(path, Path.GetFileName(files[0]));
                File.WriteAllText(dupFileName, "");
                ingestManifestCreated.EncryptFiles(path, false);
            }
            catch (AggregateException ax)
            {
                var expectedExcpetion = ax.GetBaseException() as IOException;
                throw expectedExcpetion;
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }


        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestIngestManifestCreateRetry()
        {
            var expected = new IngestManifestData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            IIngestManifest actual = _mediaContext.IngestManifests.CreateAsync(expected.Name).Result;

            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestIngestManifestCreateFailedRetry()
        {
            var expected = new IngestManifestData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.IngestManifests.CreateAsync(expected.Name).Wait();
            }
            catch (AggregateException ax)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                WebException x = (WebException)ax.GetBaseException();
                Assert.AreEqual(fakeException, x);
                throw x;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestIngestManifestCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new IngestManifestData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("ContentKeyAuthorizationPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.IngestManifests.CreateAsync(expected.Name).Wait();
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
        public void TestIngestManifestUpdateRetry()
        {
            var data = new IngestManifestData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestIngestManifestDeleteRetry()
        {
            var data = new IngestManifestData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("ContentKeyAuthorizationPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        #endregion Retry Logic tests

        private static IIngestManifest CreateManifestWithAssetsAndVerifyIt(CloudMediaContext context)
        {
            //Creating empty manifest
            const string manifestName = "Manifest 1";
            IIngestManifest ingestManifest = context.IngestManifests.Create(manifestName);
            Assert.AreEqual(IngestManifestState.Inactive, ingestManifest.State, "Expecting empty manifest to be inactive");
            //Adding manifest asset info with multiple file
            IAsset asset2 = context.Assets.Create(Guid.NewGuid().ToString(), AssetCreationOptions.StorageEncrypted);
            var files2 = new string[2] { TestFile1, TestFile2 };
            IIngestManifestAsset ingestManifestAssetInfo2 = ingestManifest.IngestManifestAssets.Create(asset2, files2);
            Assert.AreEqual(1, asset2.ContentKeys.Count, "No keys associated with asset");
            VerifyManifestAsset(ingestManifestAssetInfo2);

            Assert.AreEqual(2, ingestManifestAssetInfo2.IngestManifestFiles.Count(), "Files collection size is not matching expectations");
            
           return ingestManifest;
        }


        private static void VerifyNameForExitingManifest(IIngestManifest ingestManifest, string newName, string id)
        {
            CloudMediaContext context = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            IIngestManifest updatedIngestManifest = context.IngestManifests.Where(c => c.Id == id).FirstOrDefault();
            Assert.IsNotNull(updatedIngestManifest);
            Assert.AreSame(newName, ingestManifest.Name);
        }

        private IIngestManifest CreateEmptyManifestAndVerifyIt()
        {
            const string manifestName = "TestManifest";
            IIngestManifest ingestManifest = _mediaContext.IngestManifests.Create(manifestName);
            Assert.IsNotNull(ingestManifest);
            Assert.IsFalse(String.IsNullOrEmpty(ingestManifest.Id), "Manifest Id is null or empty");
            Assert.AreEqual(IngestManifestState.Inactive, ingestManifest.State, "Unexpected manifest state.Expected value is InActive");
            Assert.AreEqual(0, ingestManifest.IngestManifestAssets.Count(), "Newly created asset should not contain any assets");
            Assert.AreEqual(0, _mediaContext.IngestManifestAssets.Where(c => c.ParentIngestManifestId == ingestManifest.Id).Count(), "Newly created asset should not contain any assets");
            return ingestManifest;
        }

        private static int GetBlockSize(long fileSize)
        {
            const long kb = 1024;
            const long mb = 1024 * kb;
            const long maxBlocks = 50000;
            const long maxBlockSize = 20 * mb;

            long blocksize = 100 * kb;
            long blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            while (blockCount > maxBlocks - 1)
            {
                blocksize += 100 * kb;
                blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            }

            if (blocksize > maxBlockSize)
            {
                throw new ArgumentException("BlockSize is too big");
            }

            return (int)blocksize;
        }

        private static ConcurrentQueue<KeyValuePair<int, int>> PreapreDownloadQueue(int maxBlockSize, long fileSize, ref int numThreads, out List<string> blockList)
        {
            var queue = new ConcurrentQueue<KeyValuePair<int, int>>();
            blockList = new List<string>();
            int blockId = 0;
            while (fileSize > 0)
            {
                int blockLength = (int)Math.Min(maxBlockSize, fileSize);
                string blockIdString = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "BlockId{0}", blockId.ToString("0000000", CultureInfo.InvariantCulture))));
                var kvp = new KeyValuePair<int, int>(blockId++, blockLength);
                queue.Enqueue(kvp);
                blockList.Add(blockIdString);
                fileSize -= blockLength;
            }

            if (queue.Count < numThreads)
            {
                numThreads = queue.Count;
            }

            return queue;
        }
    }
}