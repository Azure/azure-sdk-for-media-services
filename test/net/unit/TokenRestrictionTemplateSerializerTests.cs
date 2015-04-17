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
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class TokenRestrictionTemplateSerializerTests
    {
        public static readonly string _sampleIssuer = "http://sampleIssuerUrl";
        public static readonly string _sampleAudience = "http://sampleAudience";

        [TestMethod]
        public void RoundTripTest()
        {
            TokenRestrictionTemplate template = new TokenRestrictionTemplate(TokenType.SWT);

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
            Assert.AreEqual(template.TokenType, TokenType.SWT);

            SymmetricVerificationKey fromTemplate = (SymmetricVerificationKey) template.PrimaryVerificationKey;
            SymmetricVerificationKey fromTemplate2 = (SymmetricVerificationKey) template2.PrimaryVerificationKey;

            Assert.IsTrue(fromTemplate.KeyValue.SequenceEqual(fromTemplate2.KeyValue));
        }

        [TestMethod]
        public void KnownGoodInputForSwtOnlyScheme()
        {
            string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys><TokenVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>GG07fDPZ+HMD2vcoknMqYjEJMb7LSq8zUmdCYMvRCevnQK//ilbhODO/FydMrHiwZGmI6XywvOOU7SSzRPlI3Q==</KeyValue></TokenVerificationKey></AlternateVerificationKeys><Audience>http://sampleaudience/</Audience><Issuer>http://sampleissuerurl/</Issuer><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims></TokenRestrictionTemplate>";

            TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
            Assert.IsNotNull(template);
            Assert.AreEqual(TokenType.SWT,template.TokenType);
        }

         [TestMethod]
        public void KnownGoodInputForJWT()
        {
          string tokenTemplate="<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys /><Audience>http://sampleissuerurl/</Audience><Issuer>http://sampleaudience/</Issuer><PrimaryVerificationKey i:type=\"X509CertTokenVerificationKey\"><RawBody>MIIDAzCCAeugAwIBAgIQ2cl0q8oGkaFG+ZTZYsilhDANBgkqhkiG9w0BAQ0FADARMQ8wDQYDVQQDEwZDQVJvb3QwHhcNMTQxMjAxMTg0NzI5WhcNMzkxMjMxMjM1OTU5WjARMQ8wDQYDVQQDEwZDQVJvb3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDjgMbtZcLtKNdJXHSGQ7l6xJBtNCVhjF4+BLZq+D2RmubKTAnGXhNGY4FO2LrPjfkWumdnv5DOlFuwHy2qrsZu1TFZxxQzU9/Yp3VAD1Afk7ShUOxniPpIO9vfkUH+FEX1Taq4ncR/TkiwnIZLy+bBa0DlF2MsPGC62KbiN4xJqvSIuecxQvcN8MZ78NDejtj1/XHF7VBmVjWi5B79GpTvY9ap39BU8nM0Q8vWb9DwmpWLz8j7hm25f+8laHIE6U8CpeeD/OrZT8ncCD0hbhR3ZGGoFqJbyv2CLPVGeaIhIxBH41zgrBYR53NjkRLTB4IEUCgeTGvSzweqlb+4totdAgMBAAGjVzBVMA8GA1UdEwEB/wQFMAMBAf8wQgYDVR0BBDswOYAQSHiCUWtQlUe79thqsTDbbqETMBExDzANBgNVBAMTBkNBUm9vdIIQ2cl0q8oGkaFG+ZTZYsilhDANBgkqhkiG9w0BAQ0FAAOCAQEABa/2D+Rxo6tp63sDFRViikNkDa5GFZscQLn4Rm35NmUt35Wc/AugLaTJ7iP5zJTYIBUI9DDhHbgFqmYpW0p14NebJlBzrRFIaoHBOsHhy4VYrxIB8Q/OvSGPgbI2c39ni/odyTYKVtJacxPrIt+MqeiFMjJ19cJSOkKT2AFoPMa/L0++znMcEObSAHYMy1U51J1njpQvNJ+MQiR8y2gvmMbGEcMgicIJxbLB2imqJWCQkFUlsrxwuuzSvNaLkdd/HyhsR1JXc+kOREO8gWjhT6MAdgGKC9+neamR7sqwJHPNfcLYTDFOhi6cJH10z74mU1Xa5uLsX+aZp2YYHUFw4Q==</RawBody></PrimaryVerificationKey><RequiredClaims /><TokenType>JWT</TokenType></TokenRestrictionTemplate>";
          TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
          Assert.IsNotNull(template);
          Assert.AreEqual(TokenType.JWT, template.TokenType);        
        }
         
        [TestMethod]
         public void KnownGoodInputForSWT()
         {
             string tokenTemplate = "<TokenRestrictionTemplate xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1\"><AlternateVerificationKeys /><Audience>http://sampleissuerurl/</Audience><Issuer>http://sampleaudience/</Issuer><PrimaryVerificationKey i:type=\"SymmetricVerificationKey\"><KeyValue>2OvxltHKwILn5PCRD8H+63sK98LBs1yF+ZdZbwzmToWYm29pLyqIMuCvMRGpLOv5DYh3NmpzWMAciu4ncW8VTg==</KeyValue></PrimaryVerificationKey><RequiredClaims><TokenClaim><ClaimType>urn:microsoft:azure:mediaservices:contentkeyidentifier</ClaimType><ClaimValue i:nil=\"true\" /></TokenClaim><TokenClaim><ClaimType>urn:myservice:claims:rental</ClaimType><ClaimValue>true</ClaimValue></TokenClaim></RequiredClaims><TokenType>SWT</TokenType></TokenRestrictionTemplate>";
             TokenRestrictionTemplate template = TokenRestrictionTemplateSerializer.Deserialize(tokenTemplate);
             Assert.IsNotNull(template);
             Assert.AreEqual(TokenType.SWT, template.TokenType);
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
            Assert.AreEqual(TokenType.SWT, template.TokenType);
        }
    }
}
