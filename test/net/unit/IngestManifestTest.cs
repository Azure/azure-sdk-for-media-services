//-----------------------------------------------------------------------
// <copyright file="IngestManifestTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
     [TestClass]
    public class IngestManifestTest
    {
        
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        //TODO: Move to separate file if we have more ingest manifest tests
        [TestMethod]
        public void QueryIngestmanifests()
        {
            Assert.IsNull(_mediaContext.IngestManifests.FirstOrDefault());
            Assert.IsNull(_mediaContext.IngestManifests.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        [TestMethod]
        public void QueryIngestManifestAssets()
        {
            Assert.IsNull(_mediaContext.IngestManifestAssets.FirstOrDefault());
            Assert.IsNull(_mediaContext.IngestManifestAssets.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        [TestMethod]
        public void QueryIngestManifestAssetFiles()
        {
            Assert.IsNull(_mediaContext.IngestManifestFiles.FirstOrDefault());
            Assert.IsNull(_mediaContext.IngestManifestFiles.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

          [TestMethod]
         public void CreateUpdateDeleteEmptyIngestManifest()
          {
              var manifest = _mediaContext.IngestManifests.Create(Guid.NewGuid().ToString(), _mediaContext.DefaultStorageAccount.Name);
              Assert.IsNotNull(manifest);
              Assert.IsNotNull(_mediaContext.IngestManifests.Where(c=>c.Id == manifest.Id).FirstOrDefault());
              Assert.IsFalse(String.IsNullOrEmpty(manifest.Id));
              Assert.IsNotNull(manifest.IngestManifestAssets);
              Assert.IsNotNull(manifest.Statistics);
              Assert.IsNotNull(manifest.StorageAccount);
              manifest.Name = Guid.NewGuid().ToString();
              manifest.Update();
              manifest.Delete();
              Assert.IsNull(_mediaContext.IngestManifests.Where(c => c.Id == manifest.Id).FirstOrDefault());

              manifest = _mediaContext.IngestManifests.CreateAsync(Guid.NewGuid().ToString()).Result;
              Assert.IsNotNull(manifest);
              Assert.IsNotNull(_mediaContext.IngestManifests.Where(c => c.Id == manifest.Id).FirstOrDefault());
              Assert.IsFalse(String.IsNullOrEmpty(manifest.Id));
              Assert.IsNotNull(manifest.IngestManifestAssets);
              Assert.IsNotNull(manifest.Statistics);
              manifest.Name = Guid.NewGuid().ToString();
              manifest.UpdateAsync().Wait();
              manifest.DeleteAsync().Wait();
              Assert.IsNull(_mediaContext.IngestManifests.Where(c => c.Id == manifest.Id).FirstOrDefault());
          }

         [TestMethod]
         public void CreateStorageEncryptedEncryptUpdateDeleteIngestManifestAsset()
         {
             AssetCreationOptions assetCreationOptions = AssetCreationOptions.StorageEncrypted;
             CreateEncryptUpdateDelete(assetCreationOptions);
         }

         //TODO: Test is failing complaining that storage encryption key is missing. Rule need to be confirmed
         [Ignore]
         [TestMethod]
         public void CreateCommonEncryptedEncryptUpdateDeleteIngestManifestAsset()
         {
             AssetCreationOptions assetCreationOptions = AssetCreationOptions.CommonEncryptionProtected;
             CreateEncryptUpdateDelete(assetCreationOptions);
         }

         //TODO: Test is failing complaining that storage encryption key is missing. Rule need to be confirmed
         [Ignore]
         [TestMethod]
         public void CreateNoneEncryptedEncryptUpdateDeleteIngestManifestAsset()
         {
             AssetCreationOptions assetCreationOptions = AssetCreationOptions.None;
             CreateEncryptUpdateDelete(assetCreationOptions);
         }
         //TODO: Test is failing complaining that storage encryption key is missing. Rule need to be confirmed
         [Ignore]
         [TestMethod]
         public void CreateEnvelopeEncryptedEncryptUpdateDeleteIngestManifestAsset()
         {
             AssetCreationOptions assetCreationOptions = AssetCreationOptions.EnvelopeEncryptionProtected;
             CreateEncryptUpdateDelete(assetCreationOptions);
         }

         [TestMethod]
         public void IngestManifestFileCRUD()
         {
             var manifest = _mediaContext.IngestManifests.Create(Guid.NewGuid().ToString(), _mediaContext.DefaultStorageAccount.Name);
             Assert.IsNotNull(manifest);
             IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(),AssetCreationOptions.StorageEncrypted);
             var ingestManifestAsset = manifest.IngestManifestAssets.CreateAsync(asset, CancellationToken.None).Result;
             ingestManifestAsset.Delete();
             ingestManifestAsset = manifest.IngestManifestAssets.CreateAsync(asset, CancellationToken.None).Result;
             ingestManifestAsset.DeleteAsync();
             ingestManifestAsset = manifest.IngestManifestAssets.CreateAsync(asset, CancellationToken.None).Result;
             string tempFileName = Path.GetTempFileName();
             try
             {
                 var ingestManifestFile = ingestManifestAsset.IngestManifestFiles.CreateAsync(tempFileName, CancellationToken.None).Result;
                 Assert.IsNull(ingestManifestFile.ErrorDetail);
                 Assert.IsNull(_mediaContext.IngestManifestFiles.Where(c => c.Id != ingestManifestFile.Id).FirstOrDefault());
                 ingestManifestFile.Delete();
                 ingestManifestAsset.Delete();

             }
             finally
             {
                 File.Delete(tempFileName);
             }
         }

         [TestMethod]
         [ExpectedException(typeof(InvalidOperationException))]
         public void ValidateDefaultStorageAccountTryingToCreateManifest()
         {
             var context = Helper.GetMockContextWithNullDefaultStorage();

             IngestManifestCollection collection = new IngestManifestCollection(context);

             try
             {
                 collection.Create("NullStorage");
             }
             catch (InvalidOperationException ex)
             {
                 Assert.AreEqual(StringTable.DefaultStorageAccountIsNull, ex.Message);
                 throw;
             }
         }

         private void CreateEncryptUpdateDelete(AssetCreationOptions assetCreationOptions)
         {
             var manifest = _mediaContext.IngestManifests.Create(Guid.NewGuid().ToString(), _mediaContext.DefaultStorageAccount.Name);
             Assert.IsNotNull(manifest);

             IAsset asset = _mediaContext.Assets.Create(Guid.NewGuid().ToString(), assetCreationOptions);
             string tempFileName = Path.GetTempFileName();
             try
             {
                 var ingestManifestAsset = manifest.IngestManifestAssets.Create(asset, new[] {tempFileName});
                 Assert.IsNotNull(ingestManifestAsset);
                 Assert.IsNotNull(ingestManifestAsset.IngestManifestFiles);
                 Assert.IsNotNull(ingestManifestAsset.Asset);
                 Assert.AreEqual(1, ingestManifestAsset.IngestManifestFiles.Count());
                 var assetfile = ingestManifestAsset.IngestManifestFiles.FirstOrDefault();
                 Assert.IsFalse(string.IsNullOrEmpty(assetfile.Name));

                 Assert.AreEqual(IngestManifestFileState.Pending, assetfile.State);

                 var output = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
                 try
                 {
                     Assert.AreEqual(0, output.GetFiles().Count(), "Expecting 0 files before manifest encryption");
                     File.WriteAllText(tempFileName, Guid.NewGuid().ToString());
                     manifest.EncryptFiles(output.FullName);
                     Assert.AreEqual(1, output.GetFiles().Count(), "Expecting 1 file after manifest encryption");
                     Assert.IsTrue(output.GetFiles().FirstOrDefault().Length > 0);
                 }
                 finally
                 {
                     output.Delete(true);
                 }
                 manifest.Name = Guid.NewGuid().ToString();
                 manifest.Update();
                 assetfile.Delete();
                 ingestManifestAsset.Delete();
                 manifest.Delete();
             }
             finally
             {
                 File.Delete(tempFileName);
             }
         }
    }
}