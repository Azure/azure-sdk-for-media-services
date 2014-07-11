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
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.Tests
{
    [TestClass]
    [Ignore] //TODO: enable when the streaming endpoint is deployed in the test environment
    public class LiveE2ETests
    {
        private CloudMediaContext _dataContext;
        private const string TestStreamingEndpointName = "e2eteststreamingendpoint-fd3a8745-3a03";
        private const string TestChannelName = "e2etestchannel-fd3a8745-3a03";
        private const string TestAssetlName = "e2etestasset-fd3a8745-3a03";
        private const string TestProgramlName = "e2etestprogram-fd3a8745-3a03";

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
            IStreamingEndpoint streamingEndpoint = _dataContext.StreamingEndpoints.Create(
                TestStreamingEndpointName,
                null, 
                null, 
                2, 
                GetAccessPolicies(), 
                GetAccessControl(), 
                GetCacheControl());

            IChannel channel = _dataContext.Channels.Create(
                TestChannelName, 
                MakeChannelInput(),
                MakeChannelPreview(),
                MakeChannelOutput());
            IAsset asset = _dataContext.Assets.Create(TestAssetlName, AssetCreationOptions.None);
            IProgram program = channel.Programs.Create(TestProgramlName, false, TimeSpan.FromHours(1), TimeSpan.FromHours(1), asset.Id);

            Assert.AreEqual(asset.Id, program.AssetId);
            Assert.AreEqual(channel.Id, program.Channel.Id);
        }

		[TestMethod]
		public void StreamingEndpointCrossDomain()
		{
			var streamingEndpoint = ObtainTestStreamingEndpoint();
		    
            var clientPolicy =
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
		    var xdomainPolicy =
		        @"<?xml version=""1.0"" ?>
			    <cross-domain-policy>
			      <allow-access-from domain=""*"" />
			    </cross-domain-policy>";

		    streamingEndpoint.CrossSiteAccessPolicies.ClientAccessPolicy = clientPolicy;
		    streamingEndpoint.CrossSiteAccessPolicies.CrossDomainPolicy = xdomainPolicy;

            streamingEndpoint.Update();

            streamingEndpoint = GetTestStreamingEndpoint();
            Assert.AreEqual(clientPolicy, streamingEndpoint.CrossSiteAccessPolicies.ClientAccessPolicy);
            Assert.AreEqual(xdomainPolicy, streamingEndpoint.CrossSiteAccessPolicies.CrossDomainPolicy);

            streamingEndpoint.Delete();
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

		    string xdomainPolicy =
		        @"<?xml version=""1.0"" ?>
			    <cross-domain-policy>
			      <allow-access-from domain=""*"" />
			    </cross-domain-policy>";

		    channel.CrossSiteAccessPolicies = new CrossSiteAccessPolicies
		    {
		        ClientAccessPolicy = clientPolicy,
		        CrossDomainPolicy = xdomainPolicy
		    };

			channel.Update();

			channel = GetTestChannel();
			Assert.AreEqual(clientPolicy, channel.CrossSiteAccessPolicies.ClientAccessPolicy);
			Assert.AreEqual(xdomainPolicy, channel.CrossSiteAccessPolicies.CrossDomainPolicy);

			channel.Delete();
		}

		[TestMethod]
		[Ignore] // need valid domain names
		public void StreamingEndpointCustomDomain()
		{
			var target = ObtainTestStreamingEndpoint();

			var domains = new[] { "a", "b" }.Select(i =>
				string.Format("{0}{1}.testingcustomdomain.com", i, new Random().Next(1000, 9999).ToString()))
				.ToList();

			target.CustomHostNames = domains;

			target.Update();

			target = GetTestStreamingEndpoint();
		    Assert.IsTrue(domains.SequenceEqual(target.CustomHostNames));

			target.Delete();
		}

        private IStreamingEndpoint ObtainTestStreamingEndpoint()
        {
            var result = _dataContext.StreamingEndpoints.Where(o => o.Name == TestStreamingEndpointName).FirstOrDefault();
			if(result == null)
			{
				result = _dataContext.StreamingEndpoints.Create(
                    TestStreamingEndpointName, 
                    null, 
                    null, 
                    2, 
                    GetAccessPolicies(), 
                    GetAccessControl(), 
                    GetCacheControl());
			}

			return result;
        }

		private IStreamingEndpoint GetTestStreamingEndpoint()
		{
			return _dataContext.StreamingEndpoints.Where(o => o.Name == TestStreamingEndpointName).FirstOrDefault();
		}

        private IChannel GetTestChannel()
        {
            return _dataContext.Channels.Where(o => o.Name == TestChannelName).FirstOrDefault();
        }

		private IChannel ObtainTestChannel()
		{
			var result = _dataContext.Channels.Where(o => o.Name == TestChannelName).FirstOrDefault();
			if(result == null)
			{
				result = _dataContext.Channels.Create(
                    TestChannelName, 
                    MakeChannelInput(),
                    MakeChannelPreview(),
                    MakeChannelOutput());
			}
			return result;
		}

        private IAsset GetTestAsset()
        {
            return _dataContext.Assets.Where(o => o.Name == TestAssetlName).FirstOrDefault();
        }

        private void Cleanup()
        {
            IStreamingEndpoint testStreamingEndpoint = GetTestStreamingEndpoint();
            if (testStreamingEndpoint != null)
            {
                testStreamingEndpoint.Delete();
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

        private static CrossSiteAccessPolicies GetAccessPolicies()
        {
            return new CrossSiteAccessPolicies
            {
                CrossDomainPolicy = File.ReadAllText(@".\crossdomain.xml"),
                ClientAccessPolicy = File.ReadAllText(@".\clientaccesspolicy.xml")
            };
        }

        private static StreamingEndpointCacheControl GetCacheControl()
        {
            return new StreamingEndpointCacheControl
            {
                MaxAge = TimeSpan.FromMinutes(30)
            };
        }

        private static StreamingEndpointAccessControl GetAccessControl()
        {
            return new StreamingEndpointAccessControl
            {
                IPAllowList = new List<IPAddress>
                {
                    new IPAddress
                    {
                        Name = "IP List 1",
                        Address = System.Net.IPAddress.Parse("131.107.0.0"),
                        SubnetPrefixLength = 16
                    },
                    new IPAddress
                    {
                        Name = "IP List 2",
                        Address = System.Net.IPAddress.Parse("131.107.192.0"),
                        SubnetPrefixLength = 24
                    }
                },

                AkamaiSignatureHeaderAuthenticationKeyList = new List<AkamaiSignatureHeaderAuthenticationKey>
                {
                    new AkamaiSignatureHeaderAuthenticationKey
                    {
                        Identifier = "a1",
                        Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                        Base64Key = Convert.ToBase64String(GenerateRandomBytes(16))
                    },
                    new AkamaiSignatureHeaderAuthenticationKey
                    {
                        Identifier = "a2",
                        Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                        Base64Key = Convert.ToBase64String(GenerateRandomBytes(16))
                    }
                }
            };
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        static IChannelInput MakeChannelInput()
        {
            IChannelInput input = new ChannelInput();

            input.KeyFrameDistanceHns = 19000000;
            input.StreamingProtocol = StreamingProtocol.Smooth;
            input.AccessControl = new ChannelAccessControl
            {
                IPAllowList = new List<IPAddress>
                {
                    new IPAddress
                    {
                        Name = "testName1",
                        Address = System.Net.IPAddress.Parse("1.1.1.1"),
                        SubnetPrefixLength = 24
                    }
                }
            };

            return input;
        }

        static IChannelPreview MakeChannelPreview()
        {
            IChannelPreview preview = new ChannelPreview();

            preview.AccessControl = new ChannelAccessControl
            {
                IPAllowList = new List<IPAddress>
                {
                    new IPAddress
                    {
                        Name = "testName1",
                        Address = System.Net.IPAddress.Parse("1.1.1.1"),
                        SubnetPrefixLength = 24
                    }
                }
            };

            return preview;
        }

        static IChannelOutput MakeChannelOutput()
        {
            IChannelOutput output = new ChannelOutput();

            output.Hls = new ChannelOutputHls { FragmentsPerSegment = 1 };

            return output;
        }
    }
}