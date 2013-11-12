//-----------------------------------------------------------------------
// <copyright file="ContentKeyBaseCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

    /// <summary>
    /// Represents a collection of content keys.
    /// </summary>
    public abstract class ContentKeyBaseCollection : BaseCollection<IContentKey>
    {
        protected ContentKeyBaseCollection(MediaContextBase context) : base(context)
        {
        }

        /// <summary>
        /// Gets or sets the queryable collection of content keys.
        /// </summary>
        /// <value>
        /// The queryable collection of content keys.
        /// </value>
        protected IQueryable<IContentKey> ContentKeyQueryable { get; set; }

        /// <summary>
        /// Asynchronously creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.</returns>
        public Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey)
        {
            return this.CreateAsync(keyId, contentKey, string.Empty);
        }

        /// <summary>
        /// Asynchronously creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <param name="contentKeyType">Type of content key to create.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.
        /// </returns>
        public abstract Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType);

        /// <summary>
        /// Creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public IContentKey Create(Guid keyId, byte[] contentKey)
        {
            return this.Create(keyId, contentKey, string.Empty);
        }

        /// <summary>
        /// Creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <param name="contentKeyType">Type of content key to create.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public abstract IContentKey Create(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType);

        /// <summary>
        /// Asynchronously creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.</returns>
        public abstract Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name);

        /// <summary>
        /// Creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public abstract IContentKey Create(Guid keyId, byte[] contentKey, string name);

        /// <summary>
        /// Verifies the content key.
        /// </summary>
        /// <param name="contentKey">The content key.</param>
        internal static void VerifyContentKey(IContentKey contentKey)
        {
            if (!(contentKey is ContentKeyData))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidContentKeyType);
            }
        }

        /// <summary>
        /// Creates the storage content key.
        /// </summary>
        /// <param name="fileEncryption">The file encryption.</param>
        /// <param name="cert">The cert.</param>
        /// <returns>The content key.</returns>
        internal static ContentKeyData InitializeStorageContentKey(FileEncryption fileEncryption, X509Certificate2 cert)
        {
            byte[] encryptedContentKey = fileEncryption.EncryptContentKeyToCertificate(cert);

            ContentKeyData contentKeyData = new ContentKeyData
            {
                Id = fileEncryption.GetKeyIdentifierAsString(),
                EncryptedContentKey = Convert.ToBase64String(encryptedContentKey),
                ContentKeyType = (int)ContentKeyType.StorageEncryption,
                ProtectionKeyId = cert.Thumbprint,
                ProtectionKeyType = (int)ProtectionKeyType.X509CertificateThumbprint,
                Checksum = fileEncryption.GetChecksum()
            };

            return contentKeyData;
        }

        /// <summary>
        /// Creates the common content key.
        /// </summary>
        /// <param name="keyId">The key id.</param>
        /// <param name="contentKey">The content key data.</param>
        /// <param name="name">The name.</param>
        /// <param name="cert">The cert.</param>
        /// <returns>The content key.</returns>
        internal static ContentKeyData InitializeCommonContentKey(Guid keyId, byte[] contentKey, string name, X509Certificate2 cert)
        {
            byte[] encryptedContentKey = CommonEncryption.EncryptContentKeyToCertificate(cert, contentKey);

            ContentKeyData contentKeyData = new ContentKeyData
            {
                Id = EncryptionUtils.GetKeyIdentifierAsString(keyId),
                EncryptedContentKey = Convert.ToBase64String(encryptedContentKey),
                ContentKeyType = (int)ContentKeyType.CommonEncryption,
                ProtectionKeyId = cert.Thumbprint,
                ProtectionKeyType = (int)ProtectionKeyType.X509CertificateThumbprint,
                Name = name,
                Checksum = EncryptionUtils.CalculateChecksum(contentKey, keyId)
            };

            return contentKeyData;
        }

        /// <summary>
        /// Creates an envelope encryption content key.
        /// </summary>
        /// <param name="keyId">The key id.</param>
        /// <param name="contentKey">The content key data.</param>
        /// <param name="name">The name.</param>
        /// <param name="cert">The cert.</param>
        /// <returns>The content key.</returns>
        internal static ContentKeyData InitializeEnvelopeContentKey(Guid keyId, byte[] contentKey, string name, X509Certificate2 cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }

            if (contentKey == null)
            {
                throw new ArgumentNullException("contentKey");
            }

            if (contentKey.Length != EncryptionUtils.KeySizeInBytesForAes128)
            {
                throw new ArgumentOutOfRangeException("contentKey", "Envelope Encryption content keys are 128-bits (16 bytes) in length.");
            }

            byte[] encryptedContentKey = EncryptionUtils.EncryptSymmetricKeyData(cert, contentKey);

            ContentKeyData contentKeyData = new ContentKeyData
            {
                Id = EncryptionUtils.GetKeyIdentifierAsString(keyId),
                EncryptedContentKey = Convert.ToBase64String(encryptedContentKey),
                ContentKeyType = (int)ContentKeyType.EnvelopeEncryption,
                ProtectionKeyId = cert.Thumbprint,
                ProtectionKeyType = (int)ProtectionKeyType.X509CertificateThumbprint,
                Name = name,
                Checksum = EncryptionUtils.CalculateChecksum(contentKey, keyId)
            };

            return contentKeyData;
        }

        /// <summary>
        /// Creates the configuration content key.
        /// </summary>
        /// <param name="configEncryption">The config encryption.</param>
        /// <param name="cert">The cert.</param>
        /// <returns>The content key.</returns>
        internal static ContentKeyData InitializeConfigurationContentKey(ConfigurationEncryption configEncryption, X509Certificate2 cert)
        {
            byte[] encryptedContentKey = configEncryption.EncryptContentKeyToCertificate(cert);

            ContentKeyData contentKeyData = new ContentKeyData
            {
                Id = configEncryption.GetKeyIdentifierAsString(),
                EncryptedContentKey = Convert.ToBase64String(encryptedContentKey),
                ContentKeyType = (int)ContentKeyType.ConfigurationEncryption,
                ProtectionKeyId = cert.Thumbprint,
                ProtectionKeyType = (int)ProtectionKeyType.X509CertificateThumbprint,
                Checksum = configEncryption.GetChecksum()
            };

            return contentKeyData;
        }

        /// <summary>
        /// Gets the protection key id for content key.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="contentKeyType">Type of the content key.</param>
        /// <returns>The content key.</returns>
        internal static string GetProtectionKeyIdForContentKey(IMediaDataServiceContext dataContext, ContentKeyType contentKeyType)
        {
            // First query Nimbus to find out what certificate to encrypt the content key with.
            string uriString = string.Format(CultureInfo.InvariantCulture, "/GetProtectionKeyId?contentKeyType={0}", Convert.ToInt32(contentKeyType, CultureInfo.InvariantCulture));
            Uri uriGetProtectionKeyId = new Uri(uriString, UriKind.Relative);

            IEnumerable<string> results = dataContext.Execute<string>(uriGetProtectionKeyId);

            return results.Single();
        }

        /// <summary>
        /// Gets the certificate for protection key id.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="protectionKeyId">The protection key id.</param>
        /// <returns>The content key.</returns>
        internal static X509Certificate2 GetCertificateForProtectionKeyId(IMediaDataServiceContext dataContext, string protectionKeyId)
        {
            // First check to see if we have the cert in our store already.
            X509Certificate2 certToUse = EncryptionUtils.GetCertificateFromStore(protectionKeyId);

            if ((certToUse == null) && (dataContext != null))
            {
                // If not, download it from Nimbus to use.
                Uri uriGetProtectionKey = new Uri(string.Format(CultureInfo.InvariantCulture, "/GetProtectionKey?protectionKeyId='{0}'", protectionKeyId), UriKind.Relative);
                IEnumerable<string> results2 = dataContext.Execute<string>(uriGetProtectionKey);
                string certString = results2.Single();

                byte[] certBytes = Convert.FromBase64String(certString);
                certToUse = new X509Certificate2(certBytes);

                // Finally save it for next time.
                EncryptionUtils.SaveCertificateToStore(certToUse);
            }

            return certToUse;
        }

        /// <summary>
        /// Gets the certificate to encrypt content key.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="contentKeyType">Type of the content key.</param>
        /// <returns>The content key.</returns>
        internal static X509Certificate2 GetCertificateToEncryptContentKey(IMediaDataServiceContext dataContext, ContentKeyType contentKeyType)
        {
            string thumbprint = GetProtectionKeyIdForContentKey(dataContext, contentKeyType);

            return GetCertificateForProtectionKeyId(dataContext, thumbprint);
        }
    }
}
