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
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.Tests
{
    [TestClass]
    public class LiveE2ETests
    {
        private CloudMediaContext _dataContext;
        private string _testOriginName = "e2etestorigin-fd3a8745-3a03";
        private string _testChannelName = "e2etestchannel-fd3a8745-3a03";
        private string _testAssetlName = "e2etestasset-fd3a8745-3a03";
        private string _testProgramlName = "e2etestprogram-fd3a8745-3a03";

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

        /// <summary>
        /// Creates everything needed for streaming.
        /// </summary>
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

		[TestMethod]
		public void OriginCrossDomain()
		{
			IOrigin origin = ObtainTestOrigin();

			string clientPolicy = 
				@"<?xml version=""1.0"" encoding=""utf-8""?>
				<access-policy>
				  <cross-domain-access>
					<policy>
					  <allow-from http-request-headers=""*"" http-methods=""*"">
						<domain uri=""*""/>
					  </allow-from>
					  <grant-to>
						<resource path=""/"" include-subpaths=""true""/>
					  </grant-to>
					</policy>
				  </cross-domain-access>
				</access-policy>";

			origin.Settings.ClientAccessPolicy = new CrossSiteAccessPolicy
			{
				Version = "1.0",
				Policy = clientPolicy					
			};

			string xdomainPolicy =
			@"<?xml version=""1.0"" ?>
			<cross-domain-policy>
			  <allow-access-from domain=""*"" />
			</cross-domain-policy>";

			origin.Settings.CrossDomainPolicy = new CrossSiteAccessPolicy
			{
				Version = "1.0",
				Policy = xdomainPolicy
			};

			origin.Update();

			origin = GetTestOrigin();
			Assert.AreEqual(clientPolicy, origin.Settings.ClientAccessPolicy.Policy);
			Assert.AreEqual(xdomainPolicy, origin.Settings.CrossDomainPolicy.Policy);

			origin.Delete();
		}

		[TestMethod]
		public void ChannelCrossDomain()
		{
			var channel = ObtainTestChannel();

			string clientPolicy =
				@"<?xml version=""1.0"" encoding=""utf-8""?>
				<access-policy>
				  <cross-domain-access>
					<policy>
					  <allow-from http-request-headers=""*"" http-methods=""*"">
						<domain uri=""*""/>
					  </allow-from>
					  <grant-to>
						<resource path=""/"" include-subpaths=""true""/>
					  </grant-to>
					</policy>
				  </cross-domain-access>
				</access-policy>";

			channel.Settings.ClientAccessPolicy = new CrossSiteAccessPolicy
			{
				Version = "1.0",
				Policy = clientPolicy
			};

			string xdomainPolicy =
			@"<?xml version=""1.0"" ?>
			<cross-domain-policy>
			  <allow-access-from domain=""*"" />
			</cross-domain-policy>";

			channel.Settings.CrossDomainPolicy = new CrossSiteAccessPolicy
			{
				Version = "1.0",
				Policy = xdomainPolicy
			};

			channel.Update();

			channel = GetTestChannel();
			Assert.AreEqual(clientPolicy, channel.Settings.ClientAccessPolicy.Policy);
			Assert.AreEqual(xdomainPolicy, channel.Settings.CrossDomainPolicy.Policy);

			channel.Delete();
		}

		[TestMethod]
		[Ignore] // need valid domain names
		public void OriginCustomDomain()
		{
			var target = ObtainTestOrigin();

			var domains = new[] { "a", "b" }.Select(i =>
				string.Format("{0}{1}.testingcustomdomain.com", i, new Random().Next(1000, 9999).ToString()))
				.ToArray();

			target.Settings.CustomDomain = new CustomDomainSettings
			{
				CustomDomainNames = domains
			};

			target.Update();

			target = GetTestOrigin();
			Assert.IsTrue(domains.SequenceEqual(target.Settings.CustomDomain.CustomDomainNames));

			target.Delete();
		}

		[TestMethod]
		[Ignore] // need valid domain names
		public void ChannelCustomDomain()
		{
			var target = ObtainTestChannel();

			var domains = new[] { "a", "b" }.Select(i =>
				string.Format("{0}{1}.testingcustomdomain.com", i, new Random().Next(1000, 9999).ToString()))
				.ToArray();

			target.Settings.CustomDomain = new CustomDomainSettings
			{
				CustomDomainNames = domains
			};

			target.Update();

			target = GetTestChannel();
			Assert.IsTrue(domains.SequenceEqual(target.Settings.CustomDomain.CustomDomainNames));

			target.Delete();
		}

        private IOrigin ObtainTestOrigin()
        {
            var result = _dataContext.Origins.Where(o => o.Name == _testOriginName).FirstOrDefault();
			if(result == null)
			{
				result = _dataContext.Origins.Create(_testOriginName, 2, MakeOriginSettings());
			}

			return result;
        }

		private IOrigin GetTestOrigin()
		{
			return _dataContext.Origins.Where(o => o.Name == _testOriginName).FirstOrDefault();
		}

        private IChannel GetTestChannel()
        {
            return _dataContext.Channels.Where(o => o.Name == _testChannelName).FirstOrDefault();
        }

		private IChannel ObtainTestChannel()
		{
			var result = _dataContext.Channels.Where(o => o.Name == _testChannelName).FirstOrDefault();
			if(result == null)
			{
				result = _dataContext.Channels.Create(_testChannelName, ChannelSize.Large, MakeChannelSettings());
			}
			return result;
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
					program.Delete();
					if (asset != null)
                    {
                        asset.Delete();
                    }
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
                    Security = new PlaybackEndpointSecuritySettings
                    {
                        AkamaiSignatureHeaderAuthentication = new List<AkamaiSignatureHeaderAuthenticationKey> 
                        { 
                            new AkamaiSignatureHeaderAuthenticationKey { Base64Key = "vUeuvDU3MIgHuFZCU3cX+24wWg6r4qho594cRcEr5fU=", Expiration = new DateTime(2030, 10, 30), Identifier = "id1" },
                        },

                        IPv4AllowList = new List<Ipv4>
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
                    Security = new IngestEndpointSecuritySettings
                    {
                        IPv4AllowList = new List<Ipv4>
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