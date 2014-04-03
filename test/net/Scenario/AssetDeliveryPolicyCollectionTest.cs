using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using System.Net;
using Moq;
using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    
    
    /// <summary>
    ///This is a test class for AssetDeliveryPolicyCollectionTest and is intended
    ///to contain all AssetDeliveryPolicyCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AssetDeliveryPolicyCollectionTest
    {
        private CloudMediaContext _mediaContext;
        private IAssetDeliveryPolicy _policy = null;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();

            _policy = Create("e2etest-AssetDeliverPolicyCollectionTest");
        }

        /*[TestCleanup]
        public void CleanupTest()
        {
            _policy.Delete();            
            var deleted = !_mediaContext.AssetDeliveryPolicies.Where(p => p.Id == _policy.Id).AsEnumerable().Any();
            Assert.IsTrue(deleted, "AssetDeliveryPolicy was not deleted");
        }*/

        public IAssetDeliveryPolicy Create(string name)
        {
            string acquisitionUrl = "http://localhost";
            string envelopeEncryptionIV = "Yx4K1t0/AApWC8W0qhTYw9IGYfm5VxC88L9FubGOeOaZ00C4lVB/6fngZZr0rgmKXjI3YHPZQ5nu8LW6Pna8GclG+YGJKdT/LoGzUs9MmvdZ4H9F+zswzMu9e1nk9itAS+rgnyekYtRrgxDx2THqWxkJ8wY9Z6OiBURedxt0mpsaqB1D66pWAkNP5ymk1i6qrTwSDiguWXf9hjp7jRttC4nziz31gZxlRJvbZiSr6xnCXMX88c/LfRJszVCylpen5DFZz/wAbcB10YbDq35nGKKj8CT1jVjGGqPlx8AQRiKgDPlQJ+YsiY5ztJIzs9t4dCaANSZezmBn/u6v8mNB7w==";
            var configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>
            {
                {AssetDeliveryPolicyConfigurationKey.KeyAcquisitionUrl, acquisitionUrl},
                {AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIV, envelopeEncryptionIV}
            };

            var result = _mediaContext.AssetDeliveryPolicies.Create(
                name,
                AssetDeliveryPolicyType.DynamicEnvelopeEncryption,
                AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.SmoothStreaming,
                configuration);

            var check = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == result.Id).AsEnumerable().SingleOrDefault();

            Assert.AreEqual(name, check.Name);
            Assert.AreEqual(acquisitionUrl, check.AssetDeliveryConfiguration[AssetDeliveryPolicyConfigurationKey.KeyAcquisitionUrl]);
            Assert.AreEqual(envelopeEncryptionIV, check.AssetDeliveryConfiguration[AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIV]);

            return result;
        }

        [TestMethod]
        public void AssetDeliveryPolicyTestUpdate()
        {
            string newName = "somenewname";
            _policy.Name = newName;
            _policy.Update();

            var check = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == _policy.Id).AsEnumerable().Single();
            Assert.AreEqual(newName, check.Name);
        }

        [TestMethod]
        public void AssetDeliveryPolicyTestAttach()
        {
            var asset = _mediaContext.Assets.Create("e2etest-94223", AssetCreationOptions.None);
            asset.DeliveryPolicies.Add(_policy);

            asset = _mediaContext.Assets.Where(a => a.Id == asset.Id).Single();
            var check = asset.DeliveryPolicies[0];
            Assert.AreEqual(_policy.Id, check.Id);
            Assert.AreEqual(1, asset.DeliveryPolicies.Count);

            for (int i = 0; i < asset.DeliveryPolicies.Count; i++)
            {
                asset.DeliveryPolicies.RemoveAt(i);
            }

            asset.Delete();
        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestAssetDeliveryPolicyCreateRetry()
        {
            var expected = new AssetDeliveryPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("AssetDeliveryPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var task = _mediaContext.AssetDeliveryPolicies.CreateAsync(expected.Name, AssetDeliveryPolicyType.None, AssetDeliveryProtocol.None, null);
            task.Wait();
            IAssetDeliveryPolicy actual = task.Result;

            Assert.AreEqual(expected.Name, actual.Name);
            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestAssetDeliveryPolicyCreateFailedRetry()
        {
            var expected = new AssetDeliveryPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("AssetDeliveryPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.AssetDeliveryPolicies.CreateAsync(expected.Name, AssetDeliveryPolicyType.None, AssetDeliveryProtocol.None, null).Wait();
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
        public void TestAssetDeliveryPolicyCreateFailedRetryMessageLengthLimitExceeded()
        {
            var expected = new AssetDeliveryPolicyData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.MessageLengthLimitExceeded);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("AssetDeliveryPolicies", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.AssetDeliveryPolicies.CreateAsync(expected.Name, AssetDeliveryPolicyType.None, AssetDeliveryProtocol.None, null).Wait();
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
        public void TestAssetDeliveryPolicyUpdateRetry()
        {
            var data = new AssetDeliveryPolicyData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("AssetDeliveryPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Update();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestAssetDeliveryPolicyDeleteRetry()
        {
            var data = new AssetDeliveryPolicyData { Name = "testData" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("AssetDeliveryPolicies", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            data.Delete();

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }
        #endregion Retry Logic tests
    }
}
