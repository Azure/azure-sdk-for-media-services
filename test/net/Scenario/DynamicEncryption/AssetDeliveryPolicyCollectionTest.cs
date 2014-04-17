using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using System.Net;
using Moq;
using System;
using System.Security.Cryptography;
using System.Data.Services.Client;

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

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        private string GetRandomIV(int size = 16)
        {
            byte[] ivBytes = GetRandomData(size);

            // Note that BitConverter.ToString returns the data as a Hex string but uses '-' in between
            // each Hex digit (like 00-0F-ED).  We don't want that so remove the dashes.
            return BitConverter.ToString(ivBytes).Replace("-", string.Empty);
        }

        private byte[] GetRandomData(int size)
        {
            byte[] randomBytes = new byte[size];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            return randomBytes;
        }

        private IAssetDeliveryPolicy CreateEnvelopePolicy(string name)
        {
            string acquisitionUrl = "http://localhost";

            // Waiting for Hex IVs to be enabled in our test environments
            // string envelopeEncryptionIV = GetRandomIV();
            // AssetDeliveryPolicyConfigurationKey envelopeIVKey = AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIV;

            AssetDeliveryPolicyConfigurationKey envelopeIVKey = AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIVAsBase64;
            string envelopeEncryptionIV = Convert.ToBase64String(GetRandomData(16));
            var configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>
            {
                {AssetDeliveryPolicyConfigurationKey.EnvelopeKeyAcquisitionUrl, acquisitionUrl},
                {envelopeIVKey, envelopeEncryptionIV}
            };

            var result = _mediaContext.AssetDeliveryPolicies.Create(
                name,
                AssetDeliveryPolicyType.DynamicEnvelopeEncryption,
                AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.SmoothStreaming,
                configuration);

            var check = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == result.Id).AsEnumerable().SingleOrDefault();

            Assert.AreEqual(name, check.Name);
            Assert.AreEqual(acquisitionUrl, check.AssetDeliveryConfiguration[AssetDeliveryPolicyConfigurationKey.EnvelopeKeyAcquisitionUrl]);
            Assert.AreEqual(envelopeEncryptionIV, check.AssetDeliveryConfiguration[envelopeIVKey]);

            return result;
        }

        private IAssetDeliveryPolicy CreatePlayReadyPolicy(string name)
        {
            string acquisitionUrl = "http://localhost";

            var configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>
            {
                {AssetDeliveryPolicyConfigurationKey.PlayReadyLicenseAcquisitionUrl, acquisitionUrl},
            };

            var result = _mediaContext.AssetDeliveryPolicies.Create(
                name,
                AssetDeliveryPolicyType.DynamicCommonEncryption,
                AssetDeliveryProtocol.SmoothStreaming,
                configuration);

            var check = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == result.Id).AsEnumerable().SingleOrDefault();

            Assert.AreEqual(name, check.Name);
            Assert.AreEqual(acquisitionUrl, check.AssetDeliveryConfiguration[AssetDeliveryPolicyConfigurationKey.PlayReadyLicenseAcquisitionUrl]);

            return result;
        }

        private void DeleteDeliveryPolicyAndVerify(IAssetDeliveryPolicy policy)
        {
            string id = policy.Id;

            policy.Delete();

            IAssetDeliveryPolicy policyToCheck = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == id).FirstOrDefault();
            Assert.IsNull(policyToCheck);
        }

        [TestMethod]
        public void AssetDeliveryPolicyTestUpdate()
        {
            IAssetDeliveryPolicy policy = CreateEnvelopePolicy("AssetDeliveryPolicyTestUpdate");

            string newName = "somenewname";
            policy.Name = newName;
            policy.Update();

            var check = _mediaContext.AssetDeliveryPolicies.Where(p => p.Id == policy.Id).AsEnumerable().Single();
            Assert.AreEqual(newName, check.Name);

            DeleteDeliveryPolicyAndVerify(policy);
        }

        [TestMethod]
        public void EnvelopeAssetDeliveryPolicyTestAttach()
        {
            var asset = _mediaContext.Assets.Create("Asset for EnvelopeAssetDeliveryPolicyTestAttach", AssetCreationOptions.None);

            var contentKey = _mediaContext.ContentKeys.Create(Guid.NewGuid(), GetRandomData(16), "Content Key for EnvelopeAssetDeliveryPolicyTestAttach", ContentKeyType.EnvelopeEncryption);

            asset.ContentKeys.Add(contentKey);

            IAssetDeliveryPolicy policy = CreateEnvelopePolicy("Policy for EnvelopeAssetDeliveryPolicyTestAttach");
            asset.DeliveryPolicies.Add(policy);

            asset = _mediaContext.Assets.Where(a => a.Id == asset.Id).Single();
            var check = asset.DeliveryPolicies[0];
            Assert.AreEqual(policy.Id, check.Id);
            Assert.AreEqual(1, asset.DeliveryPolicies.Count);

            List<IAssetDeliveryPolicy> policies = asset.DeliveryPolicies.ToList();
            foreach(IAssetDeliveryPolicy current in policies)
            {
                asset.DeliveryPolicies.Remove(current);
                current.Delete();
            }

            asset.Delete();
        }

        [TestMethod]
        [Ignore] // Not enabled in REST yet
        public void PlayReadyAssetDeliveryPolicyTestAttach()
        {
            var asset = _mediaContext.Assets.Create("Asset for PlayReadyAssetDeliveryPolicyTestAttach", AssetCreationOptions.None);

            var contentKey = _mediaContext.ContentKeys.Create(Guid.NewGuid(), GetRandomData(16), "Content Key for PlayReadyAssetDeliveryPolicyTestAttach", ContentKeyType.CommonEncryption);

            asset.ContentKeys.Add(contentKey);

            IAssetDeliveryPolicy policy = CreatePlayReadyPolicy("Policy for PlayReadyAssetDeliveryPolicyTestAttach");
            asset.DeliveryPolicies.Add(policy);

            asset = _mediaContext.Assets.Where(a => a.Id == asset.Id).Single();
            var check = asset.DeliveryPolicies[0];
            Assert.AreEqual(policy.Id, check.Id);
            Assert.AreEqual(1, asset.DeliveryPolicies.Count);

            List<IAssetDeliveryPolicy> policies = asset.DeliveryPolicies.ToList();
            foreach (IAssetDeliveryPolicy current in policies)
            {
                asset.DeliveryPolicies.Remove(current);
                current.Delete();
            }

            asset.Delete();
        }

        [TestMethod]
        [Ignore]
        public void FailToAttachPolicyIfCommonContentKeyNotPresent()
        {
            var asset = _mediaContext.Assets.Create("Asset for FailToAttachPolicyIfCommonContentKeyNotPresent", AssetCreationOptions.None);

            // Do not create or attach a content key

            IAssetDeliveryPolicy policy = CreatePlayReadyPolicy("Policy for FailToAttachPolicyIfCommonContentKeyNotPresent");

            try
            {
                asset.DeliveryPolicies.Add(policy);
                Assert.Fail("Expected DataServiceRequestException didn't occur.");
            }
            catch (DataServiceRequestException e)
            {
                Assert.IsTrue(e.ToString().Contains("Cannot set an AssetDeliveryPolicy specifying AssetDeliveryPolicyType.DynamicCommonEncryption when no ContentKey with ContentKeyType.CommonEncryption is linked to it"));

                throw;
            }
            finally
            {
                asset.Delete();
                policy.Delete();
            }
        }

        [TestMethod]
        [Ignore]
        public void FailToAttachPolicyIfRequiredEnvelopeKeyNotPresent()
        {
            var asset = _mediaContext.Assets.Create("Asset for FailToAttachPolicyIfRequiredEnvelopeKeyNotPresent", AssetCreationOptions.None);

            // Do not create or attach a content key

            IAssetDeliveryPolicy policy = CreateEnvelopePolicy("Policy for FailToAttachPolicyIfRequiredEnvelopeKeyNotPresent");

            try
            {
                asset.DeliveryPolicies.Add(policy);
                Assert.Fail("Expected DataServiceRequestException didn't occur.");
            }
            catch (DataServiceRequestException e)
            {
                Assert.IsTrue(e.ToString().Contains("Cannot set an AssetDeliveryPolicy specifying AssetDeliveryPolicyType.DynamicEnvelopeEncryption when no ContentKey with ContentKeyType.EnvelopeEncryption is linked to it"));

                throw;
            }
            finally
            {
                asset.Delete();
                policy.Delete();
            }
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
