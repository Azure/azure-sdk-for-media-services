//-----------------------------------------------------------------------
// <copyright file="AssetEncryptionStatusTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public partial class AssetEncryptionStateTests
    {
        private CloudMediaContext _mediaContext;
        private static string[] _filePaths = new[] { WindowsAzureMediaServicesTestConfiguration.SmallIsm, WindowsAzureMediaServicesTestConfiguration.SmallIsmc, WindowsAzureMediaServicesTestConfiguration.SmallIsmv };

        /// <summary>
        ///     Gets or sets the test context which provides information about and functionality for the current test run.
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
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void TestGetEncryptionState()
        {
            IAsset asset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);
            AssetDeliveryProtocol protocolsToSet = AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.SmoothStreaming | AssetDeliveryProtocol.Dash;
            Dictionary<AssetDeliveryPolicyConfigurationKey, string> configuration = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>()
            {
                {AssetDeliveryPolicyConfigurationKey.EnvelopeBaseKeyAcquisitionUrl, "https://www.test.com/"},
                {AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIVAsBase64, Convert.ToBase64String(ContentKeyTests.GetRandomBuffer(16))}
            };
            IAssetDeliveryPolicy policy = _mediaContext.AssetDeliveryPolicies.Create("Test Policy", AssetDeliveryPolicyType.DynamicEnvelopeEncryption, protocolsToSet, configuration);
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyTests.GetRandomBuffer(16), "Test key", ContentKeyType.EnvelopeEncryption);

            asset.ContentKeys.Add(key);
            asset.DeliveryPolicies.Add(policy);

            AssetEncryptionState state = asset.GetEncryptionState(protocolsToSet);
            Assert.AreEqual(AssetEncryptionState.DynamicEnvelopeEncryption, state);

            state = asset.GetEncryptionState(AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.Hds);
            Assert.AreEqual(AssetEncryptionState.NoSinglePolicyApplies, state);

            state = asset.GetEncryptionState(AssetDeliveryProtocol.Hds);
            Assert.AreEqual(AssetEncryptionState.BlockedByPolicy, state);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void ValidateEncryptionStatusOfEmptyAsset()
        {
            IAsset asset = _mediaContext.Assets.Create("Empty Asset", AssetCreationOptions.None);
            Assert.AreEqual(0, asset.AssetFiles.Count());
            Assert.AreEqual(false, asset.IsStreamable);
            Assert.AreEqual(AssetType.Unknown, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        public void ValidateEffectiveEncryptionStatusOfMP4()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.None);
            Assert.AreEqual(false, asset.IsStreamable);
            Assert.AreEqual(AssetType.MP4, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.Unsupported);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedMP4()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(false, asset.IsStreamable);
            Assert.AreEqual(AssetType.MP4, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.Unsupported);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfWmv()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv, AssetCreationOptions.None);
            Assert.AreEqual(false, asset.IsStreamable);
            Assert.AreEqual(AssetType.Unknown, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.Unsupported);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallWmv.wmv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedWmv()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv, AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(false, asset.IsStreamable);
            Assert.AreEqual(AssetType.Unknown, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.Unsupported);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        public void ValidateEffectiveEncryptionStatusOfMultiBitRateMP4()
        {
            IAsset asset = CreateMbrMp4Asset(AssetCreationOptions.None);
            Assert.AreEqual(true, asset.IsStreamable);
            Assert.AreEqual(AssetType.MultiBitrateMP4, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.ClearOutput);

            ValidateAssetEncryptionState(asset, testCases);

            SetupEnvelopePolicy(asset, AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.SmoothStreaming);
            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\SmallMp41.mp4", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedMultiBitRateMP4()
        {
            IAsset asset = CreateMbrMp4Asset(AssetCreationOptions.StorageEncrypted);
            Assert.AreEqual(true, asset.IsStreamable);
            Assert.AreEqual(AssetType.MultiBitrateMP4, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);

            // There is no asset delivery policy so streaming should be blocked
            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.StorageEncryptedWithNoDeliveryPolicy);
            ValidateAssetEncryptionState(asset, testCases);

            // now add a clear policy for Dash and retest
            SetupClearPolicy(asset, AssetDeliveryProtocol.Dash);

            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            // now add a clear policy for Hls and retest
            SetupEnvelopePolicy(asset, AssetDeliveryProtocol.HLS);

            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            // now add a clear policy for Smooth and retest
            SetupCommonPolicy(asset, AssetDeliveryProtocol.SmoothStreaming);

            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfSmooth()
        {
            IAsset asset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);

            Assert.AreEqual(true, asset.IsStreamable);
            Assert.AreEqual(AssetType.SmoothStreaming, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.ClearOutput);
            ValidateAssetEncryptionState(asset, testCases);

            SetupCommonPolicy(asset, AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.SmoothStreaming);

            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [Ignore] // Media Processor Windows Azure Media Encryptor deprecated
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        [DeploymentItem(@"Configuration\PlayReady Protection_ContentKey.xml", "Configuration")]
        public void ValidateEffectiveEncryptionStatusOfPlayReadyProtectedSmooth()
        {
            IAsset asset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);

            IAsset playReadyProtectedSmoothAsset = CreatePlayReadyProtectedSmoothAsset(asset);
            Assert.AreEqual(true, playReadyProtectedSmoothAsset.IsStreamable);
            Assert.AreEqual(AssetType.SmoothStreaming, playReadyProtectedSmoothAsset.AssetType);
            Assert.AreEqual(AssetCreationOptions.CommonEncryptionProtected, playReadyProtectedSmoothAsset.Options);
            Assert.AreEqual(1, playReadyProtectedSmoothAsset.ContentKeys.Count);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS;

            Assert.AreEqual(ContentKeyType.CommonEncryption, playReadyProtectedSmoothAsset.ContentKeys[0].ContentKeyType);

            ValidateAssetEncryptionState(playReadyProtectedSmoothAsset, protocolsToTest, AssetEncryptionState.StaticCommonEncryption);

            CleanupAsset(playReadyProtectedSmoothAsset);
            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedSmooth()
        {
            IAsset asset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.StorageEncrypted);

            Assert.IsTrue(asset.IsStreamable);
            Assert.AreEqual(AssetType.SmoothStreaming, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);

            // There is no asset delivery policy so streaming should be blocked
            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.StorageEncryptedWithNoDeliveryPolicy);
            ValidateAssetEncryptionState(asset, testCases);

            SetupEnvelopePolicy(asset, AssetDeliveryProtocol.HLS);
            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);
            ValidateAssetEncryptionState(asset, testCases);

            SetupCommonPolicy(asset, AssetDeliveryProtocol.SmoothStreaming | AssetDeliveryProtocol.Dash);
            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);
            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [Ignore] // Media Processor Windows Azure Media Encryptor deprecated
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Configuration\PlayReady Protection_ContentKey.xml", "Configuration")]
        [DeploymentItem(@"Configuration\Smooth Streams to Encrypted Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfHls()
        {
            IAsset smoothAsset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);
            IAsset clearHls = CreateHlsFromSmoothAsset(smoothAsset, AssetCreationOptions.None);

            CleanupAsset(smoothAsset);

            Assert.AreEqual(true, clearHls.IsStreamable);
            Assert.AreEqual(AssetType.MediaServicesHLS, clearHls.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, clearHls.Options);

            ValidateAssetEncryptionState(clearHls, AssetDeliveryProtocol.HLS, AssetEncryptionState.ClearOutput);

            CleanupAsset(clearHls);
        }

        [TestMethod]
        [Ignore] // Media Processor Windows Azure Media Encryptor deprecated
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Configuration\PlayReady Protection_ContentKey.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfPlayReadyProtectedHls()
        {
            IAsset smoothAsset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);
            IAsset playReadyProtectedSmoothAsset = CreatePlayReadyProtectedSmoothAsset(smoothAsset);
            IAsset playReadyProtectedHls = CreateHlsFromSmoothAsset(playReadyProtectedSmoothAsset, AssetCreationOptions.CommonEncryptionProtected);

            CleanupAsset(smoothAsset);
            CleanupAsset(playReadyProtectedSmoothAsset);

            Assert.AreEqual(true, playReadyProtectedHls.IsStreamable);
            Assert.AreEqual(AssetType.MediaServicesHLS, playReadyProtectedHls.AssetType);
            Assert.AreEqual(AssetCreationOptions.CommonEncryptionProtected, playReadyProtectedHls.Options);

            // Enable these once bug 
            //Assert.AreEqual(1, playReadyProtectedHls.ContentKeys.Count);
            //Assert.AreEqual(ContentKeyType.CommonEncryption, playReadyProtectedHls.ContentKeys[0].ContentKeyType);

            ValidateAssetEncryptionState(playReadyProtectedHls, AssetDeliveryProtocol.HLS, AssetEncryptionState.StaticCommonEncryption);

            CleanupAsset(playReadyProtectedHls);
        }

        [TestMethod]
        [Ignore] // Media Processor Windows Azure Media Encryptor deprecated
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedHls()
        {
            IAsset smoothAsset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);
            IAsset playReadyProtectedSmoothAsset = CreatePlayReadyProtectedSmoothAsset(smoothAsset);
            IAsset storageEncryptedHls = CreateHlsFromSmoothAsset(smoothAsset, AssetCreationOptions.StorageEncrypted);

            CleanupAsset(smoothAsset);

            Assert.AreEqual(true, storageEncryptedHls.IsStreamable);
            Assert.AreEqual(AssetType.MediaServicesHLS, storageEncryptedHls.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, storageEncryptedHls.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.StorageEncryptedWithNoDeliveryPolicy);
            ValidateAssetEncryptionState(storageEncryptedHls, testCases);

            SetupClearPolicy(storageEncryptedHls, AssetDeliveryProtocol.HLS);
            UpdateTestCasesForAddedPolicy(testCases, storageEncryptedHls.DeliveryPolicies);
            ValidateAssetEncryptionState(storageEncryptedHls, testCases);

            CleanupAsset(storageEncryptedHls);
        }

        [TestMethod]
        [Ignore] // Media Processor Windows Azure Media Encryptor deprecated
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\Smooth Streams to Encrypted Apple HTTP Live Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfEnvelopeProtectedHls()
        {
            IAsset smoothAsset = JobTests.CreateSmoothAsset(_mediaContext, _filePaths, AssetCreationOptions.None);
            IAsset envelopeHls = CreateHlsFromSmoothAsset(smoothAsset, AssetCreationOptions.EnvelopeEncryptionProtected);

            CleanupAsset(smoothAsset);

            Assert.AreEqual(true, envelopeHls.IsStreamable);
            Assert.AreEqual(AssetType.MediaServicesHLS, envelopeHls.AssetType);
            Assert.AreEqual(AssetCreationOptions.EnvelopeEncryptionProtected, envelopeHls.Options);

            ValidateAssetEncryptionState(envelopeHls, AssetDeliveryProtocol.HLS, AssetEncryptionState.StaticEnvelopeEncryption);

            CleanupAsset(envelopeHls);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        public void ValidateEffectiveEncryptionStatusOfSimulatedLiveStream()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallIsm, AssetCreationOptions.None);

            Assert.IsTrue(asset.IsStreamable);
            Assert.AreEqual(AssetType.SmoothStreaming, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.None, asset.Options);

            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.ClearOutput);
            ValidateAssetEncryptionState(asset, testCases);

            SetupCommonPolicy(asset, AssetDeliveryProtocol.HLS | AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.SmoothStreaming);

            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);

            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Media\Small.ism", "Media")]
        [DeploymentItem(@"Media\Small.ismc", "Media")]
        [DeploymentItem(@"Media\Small.ismv", "Media")]
        public void ValidateEffectiveEncryptionStatusOfStorageEncryptedSimulatedLiveStream()
        {
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallIsm, AssetCreationOptions.StorageEncrypted);

            Assert.IsTrue(asset.IsStreamable);
            Assert.AreEqual(AssetType.SmoothStreaming, asset.AssetType);
            Assert.AreEqual(AssetCreationOptions.StorageEncrypted, asset.Options);

            // There is no asset delivery policy so streaming should be blocked
            AssetDeliveryProtocol protocolsToTest = AssetDeliveryProtocol.SmoothStreaming |
                                                    AssetDeliveryProtocol.Dash |
                                                    AssetDeliveryProtocol.HLS |
                                                    AssetDeliveryProtocol.Hds;

            List<TestCase> testCases = GetTestsCasesForProtocolCombination(protocolsToTest, AssetEncryptionState.StorageEncryptedWithNoDeliveryPolicy);
            ValidateAssetEncryptionState(asset, testCases);

            SetupEnvelopePolicy(asset, AssetDeliveryProtocol.HLS);
            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);
            ValidateAssetEncryptionState(asset, testCases);

            SetupCommonPolicy(asset, AssetDeliveryProtocol.SmoothStreaming | AssetDeliveryProtocol.Dash);
            UpdateTestCasesForAddedPolicy(testCases, asset.DeliveryPolicies);
            ValidateAssetEncryptionState(asset, testCases);

            CleanupAsset(asset);
        }

        private static void CleanupAsset(IAsset asset)
        {
            foreach (IAssetDeliveryPolicy policy in asset.DeliveryPolicies.ToList())
            {
                asset.DeliveryPolicies.Remove(policy);
                policy.Delete();
            }

            asset.Delete();
        }

        private void SetupClearPolicy(IAsset asset, AssetDeliveryProtocol protocol)
        {
            IAssetDeliveryPolicy policy = _mediaContext.AssetDeliveryPolicies.Create("Clear Policy", AssetDeliveryPolicyType.NoDynamicEncryption, protocol, null);

            asset.DeliveryPolicies.Add(policy);
        }

        private void SetupEnvelopePolicy(IAsset asset, AssetDeliveryProtocol protocol)
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyTests.GetRandomBuffer(16), "Envelope Encryption Key", ContentKeyType.EnvelopeEncryption);
            asset.ContentKeys.Add(key);


            Dictionary<AssetDeliveryPolicyConfigurationKey, string> config = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>()
            {
                {AssetDeliveryPolicyConfigurationKey.EnvelopeBaseKeyAcquisitionUrl, "https://fakeKeyDeliveryurl.com/"},
                {AssetDeliveryPolicyConfigurationKey.EnvelopeEncryptionIVAsBase64, Convert.ToBase64String(Guid.NewGuid().ToByteArray())} // TODO: Remove this once no IV is supported
            };

            IAssetDeliveryPolicy policy = _mediaContext.AssetDeliveryPolicies.Create("Clear Policy", AssetDeliveryPolicyType.DynamicEnvelopeEncryption, protocol, config);

            asset.DeliveryPolicies.Add(policy);
        }

        private void SetupCommonPolicy(IAsset asset, AssetDeliveryProtocol protocol)
        {
            IContentKey key = _mediaContext.ContentKeys.Create(Guid.NewGuid(), ContentKeyTests.GetRandomBuffer(16), "Common Encryption Key", ContentKeyType.CommonEncryption);
            asset.ContentKeys.Add(key);

            Dictionary<AssetDeliveryPolicyConfigurationKey, string> config = new Dictionary<AssetDeliveryPolicyConfigurationKey, string>()
            {
                {AssetDeliveryPolicyConfigurationKey.PlayReadyLicenseAcquisitionUrl, "https://fakeKeyDeliveryurl.com/PlayReady"}
            };

            IAssetDeliveryPolicy policy = _mediaContext.AssetDeliveryPolicies.Create("Clear Policy", AssetDeliveryPolicyType.DynamicCommonEncryption, protocol, config);

            asset.DeliveryPolicies.Add(policy);
        }

        private IAsset CreateMbrMp4Asset(AssetCreationOptions options)
        {
            IMediaProcessor encoder = JobTests.GetEncoderMediaProcessor(_mediaContext);
            IJob job = _mediaContext.Jobs.Create("Job for ValidateEffectiveEncryptionStatusOfMultiBitRateMP4");

            ITask adpativeBitrateTask = job.Tasks.AddNew("MP4 to Adaptive Bitrate Task",
                encoder,
                "H264 Multiple Bitrate 720p",
                TaskOptions.None);

            // Specify the input Asset
            IAsset asset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.None);
            adpativeBitrateTask.InputAssets.Add(asset);

            // Add an output asset to contain the results of the job. 
            // This output is specified as AssetCreationOptions.None, which 
            // means the output asset is in the clear (unencrypted).
            IAsset abrAsset = adpativeBitrateTask.OutputAssets.AddNew("Multibitrate MP4s", options);

            job.Submit();

            job.GetExecutionProgressTask(CancellationToken.None).Wait();

            job.Refresh();

            return job.OutputMediaAssets[0];
        }

        private IAsset CreatePlayReadyProtectedSmoothAsset(IAsset clearSmoothAssetToProtect)
        {
            Guid keyId = Guid.NewGuid();
            byte[] keyValue = ContentKeyTests.GetRandomBuffer(16);
            string encryptionConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.PlayReadyConfigWithContentKey);
            encryptionConfiguration = JobTests.UpdatePlayReadyConfigurationXML(keyId, keyValue, new Uri("http://www.fakeurl.com"), encryptionConfiguration);

            IJob job = _mediaContext.Jobs.Create("Add PlayReady Protection Job");
            IMediaProcessor mediaProcessor = JobTests.GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);
            ITask task = job.Tasks.AddNew("PlayReady Encryption Task", mediaProcessor, encryptionConfiguration, TaskOptions.ProtectedConfiguration);
            task.InputAssets.Add(clearSmoothAssetToProtect);
            task.OutputAssets.AddNew("PlayReady Protected Smooth", AssetCreationOptions.CommonEncryptionProtected);

            job.Submit();

            job.GetExecutionProgressTask(CancellationToken.None).Wait();

            job.Refresh();

            return job.OutputMediaAssets[0];
        }

        private IAsset CreateHlsFromSmoothAsset(IAsset sourceAsset, AssetCreationOptions options)
        {
            IJob job = _mediaContext.Jobs.Create("Smooth to Hls Job");
            IMediaProcessor mediaProcessor = JobTests.GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpEncryptorName);

            string smoothToHlsConfiguration = null;
            if (options == AssetCreationOptions.EnvelopeEncryptionProtected)
            {
                smoothToHlsConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.SmoothToEncryptHlsConfig);
            }
            else
            {
                smoothToHlsConfiguration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.SmoothToHlsConfig);
            }

            ITask task = job.Tasks.AddNew("Smooth to Hls conversion task", mediaProcessor, smoothToHlsConfiguration, TaskOptions.None);
            task.InputAssets.Add(sourceAsset);
            task.OutputAssets.AddNew("JobOutput", options);
            job.Submit();

            job.GetExecutionProgressTask(CancellationToken.None).Wait();

            job.Refresh();

            return job.OutputMediaAssets[0];
        }

        private void ValidateAssetEncryptionState(IAsset asset, AssetDeliveryProtocol protocolsToTest, AssetEncryptionState expectedState)
        {
            AssetEncryptionState actualState = asset.GetEncryptionState(protocolsToTest);

            Assert.AreEqual(expectedState, actualState);
        }

        private void ValidateAssetEncryptionState(IAsset asset, List<TestCase> testCases)
        {
            foreach (TestCase testCase in testCases)
            {
                ValidateAssetEncryptionState(asset, testCase.ProtocolsToTest, testCase.ExpectedState);
            }
        }

        class TestCase
        {
            public TestCase(AssetDeliveryProtocol protocolsToTest, AssetEncryptionState expectedState)
            {
                ProtocolsToTest = protocolsToTest;
                ExpectedState = expectedState;
            }

            public AssetDeliveryProtocol ProtocolsToTest { get; set; }
            public AssetEncryptionState ExpectedState { get; set; }
        }

        private void AddCombinations(List<AssetDeliveryProtocol> output, List<AssetDeliveryProtocol> protocolsToCombineList, Stack<AssetDeliveryProtocol> workingStack, int offset, int length)
        {
            if (length == 0)
            {
                AssetDeliveryProtocol valueToAdd = AssetDeliveryProtocol.None;

                foreach (AssetDeliveryProtocol protocol in workingStack.ToList())
                {
                    valueToAdd |= protocol;
                }

                output.Add(valueToAdd);
            }
            else
            {
                for (int i = offset; i <= protocolsToCombineList.Count - length; ++i)
                {
                    workingStack.Push(protocolsToCombineList[i]);
                    AddCombinations(output, protocolsToCombineList, workingStack, i + 1, length - 1);
                    workingStack.Pop();
                }
            }
        }

        private List<AssetDeliveryProtocol> GetAllCombinationsOfDeliveryProtocol(AssetDeliveryProtocol protocols)
        {
            //
            //  Split the flags enumeration into single flag values.  Meaning if they pass in (AssetDeliveryProtocol.Dash | AssetDeliveryProtocol.HLS)
            //  then return a list with two entries ([0] = AssetDeliveryProtocol.Dash, [1] = AssetDeliveryProtocol.HLS).  Used as input to create
            //  all of the combinations with
            //
            List<AssetDeliveryProtocol> protocolsToCombineList = GetIndividualProtocols(protocols);
            Stack<AssetDeliveryProtocol> workingStack = new Stack<AssetDeliveryProtocol>();

            List<AssetDeliveryProtocol> returnValue = new List<AssetDeliveryProtocol>();

            for (int x = 1; x <= protocolsToCombineList.Count; x++)
            {
                AddCombinations(returnValue, protocolsToCombineList, workingStack, 0, x);
            }

            return returnValue;
        }

        private List<TestCase> GetTestsCasesForProtocolCombination(AssetDeliveryProtocol protocols, AssetEncryptionState expectedState)
        {
            List<TestCase> testCases = new List<TestCase>();

            List<AssetDeliveryProtocol> combinationList = GetAllCombinationsOfDeliveryProtocol(protocols);

            foreach (AssetDeliveryProtocol combination in combinationList)
            {
                testCases.Add(new TestCase(combination, expectedState));
            }

            return testCases;
        }

        static AssetDeliveryProtocol[] _allValues = (AssetDeliveryProtocol[])Enum.GetValues(typeof(AssetDeliveryProtocol));
        internal static List<AssetDeliveryProtocol> GetIndividualProtocols(AssetDeliveryProtocol protocolsToSplit)
        {
            List<AssetDeliveryProtocol> protocolList = new List<AssetDeliveryProtocol>();

            foreach (AssetDeliveryProtocol protocol in _allValues)
            {
                if ((protocol == AssetDeliveryProtocol.None) || (protocol == AssetDeliveryProtocol.All))
                {
                    continue;
                }

                if (protocolsToSplit.HasFlag(protocol))
                {
                    protocolList.Add(protocol);
                }
            }

            return protocolList;
        }

        private AssetEncryptionState DecideBetweenBlockedOrMultiplePolicies(AssetDeliveryProtocol protocolsToCheck, IList<IAssetDeliveryPolicy> policies)
        {
            List<AssetDeliveryProtocol> individualProtocols = GetIndividualProtocols(protocolsToCheck);

            bool partialMatch = false;

            foreach (AssetDeliveryProtocol protocol in individualProtocols)
            {
                if (policies.Any(p => p.AssetDeliveryProtocol.HasFlag(protocol)))
                {
                    partialMatch = true;
                    break;
                }
            }

            if (partialMatch)
            {
                return AssetEncryptionState.NoSinglePolicyApplies;
            }
            else
            {
                return AssetEncryptionState.BlockedByPolicy;
            }
        }

        private void UpdateTestCasesForAddedPolicy(List<TestCase> testCases, IList<IAssetDeliveryPolicy> policies)
        {
            foreach (TestCase testCase in testCases)
            {
                IAssetDeliveryPolicy policy = policies.Where(p => p.AssetDeliveryProtocol.HasFlag(testCase.ProtocolsToTest)).SingleOrDefault();

                if (policy == null)
                {
                    testCase.ExpectedState = DecideBetweenBlockedOrMultiplePolicies(testCase.ProtocolsToTest, policies);
                }
                else
                {
                    switch (policy.AssetDeliveryPolicyType)
                    {
                        case AssetDeliveryPolicyType.Blocked:
                            testCase.ExpectedState = AssetEncryptionState.BlockedByPolicy;
                            break;
                        case AssetDeliveryPolicyType.DynamicCommonEncryption:
                            testCase.ExpectedState = AssetEncryptionState.DynamicCommonEncryption;
                            break;
                        case AssetDeliveryPolicyType.DynamicEnvelopeEncryption:
                            testCase.ExpectedState = AssetEncryptionState.DynamicEnvelopeEncryption;
                            break;
                        case AssetDeliveryPolicyType.NoDynamicEncryption:
                            testCase.ExpectedState = AssetEncryptionState.NoDynamicEncryption;
                            break;
                        default:
                            throw new Exception("Unexpected policy type");
                    }
                }
            }
        }
    }
}
