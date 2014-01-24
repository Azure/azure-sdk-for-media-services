//-----------------------------------------------------------------------
// <copyright file="AssetFileCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IAssetFile"/>.
    /// </summary>
    internal class AssetFileCollection : AssetFileBaseCollection
    {
        /// <summary>
        /// The name of the file set.
        /// </summary>
        public const string FileSet = "Files";

        private readonly Lazy<IQueryable<IAssetFile>> _assetFileQuery; 
        private readonly IAsset _parentAsset;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFileCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The cloud media context.</param>
        internal AssetFileCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            MediaServicesClassFactory factory = this.MediaContext.MediaServicesClassFactory;
            this._assetFileQuery = new Lazy<IQueryable<IAssetFile>>(() => factory.CreateDataServiceContext().CreateQuery<IAssetFile, AssetFileData>(FileSet));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFileCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The cloud media context.</param>
        /// <param name="parentAsset">The parent <see cref="IAsset"/>.</param>
        internal AssetFileCollection(MediaContextBase cloudMediaContext, IAsset parentAsset)
            : this(cloudMediaContext)
        {
            _parentAsset = parentAsset;
            MediaServicesClassFactory factory = this.MediaContext.MediaServicesClassFactory;
            this._assetFileQuery = new Lazy<IQueryable<IAssetFile>>(() => factory.CreateDataServiceContext().CreateQuery<IAssetFile, AssetFileData>(FileSet));
        }

        /// <summary>
        /// Gets the queryable collection of file information items.
        /// </summary>
        protected override IQueryable<IAssetFile> Queryable
        {
            get { return _parentAsset != null ? this._assetFileQuery.Value.Where(c => c.ParentAssetId == _parentAsset.Id) : this._assetFileQuery.Value; }
            set { throw new NotSupportedException(); }
        }


        /// <summary>
        /// Creates the the <see cref="IAssetFile"/>
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <returns><see cref="IAssetFile"/></returns>
        public override IAssetFile Create(string name)
        {
            return CreateAsync(name, CancellationToken.None).Result;
        }

        public override Task<IAssetFile> CreateAsync(string name, CancellationToken cancelation)
        {
            if (_parentAsset == null)
            {
                throw new InvalidOperationException(StringTable.AssetFileCreateParentAssetIsNull);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, StringTable.ErrorCreatingAssetFileEmptyFileName));
            }
            cancelation.ThrowIfCancellationRequested();
            IMediaDataServiceContext dataContext = null;
            AssetFileData assetFile = null;
            return Task.Factory.StartNew(() =>
            {
                cancelation.ThrowIfCancellationRequested();
                dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();

                bool isEncrypted = _parentAsset.Options.HasFlag(AssetCreationOptions.CommonEncryptionProtected) || _parentAsset.Options.HasFlag(AssetCreationOptions.StorageEncrypted) ||
                                   _parentAsset.Options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected);

                string scheme = null;
                string schemeVersion = null;
                string encryptionKeyId = null;

                if (_parentAsset.Options.HasFlag(AssetCreationOptions.StorageEncrypted))
                {
                    IContentKey storageEncryptionKey = _parentAsset.ContentKeys.Where(c => c.ContentKeyType == ContentKeyType.StorageEncryption).FirstOrDefault();
                    cancelation.ThrowIfCancellationRequested();
                    if (storageEncryptionKey != null)
                    {
                        encryptionKeyId = storageEncryptionKey.Id.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, StringTable.StorageEncryptionContentKeyIsMissing, _parentAsset.Id));
                    }

                    scheme = FileEncryption.SchemeName;
                    schemeVersion = FileEncryption.SchemeVersion;
                }
                else if (_parentAsset.Options.HasFlag(AssetCreationOptions.CommonEncryptionProtected))
                {
                    scheme = CommonEncryption.SchemeName;
                    schemeVersion = CommonEncryption.SchemeVersion;
                }
                else if (_parentAsset.Options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected))
                {
                    scheme = EnvelopeEncryption.SchemeName;
                    schemeVersion = EnvelopeEncryption.SchemeVersion;
                }

                // Set a MIME type based on the extension of the file name
                string mimeType = AssetFileData.GetMimeType(name);
                assetFile = new AssetFileData
                {
                    Name = name,
                    IsEncrypted = isEncrypted,
                    EncryptionScheme = scheme,
                    EncryptionVersion = schemeVersion,
                    EncryptionKeyId = encryptionKeyId,
                    ParentAssetId = _parentAsset.Id,
                    MimeType = mimeType,
                };

                dataContext.AddObject(AssetFileCollection.FileSet, assetFile);
                cancelation.ThrowIfCancellationRequested();
                cancelation.ThrowIfCancellationRequested();
                MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();
                return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() =>
                {
                    cancelation.ThrowIfCancellationRequested();
                    return dataContext.SaveChangesAsync(assetFile);

                }, cancelation).Result;
            }, cancelation)
            .ContinueWith<IAssetFile>(t =>
            {

                t.ThrowIfFaulted();
                AssetFileData data = (AssetFileData)t.Result.AsyncState;
                return data;
            }, cancelation);

        }
    }
}
