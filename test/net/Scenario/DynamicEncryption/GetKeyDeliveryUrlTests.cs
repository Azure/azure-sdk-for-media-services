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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

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
                contentKey = CreateTestKey(ContentKeyType.CommonEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.PlayReadyLicense, null, "fake configuration", ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption> { policyOption };

                contentKeyAuthorizationPolicy = CreateTestPolicy(String.Empty, options, ref contentKey);

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
        public void GetHlsKeyDeliveryUrl()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(ContentKeyType.EnvelopeEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption> { policyOption };

                contentKeyAuthorizationPolicy = CreateTestPolicy(String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);

                Assert.IsNotNull(keyDeliveryServiceUri);
                Assert.IsTrue(0 == String.Compare(keyDeliveryServiceUri.AbsolutePath, "/HlsHandler.ashx", StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsurePlayReadyLicenseDeliveryUrlForEnvelopeKeyFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(ContentKeyType.EnvelopeEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption> { policyOption };

                contentKeyAuthorizationPolicy = CreateTestPolicy(String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.PlayReadyLicense);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsureEnvelopeKeyDeliveryUrlForCommonKeyFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(ContentKeyType.CommonEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.PlayReadyLicense, null, "fake configuration", ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption> { policyOption };

                contentKeyAuthorizationPolicy = CreateTestPolicy(String.Empty, options, ref contentKey);

                Uri keyDeliveryServiceUri = contentKey.GetKeyDeliveryUrl(ContentKeyDeliveryType.BaselineHttp);
            }
            finally
            {
                CleanupKeyAndPolicy(contentKey, contentKeyAuthorizationPolicy, policyOption);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void EnsureNoneKeyDeliveryUrlFails()
        {
            IContentKey contentKey = null;
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = null;
            IContentKeyAuthorizationPolicyOption policyOption = null;

            try
            {
                contentKey = CreateTestKey(ContentKeyType.EnvelopeEncryption);

                policyOption = ContentKeyAuthorizationPolicyOptionTests.CreateOption(_mediaContext, String.Empty, ContentKeyDeliveryType.BaselineHttp, null, null, ContentKeyRestrictionType.Open);

                List<IContentKeyAuthorizationPolicyOption> options = new List<IContentKeyAuthorizationPolicyOption> { policyOption };

                contentKeyAuthorizationPolicy = CreateTestPolicy(String.Empty, options, ref contentKey);

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

        public IContentKey CreateTestKey(ContentKeyType contentKeyType, string name = "")
        {
            byte[] key = ContentKeyTests.GetRandomBuffer(16);
            var contentKey = _mediaContext.ContentKeys.Create(Guid.NewGuid(), key, name, contentKeyType);

            return contentKey;
        }

        private IContentKeyAuthorizationPolicy CreateTestPolicy(string name, List<IContentKeyAuthorizationPolicyOption> policyOptions, ref IContentKey contentKey)
        {
            IContentKeyAuthorizationPolicy contentKeyAuthorizationPolicy = _mediaContext.ContentKeyAuthorizationPolicies.CreateAsync(name).Result;

            foreach (IContentKeyAuthorizationPolicyOption option in policyOptions)
            {
                contentKeyAuthorizationPolicy.Options.Add(option);
            }

            // Associate the content key authorization policy with the content key
            contentKey.AuthorizationPolicyId = contentKeyAuthorizationPolicy.Id;
            contentKey = contentKey.UpdateAsync().Result;

            return contentKeyAuthorizationPolicy;
        }

        #endregion // HelperMethods
    }
}
