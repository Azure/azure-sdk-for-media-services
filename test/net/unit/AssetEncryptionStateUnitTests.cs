//-----------------------------------------------------------------------
// <copyright file="AssetEncryptionStatusUnitTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class AssetEncryptionStateUnitTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }
        private const string c_TestCaseDataFile = "TestData\\AssetEncryptionStateTestCases.csv";

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        private AssetFileData GetAssetFile(string name, bool isPrimary)
        {
            AssetFileData returnValue = new AssetFileData();

            returnValue.Name = name;
            returnValue.IsPrimary = isPrimary;

            return returnValue;
        }

        private void AddTestAssetFiles(IAsset asset, AssetType assetType)
        {
            IAssetFile assetFile = null;

            switch (assetType)
            {
                case AssetType.MP4:
                    assetFile = asset.AssetFiles.Create("test.mp4");
                    assetFile.IsPrimary = true;
                    assetFile.Update();

                    break;
                case AssetType.MediaServicesHLS:
                    assetFile = asset.AssetFiles.Create("test-m3u8-aapl.ism");

                    assetFile.IsPrimary = true;
                    assetFile.Update();

                    asset.AssetFiles.Create("test-m3u8-aapl.m3u8");
                    asset.AssetFiles.Create("test-m3u8-aapl-952962.ts");
                    asset.AssetFiles.Create("test-m3u8-aapl-952962.m3u8");
                    asset.AssetFiles.Create("test-m3u8-aapl-952962.ismx");
                    break;
                case AssetType.MultiBitrateMP4:
                    assetFile = asset.AssetFiles.Create("test.ism");
                    assetFile.IsPrimary = true;
                    assetFile.Update();

                    asset.AssetFiles.Create("test.mp4");
                    break;
                case AssetType.SmoothStreaming:
                    assetFile = asset.AssetFiles.Create("test.ism");
                    assetFile.IsPrimary = true;
                    assetFile.Update();

                    asset.AssetFiles.Create("test.ismc");
                    asset.AssetFiles.Create("test.ismv");
                    break;
                case AssetType.Unknown:
                    assetFile = asset.AssetFiles.Create("test.wmv");
                    assetFile.IsPrimary = true;
                    assetFile.Update();

                    break;
            }
        }

        private void AddTestDeliveryPolicies(IAsset asset, AssetDeliveryProtocol protocol, AssetDeliveryPolicyType deliveryType)
        {
            if (deliveryType != AssetDeliveryPolicyType.None)
            {
                IAssetDeliveryPolicy policy = _mediaContext.AssetDeliveryPolicies.Create("Test Asset Delivery Policy", deliveryType, protocol, null);
                asset.DeliveryPolicies.Add(policy);
            }
        }

        private IAsset GetTestAsset(AssetCreationOptions options, AssetType assetType, AssetDeliveryProtocol protocol, AssetDeliveryPolicyType deliveryType)
        {
            IAsset asset = _mediaContext.Assets.Create("Test", options);

            AddTestAssetFiles(asset, assetType);

            AddTestDeliveryPolicies(asset, protocol, deliveryType);

            return asset;
        }

        [TestMethod]
        [DeploymentItem(@"TestData\AssetEncryptionStateTestCases.csv", "TestData")]
        [DeploymentItem(@"UnitTest.pfx")]
        public void RunAllGetEffectiveDeliveryPolicyTestCases()
        {
            string testCaseDataFilePath = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, c_TestCaseDataFile);
            string[] testCases = File.ReadAllLines(testCaseDataFilePath);
            Assert.IsNotNull(testCases);
            Assert.AreEqual(401, testCases.Length); // ensure we have the expected number of cases

            int failureCount = 0;

            StringBuilder builder = new StringBuilder();

            builder.Append(testCases[0]);
            builder.Append(",ActualAssetType,ActualIsStreamable,ActualEffectiveEncryptionState");
            builder.AppendLine();

            for (int i = 1; i < testCases.Length; i++)
            {
                string[] parameters = testCases[i].Split(',');
                AssetCreationOptions options = (AssetCreationOptions)Enum.Parse(typeof(AssetCreationOptions), parameters[0]);
                AssetType assetType = (AssetType)Enum.Parse(typeof(AssetType), parameters[1]);
                AssetDeliveryProtocol assetDeliveryProtocol = (AssetDeliveryProtocol)Enum.Parse(typeof(AssetDeliveryProtocol), parameters[2]);
                AssetDeliveryPolicyType assetDeliveryPolicyType = (AssetDeliveryPolicyType)Enum.Parse(typeof(AssetDeliveryPolicyType), parameters[3]);
                AssetEncryptionState expectedEncryptionState = (AssetEncryptionState)Enum.Parse(typeof(AssetEncryptionState), parameters[4]);
                bool expectedIsStreamable = bool.Parse(parameters[5]);
                AssetType expectedAssetType = (AssetType)Enum.Parse(typeof(AssetType), parameters[6]);

                IAsset asset = GetTestAsset(options, assetType, assetDeliveryProtocol, assetDeliveryPolicyType);

                AssetEncryptionState actualEncryptionState = asset.GetEncryptionState(assetDeliveryProtocol);

                if (false == ((expectedAssetType == asset.AssetType) &&
                              (expectedIsStreamable == asset.IsStreamable) &&
                              (expectedEncryptionState == actualEncryptionState)
                              )
                    )
                {
                    // We had a failure so increase our failed count and then save the details of the test case and where it failed
                    failureCount++;

                    builder.Append(testCases[i]);
                    builder.Append(",");
                    builder.Append(asset.AssetType.ToString());
                    builder.Append(",");
                    builder.Append(asset.IsStreamable.ToString());
                    builder.Append(",");
                    builder.Append(actualEncryptionState.ToString());
                    builder.AppendLine();
                }
            }

            if (failureCount > 0)
            {
                Assert.Fail("Some RunAllGetEffectiveDeliveryPolicyTestCases failed");

                // If there are a lot of failures the best way to debug then is to dump
                // failed test case input and output data to a csv file for more detailed
                // analysis
                //File.WriteAllText("output.csv", builder.ToString());
            }
        }

        //
        //  Function to create the input data part of the TestCaseDataFile.
        //  Note that the ExpectedEffectiveDeliveryPolicy, ExpectedIsStreamable, ExpectedAssetType were added manually
        //
        private void GenerateTestCaseInputParameters()
        {
            StringBuilder builder = new StringBuilder();

            // Add the header
            builder.AppendLine("AssetCreationOptions,AssetType,AssetDeliveryProtocol,AssetDeliveryPolicyType");

            foreach (AssetType at in Enum.GetValues(typeof(AssetType)))
            {
                foreach (AssetCreationOptions o in Enum.GetValues(typeof(AssetCreationOptions)))
                {
                    foreach (AssetDeliveryProtocol adp in Enum.GetValues(typeof(AssetDeliveryProtocol)))
                    {
                        if ((adp == AssetDeliveryProtocol.All) || (adp == AssetDeliveryProtocol.None))
                        {
                            continue;
                        }

                        foreach (AssetDeliveryPolicyType adpt in Enum.GetValues(typeof(AssetDeliveryPolicyType)))
                        {
                            builder.Append(o.ToString());
                            builder.Append(",");
                            builder.Append(at.ToString());
                            builder.Append(",");
                            builder.Append(adp.ToString());
                            builder.Append(",");
                            builder.Append(adpt.ToString());
                            builder.AppendLine();
                        }
                    }
                }
            }

            File.WriteAllText(c_TestCaseDataFile, builder.ToString());
        }
    }
}