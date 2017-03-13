//-----------------------------------------------------------------------
// <copyright file="ContentKeyCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IContentKey"/>.
    /// </summary>
    public class ContentKeyCollection : ContentKeyBaseCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentKeyCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal ContentKeyCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
        }

        /// <summary>
        /// Asynchronously creates a content key with the specified key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.
        /// </returns>
        public override Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name)
        {
             return CreateAsync(keyId, contentKey, name, ContentKeyType.CommonEncryption);
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
        public override Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType)
        {
            return CreateAsync(keyId, contentKey, name, contentKeyType, null);
        }

        /// <summary>
        /// Asynchronously creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <param name="contentKeyType">Type of content key to create.</param>
        /// <param name="trackIdentifiers">A list of tracks to be encrypted by this content key.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.
        /// </returns>
        public override Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType, IEnumerable<string> trackIdentifiers)
        {
            var allowedKeyTypes = new[]
            {
                ContentKeyType.CommonEncryption,
                ContentKeyType.CommonEncryptionCbcs,
                ContentKeyType.EnvelopeEncryption,
                ContentKeyType.FairPlayASk,
                ContentKeyType.FairPlayPfxPassword,
            };

            if (!allowedKeyTypes.Contains(contentKeyType))
            {
                throw new ArgumentException(StringTable.ErrorUnsupportedContentKeyType, "contentKey");
            }

            if (keyId == Guid.Empty)
            {
                throw new ArgumentException(StringTable.ErrorCreateKey_EmptyGuidNotAllowed, "keyId");
            }

            if (contentKey == null)
            {
                throw new ArgumentNullException("contentKey");
            }

            if (contentKeyType != ContentKeyType.FairPlayPfxPassword &&
                contentKey.Length != EncryptionUtils.KeySizeInBytesForAes128)
            {
                throw new ArgumentException(StringTable.ErrorCommonEncryptionKeySize, "contentKey");
            }

            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            X509Certificate2 certToUse = GetCertificateToEncryptContentKey(MediaContext, ContentKeyType.CommonEncryption);

            ContentKeyData contentKeyData = null;

            if (contentKeyType == ContentKeyType.CommonEncryption)
            {
                contentKeyData = InitializeCommonContentKey(keyId, contentKey, name, certToUse);
            }
            else if (contentKeyType == ContentKeyType.CommonEncryptionCbcs)
            {
                contentKeyData = InitializeCommonContentKey(keyId, contentKey, name, certToUse);
                contentKeyData.ContentKeyType = (int)ContentKeyType.CommonEncryptionCbcs;
            }
            else if (contentKeyType == ContentKeyType.EnvelopeEncryption)
            {
                contentKeyData = InitializeEnvelopeContentKey(keyId, contentKey, name, certToUse);
            }
            else if (contentKeyType == ContentKeyType.FairPlayPfxPassword)
            {
                contentKeyData = InitializeFairPlayPfxPassword(keyId, contentKey, name, certToUse);
            }
            else if (contentKeyType == ContentKeyType.FairPlayASk)
            {
                contentKeyData = InitializeFairPlayASk(keyId, contentKey, name, certToUse);
            }

            dataContext.AddObject(ContentKeySet, contentKeyData);

            contentKeyData.TrackIdentifiers = (trackIdentifiers!= null && trackIdentifiers.Any()) ? string.Join(",", trackIdentifiers) : null;


            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(contentKeyData))
                .ContinueWith<IContentKey>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (ContentKeyData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <param name="contentKeyType">Type of content key to create.</param>
        /// <param name="trackIdentifiers">A list of tracks to be encrypted by this content key.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public override IContentKey Create(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType, IEnumerable<string> trackIdentifiers)
        {
            try
            {
                Task<IContentKey> task = this.CreateAsync(keyId, contentKey, name, contentKeyType, trackIdentifiers);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Creates a content key with the specified key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public override IContentKey Create(Guid keyId, byte[] contentKey, string name)
        {
            try
            {
                Task<IContentKey> task = this.CreateAsync(keyId, contentKey, name, ContentKeyType.CommonEncryption);

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Creates a content key with the specified key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <param name="contentKeyType">Type of content key to create.</param>
        /// <returns>A <see cref="IContentKey"/> that can be associated with an <see cref="IAsset"/>.</returns>
        public override IContentKey Create(Guid keyId, byte[] contentKey, string name, ContentKeyType contentKeyType)
        {
            try
            {
                Task<IContentKey> task = this.CreateAsync(keyId, contentKey, name, contentKeyType);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }
    }
}
