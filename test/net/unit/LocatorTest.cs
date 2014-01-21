using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class LocatorTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void LocatorCRUD()
        {
            var stubed = _mediaContext.Locators.FirstOrDefault();
            Assert.IsNotNull(stubed);
            Assert.IsNotNull(stubed.Asset);
            Assert.IsNotNull(stubed.AccessPolicy);

            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, policy);
            Assert.IsNotNull(locator.AccessPolicy);
            Assert.IsNotNull(locator.Asset);
            locator.Update(DateTime.UtcNow.AddDays(5));
            locator.Update(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5));
            locator.Delete();
            Assert.IsNull(_mediaContext.Locators.Where(c => c.Id == locator.Id).FirstOrDefault());

            var sas = _mediaContext.Locators.CreateSasLocator(asset, policy);
            sas.Delete();

            var sasAsync = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy,DateTime.UtcNow,"Name").Result;
            sasAsync.DeleteAsync().Wait();



        }

        [TestMethod]
        public void AccessPolicyCRUD()
        {
            IAsset asset = _mediaContext.Assets.Create("AccessPolicyCRUD", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            Assert.AreEqual(AccessPermissions.Read, policy.Permissions);
            Assert.AreEqual(TimeSpan.FromDays(1), policy.Duration);
            Assert.AreEqual("AccessPolicyCRUD", policy.Name);
            policy.Delete();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            policy.DeleteAsync();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(-1), AccessPermissions.Read);
            policy.DeleteAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void OnlyOriginLocatorCanBeUpdated()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.Sas, asset, policy);
            locator.Update(DateTime.UtcNow.AddDays(5));


        }

        [TestMethod]
        public void ContentKeyCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16]{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1});
            UpdateDeleteContentKey(key);
            key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            key.DeleteAsync();
        }
        [TestMethod]
        public void ContentKeyCommonEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },Guid.NewGuid().ToString(),contentKeyType:ContentKeyType.CommonEncryption);
            UpdateDeleteContentKey(key);
            
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContentKeyConfigurationEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.ConfigurationEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        public void ContentKeyEnvelopeEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.EnvelopeEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContentKeyStorageEncryptionEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.StorageEncryption);
            UpdateDeleteContentKey(key);

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ContentKeyUrlEncryptionCRUD()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, Guid.NewGuid().ToString(), contentKeyType: ContentKeyType.UrlEncryption);
            UpdateDeleteContentKey(key);

        }

        private static void UpdateDeleteContentKey(IContentKey key)
        {
            key.AuthorizationPolicyId = Guid.NewGuid().ToString();
            key.Update();
            key.AuthorizationPolicyId = Guid.NewGuid().ToString();
            key.UpdateAsync();
            key.Delete();
        }


        [TestMethod]
        public void LinkContentKeyToAsset()
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[16] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            IAsset asset = _mediaContext.Assets.Create("LinkContentKeyToAsset", AssetCreationOptions.StorageEncrypted);
            asset.ContentKeys.Add(key);
            var keys = asset.ContentKeys.ToList();
            Assert.AreEqual(2,keys.Count);
            asset.ContentKeys.Remove(key);
            Assert.AreEqual(1, asset.ContentKeys.Count);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateShortContentKeyShouldFail()
        {
            var key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), new byte[1] { 1});
        }
    }
}