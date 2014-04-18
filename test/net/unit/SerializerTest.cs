//-----------------------------------------------------------------------
// <copyright file="SerializerTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.MediaServices.Client;
using InternalRest = Microsoft.WindowsAzure.MediaServices.Client.Rest;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{    
    /// <summary>
    ///This is a test class for OriginDataTest and is intended
    ///to contain all OriginDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SerializerTest
    {
        /// <summary>
        ///A test for DeserializeSettings
        ///</summary>
        [TestMethod()]
        public void DeserializeSettingsTestEmpty()
        {
            var actual = Serializer.Deserialize<OriginSettings>(string.Empty);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for DeserializeSettings
        ///</summary>
        [TestMethod()]
        public void SerializeSettingsTestEmpty()
        {
            string actual = Serializer.Serialize<OriginSettings>(null);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for SerializeSettings
        ///</summary>
        [TestMethod()]
        public void SerializeSettingsTestSimple()
        {
            var settings = new OriginSettings
            {
                Playback = new PlaybackEndpointSettings
                {
                    MaxCacheAge = TimeSpan.FromMinutes(1),
                    Security = new PlaybackEndpointSecuritySettings
                    {
                        AkamaiSignatureHeaderAuthentication = new List<AkamaiSignatureHeaderAuthenticationKey> 
                        { 
                            new AkamaiSignatureHeaderAuthenticationKey { Base64Key = "b64Key1", Expiration = new DateTime(2013, 1, 30, 8, 0, 0, DateTimeKind.Utc), Identifier = "id1" },
                            new AkamaiSignatureHeaderAuthenticationKey { Base64Key = "b64Key2", Expiration = new DateTime(2013, 1, 30, 8, 0, 0, DateTimeKind.Utc), Identifier = "id2" },
                        },

                        IPv4AllowList = new List<Ipv4>
                        {
                            new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                            new Ipv4 { Name = "testName2", IP = "1.1.1.2" },
                        }
                    }
                },

				ClientAccessPolicy = new CrossSiteAccessPolicy { Policy = "test", Version = "1.0" },
				CrossDomainPolicy = new CrossSiteAccessPolicy { Policy = "test2", Version = "2.0" },
				CustomDomain = new CustomDomainSettings { CustomDomainNames = new[] { "name1", "name2" } }
			};

            var serialized = Serializer.Serialize<InternalRest.OriginServiceSettings>(new InternalRest.OriginServiceSettings(settings));

            string expected = @"{
                    ""Playback"":
                    {
	                    ""MaxCacheAge"":60,
	                    ""Security"":
	                    {
		                    ""IPv4AllowList"": [{""Name"":""testName1"",""IP"":""1.1.1.1""},{""Name"":""testName2"",""IP"":""1.1.1.2""}],	
		                    ""AkamaiSignatureHeaderAuthentication"":
		                    [
			                    {""Expiration"":""\/Date(1359532800000)\/"",""Identifier"":""id1"",""Base64Key"":""b64Key1""},
			                    {""Expiration"":""\/Date(1359532800000)\/"",""Identifier"":""id2"",""Base64Key"":""b64Key2""}
		                    ]
	                    }
                    },
					""ClientAccessPolicy"":{""Policy"":""test"",""Version"":""1.0""},
					""CrossDomainPolicy"":{""Policy"":""test2"",""Version"":""2.0""},
					""CustomDomain"":{""CustomDomainNames"":[""name1"",""name2""]}
                }";

            bool ok = expected.Where(c => !char.IsWhiteSpace(c)).SequenceEqual(serialized);
            Assert.IsTrue(ok);
        }

        /// <summary>
        ///A test for SerializeSettings
        ///</summary>
        [TestMethod()]
        public void DeserializeSettingsTestSimple()
        {
            string serialized = @"{
                    ""Playback"":
                    {
	                    ""MaxCacheAge"":10,
	                    ""Security"":
	                    {
		                    ""IPv4AllowList"": [{""Name"":""testName1"",""IP"":""1.1.1.1""},{""Name"":""testName2"",""IP"":""1.1.1.2""}],	
		                    ""AkamaiSignatureHeaderAuthentication"":
		                    [
			                    {""Expiration"":""\/Date(1359532800000)\/"",""Identifier"":""id1"",""Base64Key"":""b64Key1""},
			                    {""Expiration"":""\/Date(1359532800000)\/"",""Identifier"":""id1"",""Base64Key"":""b64Key1""},
			                    {""Expiration"":""\/Date(1359532800000)\/"",""Identifier"":""id2"",""Base64Key"":""b64Key2""}
		                    ]
	                    }
                    },
					""ClientAccessPolicy"":{""Policy"":""test"",""Version"":""1.0""},
					""CrossDomainPolicy"":{""Policy"":""test2"",""Version"":""2.0""},
					""CustomDomain"":{""CustomDomainNames"":[""name1"",""name2""]}
                }";

            var deserialized = (OriginSettings)Serializer.Deserialize<InternalRest.OriginServiceSettings>(serialized);
            
            Assert.AreEqual(2, deserialized.Playback.Security.IPv4AllowList.Count);
            Assert.AreEqual(3, deserialized.Playback.Security.AkamaiSignatureHeaderAuthentication.Count);
            Assert.AreEqual(new DateTime(2013, 1, 30, 8, 0, 0, DateTimeKind.Utc), deserialized.Playback.Security.AkamaiSignatureHeaderAuthentication[2].Expiration);
            Assert.AreEqual("1.1.1.2", deserialized.Playback.Security.IPv4AllowList[1].IP);
            Assert.AreEqual("testName2", deserialized.Playback.Security.IPv4AllowList[1].Name);
            Assert.AreEqual(10, deserialized.Playback.MaxCacheAge.Value.TotalSeconds);
			Assert.AreEqual("test", deserialized.ClientAccessPolicy.Policy);
			Assert.AreEqual("test2", deserialized.CrossDomainPolicy.Policy);
			Assert.AreEqual("name1", deserialized.CustomDomain.CustomDomainNames[0]);
			Assert.AreEqual("name2", deserialized.CustomDomain.CustomDomainNames[1]);
		}
    }
}
