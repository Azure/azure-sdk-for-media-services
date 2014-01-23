//-----------------------------------------------------------------------
// <copyright file="IngestManifestFileCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    ///  Represents a collection of <see cref="IIngestManifestFile"/>.
    /// </summary>
    public class IngestManifestFileCollection : BaseCollection<IIngestManifestFile>
    {

        internal const string EntitySet = "IngestManifestFiles";
        private readonly Lazy<IQueryable<IIngestManifestFile>> _query;
        private readonly IngestManifestAssetData _parentIngestManifestAsset;


        /// <summary>
        /// Initializes a new instance of the <see cref="IngestManifestFileCollection"/> class.
        /// </summary>
        /// <param name="mediaContext"></param>
        /// <param name="parentIngestManifestAsset">The parent manifest asset.</param>
        internal IngestManifestFileCollection(MediaContextBase mediaContext, IIngestManifestAsset parentIngestManifestAsset)
            : base(mediaContext)
        {
            MediaServicesClassFactory factory = this.MediaContext.MediaServicesClassFactory;
            this._query = new Lazy<IQueryable<IIngestManifestFile>>(() => factory.CreateDataServiceContext().CreateQuery<IIngestManifestFile, IngestManifestFileData>(EntitySet));

            if (parentIngestManifestAsset != null)
            {
                this._parentIngestManifestAsset = (IngestManifestAssetData)parentIngestManifestAsset;
            }
        }
        /// <summary>
        /// Creates the manifest asset file
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns><see cref="IIngestManifestFile"/></returns>
        public IIngestManifestFile Create(string filePath)
        {
            if (this._parentIngestManifestAsset == null)
            {
                throw new InvalidOperationException(StringTable.InvalidCreateManifestAssetFileOperation);
            }

            return this.CreateAsync(_parentIngestManifestAsset, filePath, CancellationToken.None).Result;
        }

        /// <summary>
        /// Creates the manifest asset file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/>of type <see cref="IIngestManifestFile"/></returns>
        public Task<IIngestManifestFile> CreateAsync(string filePath, CancellationToken token)
        {
            if (this._parentIngestManifestAsset == null)
            {
                throw new InvalidOperationException(StringTable.InvalidCreateManifestAssetFileOperation);
            }
            return this.CreateAsync(this._parentIngestManifestAsset, filePath, token);
        }

        /// <summary>
        /// Creates the manifest asset file asynchronously.
        /// </summary>
        /// <param name="ingestManifestAsset">The parent manifest asset.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/>of type <see cref="IIngestManifestFile"/></returns>
        public Task<IIngestManifestFile> CreateAsync(IIngestManifestAsset ingestManifestAsset, string filePath,CancellationToken token)
        {
            if(ingestManifestAsset==null)
            {
                throw new ArgumentNullException("ingestManifestAsset");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, StringTable.ErrorCreatingIngestManifestFileEmptyFilePath));
            }

            AssetCreationOptions options = ingestManifestAsset.Asset.Options;
            
            Task<IIngestManifestFile> rootTask = new Task<IIngestManifestFile>(() =>
            {
                token.ThrowIfCancellationRequested();

                IngestManifestAssetCollection.VerifyManifestAsset(ingestManifestAsset);

                IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();

                // Set a MIME type based on the extension of the file name
                string mimeType = AssetFileData.GetMimeType(filePath);

                IngestManifestFileData data = new IngestManifestFileData
                {
                    Name = Path.GetFileName(filePath),
                    MimeType = mimeType,
                    ParentIngestManifestId = ingestManifestAsset.ParentIngestManifestId,
                    ParentIngestManifestAssetId = ingestManifestAsset.Id,
                    Path = filePath,
                };

                SetEncryptionSettings(ingestManifestAsset, options, data);

                dataContext.AddObject(EntitySet, data);

                MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

                Task<IIngestManifestFile> task = retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(data))
                    .ContinueWith<IIngestManifestFile>(t =>
                    {
                        t.ThrowIfFaulted();
                        token.ThrowIfCancellationRequested();
                        IngestManifestFileData ingestManifestFile = (IngestManifestFileData)t.Result.AsyncState;                   
                        return ingestManifestFile;
                    });

                return task.Result;

            });
            rootTask.Start();
            return rootTask;

        }

        private static void SetEncryptionSettings(IIngestManifestAsset ingestManifestAsset, AssetCreationOptions options, IngestManifestFileData data)
        {
            if (options.HasFlag(AssetCreationOptions.StorageEncrypted))
            {
                var contentKeyData = ingestManifestAsset.Asset.ContentKeys.Where(c => c.ContentKeyType == ContentKeyType.StorageEncryption).FirstOrDefault();
                if (contentKeyData == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, StringTable.StorageEncryptionContentKeyIsMissing, ingestManifestAsset.Asset.Id));
                }
                using (var fileEncryption = new FileEncryption(contentKeyData.GetClearKeyValue(), EncryptionUtils.GetKeyIdAsGuid(contentKeyData.Id)))
                {
                    if (!fileEncryption.IsInitializationVectorPresent(data.Name))
                    {
                        fileEncryption.CreateInitializationVectorForFile(data.Name);
                    }
                    ulong iv = fileEncryption.GetInitializationVectorForFile(data.Name);

                    data.IsEncrypted = true;
                    data.EncryptionKeyId = fileEncryption.GetKeyIdentifierAsString();
                    data.EncryptionScheme = FileEncryption.SchemeName;
                    data.EncryptionVersion = FileEncryption.SchemeVersion;
                    data.InitializationVector = iv.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (options.HasFlag(AssetCreationOptions.CommonEncryptionProtected))
            {
                data.IsEncrypted = true;
                data.EncryptionScheme = CommonEncryption.SchemeName;
                data.EncryptionVersion = CommonEncryption.SchemeVersion;
            }
            else if (options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected))
            {
                data.IsEncrypted = true;
                data.EncryptionScheme = EnvelopeEncryption.SchemeName;
                data.EncryptionVersion = EnvelopeEncryption.SchemeVersion;
            }        
        }

        protected override IQueryable<IIngestManifestFile> Queryable
        {
            get
            {
                if (_parentIngestManifestAsset != null)
                {
                    return this._query.Value.Where(c => c.ParentIngestManifestAssetId== _parentIngestManifestAsset.Id);
                }
                else
                {
                    return this._query.Value;
                }

            }
            set { throw new NotSupportedException(); }
        }
    }
}