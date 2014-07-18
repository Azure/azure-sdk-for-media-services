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
using System.Data.Services.Client;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.DynamicEncryption;

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
                byte[] key = keyClient.AcquireHlsKey(keyDeliveryServiceUri, TokenServiceClient.GetAuthTokenForKey(rawkey));

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
        [ExpectedException(typeof (DataServiceQueryException))]
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
        [ExpectedException(typeof (DataServiceQueryException))]
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
        [ExpectedException(typeof (DataServiceQueryException))]
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

        public static IContentKey CreateTestKey(CloudMediaContext mediaContext,ContentKeyType contentKeyType, string name = "")
        {
            byte[] key;
            return CreateTestKey(mediaContext,contentKeyType, out key);
        }

        public static IContentKey CreateTestKey(CloudMediaContext mediaContext, ContentKeyType contentKeyType, out byte[] key, string name = "")
        {
            key = ContentKeyTests.GetRandomBuffer(16);
            IContentKey contentKey = mediaContext.ContentKeys.Create(Guid.NewGuid(), key, name, contentKeyType);

            return contentKey;
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
            char[] chars = new char[bytes.Length/sizeof (char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        #endregion // HelperMethods
    }
}
