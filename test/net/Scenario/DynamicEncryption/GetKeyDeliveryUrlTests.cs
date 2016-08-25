//-----------------------------------------------------------------------
// <copyright file="GetKeyDeliveryUrlTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Configuration;
using System.Data.Services.Client;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.DynamicEncryption;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using Microsoft.WindowsAzure.MediaServices.Client.Widevine;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class GetKeyDeliveryUrlTests
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GetPlayReadyLicenseDeliveryUrl()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.CommonEncryption);

                PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
                responseTemplate.LicenseTemplates.Add(new PlayReadyLicenseTemplate());
                string licenseTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.PlayReadyLicense, null, licenseTemplate, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.PlayReadyLicense);

                Assert.IsNotNull(keyDeliveryServiceUri);
                Assert.IsTrue(0 == String.Compare(keyDeliveryServiceUri.AbsolutePath, "/PlayReady/", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GetHlsKeyDeliveryUrlAndFetchKey()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption, out expectedKey);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);

                Assert.IsNotNull(keyDeliveryServiceUri);

                // Enable once all accounts are enabled for per customer Key Delivery Urls
                //Assert.IsTrue(keyDeliveryServiceUri.Host.StartsWith(_mediaContext.Credentials.ClientId));

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                string rawkey = EncryptionUtils.GetKeyIdAsGuid(contentKey.Id).ToString();
                byte[] key = keyClient.AcquireHlsKeyWithBearerHeader(keyDeliveryServiceUri, TokenServiceClient.GetAuthTokenForKey(rawkey));

                string expectedString = GetString(expectedKey);
                string fetchedString = GetString(key);
                Assert.AreEqual(expectedString, fetchedString);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GetWidevineKeyDeliveryUrlAndFetchLicense()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.CommonEncryption, out expectedKey);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.Widevine, null, "{}", ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.Widevine);

                Assert.IsNotNull(keyDeliveryServiceUri);

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                string rawkey = EncryptionUtils.GetKeyIdAsGuid(contentKey.Id).ToString();

                string payload = "CAEShAEKTAgAEkgAAAACAAAQWPXbhtb/q43f3SfuC2VP3q0jeAECW3emQkWn2wXCYVOnvlWPDNqh8VVIB4GmsNA8eVVFigXkQWIGN0GlgMKjpUESLAoqChQIARIQJMPCzl2bViyMQEtyK/gtmRABGhAyNWY3ODMzMTcyMmJjM2EyGAEgv5iQkAUaIC3ON1zVgeV0rP7w2VmVLGorqClcMQO4BdbHPyk3GsnY";

                byte[] license = keyClient.AcquireWidevineLicenseWithBearerHeader(
                    keyDeliveryServiceUri, 
                    TokenServiceClient.GetAuthTokenForKey(rawkey),
                    Convert.FromBase64String(payload));

                string expectedString = Convert.ToBase64String(license);
                Assert.AreEqual("CAIS", expectedString.Substring(0, 4));
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GetWidevineKeyDeliveryUrlAndFetchLicenseWithPolicy()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.CommonEncryption, out expectedKey);

                var template = new WidevineMessage
                {
                    allowed_track_types = AllowedTrackTypes.SD_HD,
                    content_key_specs = new[]
                    {
                        new ContentKeySpecs 
                        { 
                            key_id = contentKey.Id, 
                            required_output_protection = new RequiredOutputProtection { hdcp = Hdcp.HDCP_NONE}, 
                            security_level = 1, 
                            track_type = "SD"
                        }
                    },
                    policy_overrides = new 
                    {
                        can_play = true,
                        can_persist = true,
                        can_renew = true,
                        license_duration_seconds = 10,
                        renewal_delay_seconds = 3,
                    }
                };

                string configuration = JsonConvert.SerializeObject(template);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(
                    _mediaContext, 
                    String.Empty, 
                    ContentKeyDeliveryType.Widevine, 
                    null, 
                    configuration, 
                    ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.Widevine);

                Assert.IsNotNull(keyDeliveryServiceUri);

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                string rawkey = EncryptionUtils.GetKeyIdAsGuid(contentKey.Id).ToString();

                string payload = "CAEShAEKTAgAEkgAAAACAAAQWPXbhtb/q43f3SfuC2VP3q0jeAECW3emQkWn2wXCYVOnvlWPDNqh8VVIB4GmsNA8eVVFigXkQWIGN0GlgMKjpUESLAoqChQIARIQJMPCzl2bViyMQEtyK/gtmRABGhAyNWY3ODMzMTcyMmJjM2EyGAEgv5iQkAUaIC3ON1zVgeV0rP7w2VmVLGorqClcMQO4BdbHPyk3GsnY";

                byte[] license = keyClient.AcquireWidevineLicenseWithBearerHeader(
                    keyDeliveryServiceUri,
                    TokenServiceClient.GetAuthTokenForKey(rawkey),
                    Convert.FromBase64String(payload));

                string expectedString = Convert.ToBase64String(license);
                Assert.AreEqual("CAIS", expectedString.Substring(0, 4));
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        [DeploymentItem("amscer.pfx") ]
        public void GetHlsKeyDeliveryUrlAndFetchKeyWithJWTAuthenticationWhenIssuerIsURI()
        {

            string audience = "http://sampleAudience";
            string issuer = "http://sampleIssuerUrl";
            FetchKeyWithJWTAuth(audience, issuer);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        [DeploymentItem("amscer.pfx")]
        public void GetHlsKeyDeliveryUrlAndFetchKeyWithJWTAuthenticationWhenIssuerIsStringGuid()
        {

            string audience = Guid.NewGuid().ToString();
            string issuer = Guid.NewGuid().ToString();
            FetchKeyWithJWTAuth(audience, issuer);
        }

        private void FetchKeyWithJWTAuth(string audience, string issuer)
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption, out expectedKey);

                var templatex509Certificate2 = new X509Certificate2("amscer.pfx", "AMSGIT");
                SigningCredentials cred = new X509SigningCredentials(templatex509Certificate2);

                TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate(TokenType.JWT);
                tokenRestrictionTemplate.PrimaryVerificationKey = new X509CertTokenVerificationKey(templatex509Certificate2);
                tokenRestrictionTemplate.Audience = audience;
                tokenRestrictionTemplate.Issuer = issuer;

                string optionName = "GetHlsKeyDeliveryUrlAndFetchKeyWithJWTAuthentication";
                string requirements = TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, optionName,
                    ContentKeyDeliveryType.BaselineHttp, requirements, null, ContentKeyRestrictionType.TokenRestricted);

                JwtSecurityToken token = new JwtSecurityToken(issuer: tokenRestrictionTemplate.Issuer,
                    audience: tokenRestrictionTemplate.Audience, notBefore: DateTime.Now.AddMinutes(-5),
                    expires: DateTime.Now.AddMinutes(5), signingCredentials: cred);

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                string jwtTokenString = handler.WriteToken(token);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);

                Assert.IsNotNull(keyDeliveryServiceUri);

                // Enable once all accounts are enabled for per customer Key Delivery Urls
                //Assert.IsTrue(keyDeliveryServiceUri.Host.StartsWith(_mediaContext.Credentials.ClientId));

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                byte[] key = keyClient.AcquireHlsKeyWithBearerHeader(keyDeliveryServiceUri, jwtTokenString);

                string expectedString = GetString(expectedKey);
                string fetchedString = GetString(key);
                Assert.AreEqual(expectedString, fetchedString);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        public void GetHlsKeyDeliveryUrlAndFetchKeyWithADJWTAuthUsingADOpenConnectDiscovery()
        {
            //
            // The Client ID is used by the application to uniquely identify itself to Azure AD.
            // The App Key is a credential used by the application to authenticate to Azure AD.
            // The Tenant is the name of the Azure AD tenant in which this application is registered.
            // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
            // The Authority is the sign-in URL of the tenant.
            //
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
            string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            //
            // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
            // To contact the To Do list service we need it's URL as well.
            //
            string appResourceId = ConfigurationManager.AppSettings["app:AppResourceId"];

            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(clientId, appKey);

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption, out expectedKey, "GetHlsKeyDeliveryUrlAndFetchKeyWithADJWTAuthUsingADOpenConnectDiscovery"+Guid.NewGuid().ToString());

                TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate(TokenType.JWT);
                tokenRestrictionTemplate.OpenIdConnectDiscoveryDocument = new OpenIdConnectDiscoveryDocument("https://login.windows.net/common/.well-known/openid-configuration");
                var result = authContext.AcquireTokenAsync(appResourceId, clientCredential).Result;
                string jwtTokenString = result.AccessToken;
                
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                Assert.IsTrue(handler.CanReadToken(jwtTokenString));
                JwtSecurityToken token = handler.ReadToken(jwtTokenString) as JwtSecurityToken;
                Assert.IsNotNull(token);

                tokenRestrictionTemplate.Audience =  token.Audiences.First();
                tokenRestrictionTemplate.Issuer = token.Issuer;

                string optionName = "GetHlsKeyDeliveryUrlAndFetchKeyWithJWTAuthentication";
                string requirements = TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, optionName, ContentKeyDeliveryType.BaselineHttp, requirements, null, ContentKeyRestrictionType.TokenRestricted);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);
                Assert.IsNotNull(keyDeliveryServiceUri);

                // Enable once all accounts are enabled for per customer Key Delivery Urls
                //Assert.IsTrue(keyDeliveryServiceUri.Host.StartsWith(_mediaContext.Credentials.ClientId));

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                byte[] key = keyClient.AcquireHlsKeyWithBearerHeader(keyDeliveryServiceUri, jwtTokenString);

                string expectedString = GetString(expectedKey);
                string fetchedString = GetString(key);
                Assert.AreEqual(expectedString, fetchedString);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        public void FetchKeyWithRSATokenValidationKeyAsPrimaryVerificationKey()
        {

            //Create a new RSACryptoServiceProvider object. 
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                //Export the key information to an RSAParameters object. 
                //Pass false to export the public key information or pass 
                //true to export public and private key information.
                RSAParameters RSAParams = RSA.ExportParameters(true);

                TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate(TokenType.JWT);

                var tokenVerificationKey   = new RsaTokenVerificationKey();
                tokenVerificationKey.InitFromRsaParameters(RSAParams);
                tokenRestrictionTemplate.PrimaryVerificationKey = tokenVerificationKey;

                tokenRestrictionTemplate.Audience = "http://sampleIssuerUrl";
                tokenRestrictionTemplate.Issuer = "http://sampleAudience";
                string requirements = TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
            }

           


        }
       

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GetHlsKeyDeliveryUrlAndFetchKeyWithSWTAuthenticationWhenIssuerIsUri()
        {

            string audience = "http://sampleAudience";
            string issuer = "http://sampleIssuerUrl";
            FetchKeyWithSWTToken(audience, issuer);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void GetHlsKeyDeliveryUrlAndFetchKeyWithSWTAuthenticationWhenIssuerIsStringGuidShouldThrow()
        {

            string audience = "http://www.microsoft.com";
            string issuer = Guid.NewGuid().ToString();
            try
            {
                FetchKeyWithSWTToken(audience, issuer);
            }
            catch (InvalidDataContractException ex)
            {
                Assert.IsTrue(ex.Message == "SWT token type template validation error.  template.Issuer is not valid absolute Uri.");
                throw;
            }
        }

        private void FetchKeyWithSWTToken(string audience, string issuer)
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                byte[] expectedKey = null;
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption, out expectedKey);

                var contentKeyId = Guid.Parse(contentKey.Id.Replace("nb:kid:UUID:", String.Empty));

                TokenRestrictionTemplate tokenRestrictionTemplate = new TokenRestrictionTemplate(TokenType.SWT);
                tokenRestrictionTemplate.PrimaryVerificationKey = new SymmetricVerificationKey();
                    // the default constructor automatically generates a random key

                tokenRestrictionTemplate.Audience = audience;
                tokenRestrictionTemplate.Issuer = issuer;
                tokenRestrictionTemplate.TokenType = TokenType.SWT;
                tokenRestrictionTemplate.RequiredClaims.Add(new TokenClaim(TokenClaim.ContentKeyIdentifierClaimType,
                    contentKeyId.ToString()));

                string optionName = "GetHlsKeyDeliveryUrlAndFetchKeyWithSWTAuthentication";
                string requirements = TokenRestrictionTemplateSerializer.Serialize(tokenRestrictionTemplate);
                ContentKeyRestrictionType restrictionType = ContentKeyRestrictionType.TokenRestricted;
                var _testOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, optionName,
                    ContentKeyDeliveryType.BaselineHttp, requirements, null, restrictionType);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    _testOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);


                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);

                Assert.IsNotNull(keyDeliveryServiceUri);

                Assert.IsTrue(keyDeliveryServiceUri.Host.StartsWith(_mediaContext.Credentials.ClientId));

                KeyDeliveryServiceClient keyClient = new KeyDeliveryServiceClient(RetryPolicy.DefaultFixed);
                string swtTokenString = TokenRestrictionTemplateSerializer.GenerateTestToken(tokenRestrictionTemplate,
                    tokenRestrictionTemplate.PrimaryVerificationKey, contentKeyId, DateTime.Now.AddDays(2));
                byte[] key = keyClient.AcquireHlsKeyWithBearerHeader(keyDeliveryServiceUri, swtTokenString);

                string expectedString = GetString(expectedKey);
                string fetchedString = GetString(key);
                Assert.AreEqual(expectedString, fetchedString);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsurePlayReadyLicenseDeliveryUrlForEnvelopeKeyFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.PlayReadyLicense);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsureEnvelopeKeyDeliveryUrlForCommonKeyFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            PlayReadyLicenseResponseTemplate responseTemplate = new PlayReadyLicenseResponseTemplate();
            responseTemplate.LicenseTemplates.Add(new PlayReadyLicenseTemplate());
            string licenseTemplate = MediaServicesLicenseTemplateSerializer.Serialize(responseTemplate);

            try
            {
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.CommonEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.PlayReadyLicense, null, licenseTemplate, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsureNoneKeyDeliveryUrlFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(_mediaContext, ContentKeyType.EnvelopeEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption>
                {
                    policyOption
                };

                contentKeyAuthorizationPolicy = CreateTestPolicy(_mediaContext, String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.None);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        #region HelperMethods

        private void CleanupKeyAndPolicy(IContentKey contentKey, IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy, IContentKeyAuthorizationPolicyOption policyOption)
        {
            if (contentKey != null)
            {
                contentKey.Delete();
            }

            if (contentKeyAuthorizationPolicy != null)
            {
                contentKeyAuthorizationPolicy.Delete();
            }

            /*
            if (policyOption != null)
            {
                policyOption.Delete();
            }
            */
        }

        public static IContentKey CreateTestKey(CloudMediaContext mediaContext, ContentKeyType contentKeyType, string name = "")
        {
            byte[] key;
            return CreateTestKey(mediaContext, contentKeyType, out key);
        }

        public static IContentKey CreateTestKey(CloudMediaContext mediaContext, ContentKeyType contentKeyType, out byte[] key, string name = "")
        {
            key = ContentKeyTests.GetRandomBuffer(16);
            IContentKey contentKey = mediaContext.ContentKeys.Create(Guid.NewGuid(), key, name, contentKeyType);

            return contentKey;
        }

        public static IContentKey CreateTestKeyWithSpecified(string keyIdentifier, CloudMediaContext mediaContext, ContentKeyType contentKeyType, string name = "")
        {
            var keyId = EncryptionUtils.GetKeyIdAsGuid(keyIdentifier);
            SymmetricAlgorithm symmetricAlgorithm = new AesCryptoServiceProvider();
            if ((contentKeyType == ContentKeyType.CommonEncryption) ||
                (contentKeyType == ContentKeyType.EnvelopeEncryption))
            {
                symmetricAlgorithm.KeySize = EncryptionUtils.KeySizeInBitsForAes128;
            }
            else
            {
                symmetricAlgorithm.KeySize = EncryptionUtils.KeySizeInBitsForAes256;
            }
            IContentKey contentKey = mediaContext.ContentKeys.Create(keyId, symmetricAlgorithm.Key, name, contentKeyType);

            return contentKey;
        }

        public static Guid GetGuidFromBase64String(string base64keyId)
        {
            byte[] keyIdBytes = Convert.FromBase64String(base64keyId);
            Guid keyId = new Guid(keyIdBytes);
            return keyId;
        }

        public static IContentKeyAuthorizationPolicy CreateTestPolicy(CloudMediaContext mediaContext, string name, List<IContentKeyAuthorizationPolicyOption> policyOptions, ref IContentKey contentKey)
        {
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(name).Result;

            foreach (IContentKeyAuthorizationPolicyOption option in policyOptions)
            {
                contentKeyAuthorizationPolicy.Options.Add(option);
            }

            // Associate the content key authorization policy with the content key
            contentKey.AuthorizationPolicyId = contentKeyAuthorizationPolicy.Id;
            contentKey = contentKey.UpdateAsync().Result;

            return contentKeyAuthorizationPolicy;
        }


        public static string GetString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        #endregion // HelperMethods
    }
}
