//-----------------------------------------------------------------------
// <copyright file="TokenRestrictionTemplateSerializerTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class TokenRestrictionTemplateSerializerTests
    {
        public static readonly Uri _sampleIssuer = new Uri("http://sampleIssuerUrl");
        public static readonly Uri _sampleAudience = new Uri("http://sampleAudience");

        [TestMethod]
        public void RoundTripTest()
        {
            TokenRestrictionTemplate template = new TokenRestrictionTemplate();

            template.PrimaryVerificationKey = new SymmetricVerificationKey();
            template.AlternateVerificationKeys.Add(new SymmetricVerificationKey());
            template.Audience = _sampleAudience;
            template.Issuer = _sampleIssuer;
            template.RequiredClaims.Add(TokenClaim.ContentKeyIdentifierClaim);
            template.RequiredClaims.Add(new TokenClaim("Rental", "true"));

            string serializedTemplate = TokenRestrictionTemplateSerializer.Serialize(template);
            Assert.IsFalse(String.IsNullOrWhiteSpace(serializedTemplate));

            TokenRestrictionTemplate template2 = TokenRestrictionTemplateSerializer.Deserialize(serializedTemplate);
            Assert.IsNotNull(template2);
            Assert.AreEqual(template.Issuer, template2.Issuer);
            Assert.AreEqual(template.Audience, template2.Audience);

            SymmetricVerificationKey fromTemplate = (SymmetricVerificationKey) template.PrimaryVerificationKey;
            SymmetricVerificationKey fromTemplate2 = (SymmetricVerificationKey) template2.PrimaryVerificationKey;

            Assert.IsTrue(fromTemplate.KeyValue.SequenceEqual(fromTemplate2.KeyValue));
        }

        [TestMethod]
        public void KnownGoodInputTest()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Audience>http://sampleaudience/</Audience><Issuer>http://sampleissuerurl/</Issuer><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims></TokenRestrictionTemplate>";

            TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
            Assert.IsNotNull(template);
        }

        [TestMethod]
        public void InputMissingIssuerShouldThrow()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Audience>http://sampleaudience/</Audience><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims></TokenRestrictionTemplate>";

            try
            {
                TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
                Assert.Fail("Should throw");
            }
            catch (SerializationException e)
            {
                Assert.IsTrue(e.Message.Contains("Expecting element 'Issuer'."));
            }
        }

        [TestMethod]
        public void InputMissingAudienceShouldThrow()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Issuer>http://sampleissuerurl/</Issuer><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims></TokenRestrictionTemplate>";

            try
            {
                TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
                Assert.Fail("Should throw");
            }
            catch (SerializationException e)
            {
                Assert.IsTrue(e.Message.Contains("Expecting element 'Audience'."));
            }
        }

        [TestMethod]
        public void InputMissingPrimaryKeyShouldThrow()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Audience>http://sampleaudience/</Audience><Issuer>http://sampleissuerurl/</Issuer><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims></TokenRestrictionTemplate>";

            try
            {
                TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
                Assert.Fail("Should throw");
            }
            catch (SerializationException e)
            {
                Assert.IsTrue(e.Message.Contains("Expecting element 'PrimaryVerificationKey'."));
            }
        }

        [TestMethod]
        public void InputMissingRequiredClaimsOkay()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Audience>http://sampleaudience/</Audience><Issuer>http://sampleissuerurl/</Issuer><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey></TokenRestrictionTemplate>";

            TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
            Assert.IsNotNull(template);
        }
    }
}
