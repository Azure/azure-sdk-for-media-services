using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    
    
    /// <summary>
    ///This is a test class for AssetDeliveryPolicyCollectionTest and is intended
    ///to contain all AssetDeliveryPolicyCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AssetDeliveryPolicyCollectionTest
    {
        private CloudMediaContext _dataContext;
        private IAssetDeliveryPolicy _policy = null;

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            _policy = Create("e2etest-456");
        }

        [TestCleanup]
        public void CleanupTest()
        {
            _policy.Delete();            
            var deleted = !_dataContext.AssetDeliveryPolicies.Where(p => p.Id == _policy.Id).AsEnumerable().Any();
            Assert.IsTrue(deleted, "ContentKeyAuthorizationPolicyOption was not deleted");
        }

        public IAssetDeliveryPolicy Create(string name)
        {
            string acquisitionUrl = "http://localhost";
            var configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>
            {
                {AssetDeliveryPolicyConfigurationKey.KeyAcquisitionUrl, acquisitionUrl},
                {
                    AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIV, 
                    "Yx4K1t0/AApWC8W0qhTYw9IGYfm5VxC88L9FubGOeOaZ00C4lVB/6fngZZr0rgmKXjI3YHPZQ5nu8LW6Pna8GclG+YGJKdT/LoGzUs9MmvdZ4H9F+zswzMu9e1nk9itAS+rgnyekYtRrgxDx2THqWxkJ8wY9Z6OiBURedxt0mpsaqB1D66pWAkNP5ymk1i6qrTwSDiguWXf9hjp7jRttC4nziz31gZxlRJvbZiSr6xnCXMX88c/LfRJszVCylpen5DFZz/wAbcB10YbDq35nGKKj8CT1jVjGGqPlx8AQRiKgDPlQJ+YsiY5ztJIzs9t4dCaANSZezmBn/u6v8mNB7w=="
                }
            };

            var result = _dataContext.AssetDeliveryPolicies.Create(
                name,
                AssetDeliveryPolicyType.DynamicEnvelopeEncryption,
                AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.SmoothStreaming,
                configuration);

            var check = _dataContext.AssetDeliveryPolicies.Where(p => p.Id == result.Id).AsEnumerable().SingleOrDefault();

            Assert.AreEqual(name, check.Name);
            Assert.AreEqual(acquisitionUrl, check.AssetDeliveryConfiguration[AssetDeliveryPolicyConfigurationKey.KeyAcquisitionUrl]);

            return result;
        }

        [TestMethod]
        public void AssetDeliveryPolicyTestUpdate()
        {
            string newName = "somenewname";
            _policy.Name = newName;
            _policy.Update();

            var check = _dataContext.AssetDeliveryPolicies.Where(p => p.Id == _policy.Id).AsEnumerable().Single();
            Assert.AreEqual(newName, check.Name);
        }

        [TestMethod]
        public void AssetDeliveryPolicyTestAttach()
        {
            var asset = _dataContext.Assets.Create("e2etest-94223", AssetCreationOptions.None);
            asset.DeliveryPolicies.Add(_policy);

            var check = _dataContext.Assets.Where(a => a.Id == asset.Id).Single().DeliveryPolicies[0];
            Assert.AreEqual(_policy.Id, check.Id);

            for (int i = 0; i < asset.DeliveryPolicies.Count; i++)
            {
                asset.DeliveryPolicies.RemoveAt(i);
            }

            asset.Delete();
        }
    }
}
