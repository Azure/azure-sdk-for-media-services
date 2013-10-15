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
        /// The name of the content key set.
        /// </summary>
        internal const string ContentKeySet = "ContentKeys";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentKeyCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal ContentKeyCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            
            this.ContentKeyQueryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<ContentKeyData>(ContentKeySet);
        }

        /// <summary>
        /// Gets the <see cref="System.Linq.IQueryable"/> interface to evaluate queries against 
        /// the collection of content keys.
        /// </summary>
        protected override IQueryable<IContentKey> Queryable
        {
            get { return this.ContentKeyQueryable; }
            set { this.ContentKeyQueryable = value; }
        }

        /// <summary>
        /// Asynchronously creates a content key with the specifies key identifier and value.
        /// </summary>
        /// <param name="keyId">The key identifier.</param>
        /// <param name="contentKey">The value of the content key.</param>
        /// <param name="name">A friendly name for the content key.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;IContentKey&gt;.
        /// </returns>
        public override Task<IContentKey> CreateAsync(Guid keyId, byte[] contentKey, string name)
        {
            if (keyId == Guid.Empty)
            {
                throw new ArgumentException(StringTable.ErrorCreateKey_EmptyGuidNotAllowed, "keyId");
            }

            if (contentKey == null)
            {
                throw new ArgumentNullException("contentKey");
            }

            if (contentKey.Length != EncryptionUtils.KeySizeInBytesForAes128)
            {
                throw new ArgumentException(StringTable.ErrorCommonEncryptionKeySize, "contentKey");
            }

            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            X509Certificate2 certToUse = ContentKeyBaseCollection.GetCertificateToEncryptContentKey(dataContext, ContentKeyType.CommonEncryption);
            ContentKeyData contentKeyData = CreateCommonContentKey(keyId, contentKey, name, certToUse);            

            dataContext.AddObject(ContentKeySet, contentKeyData);

            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

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
                Task<IContentKey> task = this.CreateAsync(keyId, contentKey, name);
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
