using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class CollectionQueryTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void QueryStorageAccounts()
        {
            //checking stubbed data
            Assert.IsNotNull(_mediaContext.StorageAccounts.Where(c => c.IsDefault).FirstOrDefault());
            //Should not return for non existing items
            Assert.IsNull(_mediaContext.StorageAccounts.Where(c => c.Name == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        [TestMethod]
        public void QueryNotificationsEndPoint()
        {
            Assert.IsNull(_mediaContext.NotificationEndPoints.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        //TODO: Move to separate file if we have more media processor tests
        [TestMethod]
        public void QueryMediaprocessors()
        {
            //We should have at least one for job testing
            IMediaProcessor firstOrDefault = _mediaContext.MediaProcessors.FirstOrDefault();
            Assert.IsNotNull(firstOrDefault);
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Id));
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Name));
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Version));
            Assert.IsNull(_mediaContext.MediaProcessors.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        //TODO: Move to separate file if we have more job templates tests
        [TestMethod]
        public void QueryJobTemplates()
        {
            //We should have at least one for job testing
            Assert.IsNull(_mediaContext.JobTemplates.FirstOrDefault());
            Assert.IsNull(_mediaContext.JobTemplates.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
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
    }
}
