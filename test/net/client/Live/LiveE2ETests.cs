//-----------------------------------------------------------------------
// <copyright file="AssetFilesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.Tests
{
    [TestClass]
    public class LiveE2ETests
    {
        private CloudMediaContext _dataContext;
        private string _testOriginName = "e2etestorigin";
        private string _testChannelName = "e2etestchannel";
        private string _testAssetlName = "e2etestasset";
        private string _testProgramlName = "e2etestprogram";

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            Cleanup();
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Cleanup();
        }

        [TestMethod]
        public void CreateStreamingTest()
        {
            IOrigin origin = _dataContext.Origins.Create(_testOriginName, 2, MakeOriginSettings());
            IChannel channel = _dataContext.Channels.Create(_testChannelName, ChannelSize.Large, MakeChannelSettings());
            IAsset asset = _dataContext.Assets.Create(_testAssetlName, AssetCreationOptions.None);
            IProgram program = channel.Programs.Create(_testProgramlName, false, TimeSpan.FromHours(1), TimeSpan.FromHours(1), asset.Id);

            Assert.AreEqual(asset.Id, program.AssetId);
            Assert.AreEqual(channel.Id, program.Channel.Id);
        }

        private IOrigin GetTestOrigin()
        {
            return _dataContext.Origins.Where(o => o.Name == _testOriginName).FirstOrDefault();
        }

        private IChannel GetTestChannel()
        {
            return _dataContext.Channels.Where(o => o.Name == _testChannelName).FirstOrDefault();
        }

        private IAsset GetTestAsset()
        {
            return _dataContext.Assets.Where(o => o.Name == _testAssetlName).FirstOrDefault();
        }

        private void Cleanup()
        {
            IOrigin testOrigin = GetTestOrigin();
            if (testOrigin != null)
            {
                testOrigin.Delete();
            }

            IAsset asset;
            IChannel channel = GetTestChannel();
            if (channel != null)
            {
                foreach (var program in channel.Programs)
                {
                    asset = _dataContext.Assets.Where(o => o.Id == program.AssetId).FirstOrDefault();
                    if (asset != null)
                    {
                        asset.Delete();
                    }
                    program.Delete();
                }

                channel.Delete();
            }

            asset = GetTestAsset();
            if (asset != null)
            {
                asset.Delete();
            }
        }

        static OriginSettings MakeOriginSettings()
        {
            var settings = new OriginSettings
            {
                Playback = new PlaybackEndpointSettings
                {
                    Security = new SecuritySettings
                    {
                        AkamaiG20Authentication = new List<G20Key> 
                        { 
                            new G20Key { Base64Key = "vUeuvDU3MIgHuFZCU3cX+24wWg6r4qho594cRcEr5fU=", Expiration = new DateTime(2030, 10, 30), Identifier = "id1" },
                        },

                        Ipv4Whitelist = new List<Ipv4>
                        {
                            new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                            new Ipv4 { Name = "testName2", IP = "1.1.1.2" },
                        }
                    }
                },
            };

            return settings;
        }

        static ChannelSettings MakeChannelSettings()
        {
            var settings = new ChannelSettings
            {
                Ingest = new IngestEndpointSettings
                {
                    Security = new SecuritySettings
                    {
                        Ipv4Whitelist = new List<Ipv4>
                        {
                            new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                        }

                    }
                },
            };

            return settings;
        }
    }
}