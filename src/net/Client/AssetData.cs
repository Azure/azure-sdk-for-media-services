//-----------------------------------------------------------------------
// <copyright file="AssetData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class AssetData : IAsset, ICloudMediaContextInit
    {
        private const string ContentKeysPropertyName = "ContentKeys";
        private const string LocatorsPropertyName = "Locators";
        private const string ParentAssetsPropertyName = "ParentAssets";

        private AssetFileCollection _fileCollection;
        private ReadOnlyCollection<ILocator> _locatorCollection;
        private IList<IContentKey> _contentKeyCollection;
        private ReadOnlyCollection<IAsset> _parentAssetCollection;
        private CloudMediaContext _cloudMediaContext;

        private readonly object _contentKeyLocker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetData"/> class.
        /// </summary>
        public AssetData()
        {
            this.Locators = new List<LocatorData>();
            this.ContentKeys = new List<ContentKeyData>();
            this.Files = new List<AssetFileData>();
            
        }

        /// <summary>
        /// Gets or sets the parent assets.
        /// </summary>
        /// <value>
        /// The parent assets.
        /// </value>
        public List<AssetData> ParentAssets { get; set; }

        /// <summary>
        /// Gets or sets the content keys.
        /// </summary>
        /// <value>
        /// The content keys.
        /// </value>
        public List<ContentKeyData> ContentKeys { get; set; }

        /// <summary>
        /// Gets a collection of files contained by the asset.
        /// </summary>
        /// <value>A collection of files contained by the Asset.</value>
        AssetFileBaseCollection IAsset.AssetFiles 
        {
            get
            {
                if (_fileCollection == null && _cloudMediaContext != null)
                {
                    this._fileCollection = new AssetFileCollection(_cloudMediaContext, this);
                }
                return _fileCollection;

            }
        }


        public List<AssetFileData> Files { get; set; }


        /// <summary>
        /// Gets or sets the locators.
        /// </summary>
        /// <value>
        /// The locators.
        /// </value>
        public List<LocatorData> Locators { get; set; }

        /// <summary>
        /// Gets the Content Keys associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IContentKey"/> associated with the Asset.</value>
        IList<IContentKey> IAsset.ContentKeys
        {
            get
            {
                lock (_contentKeyLocker)
                {
                    if ((this._contentKeyCollection == null) && !string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(AssetCollection.AssetSet, this);
                        dataContext.LoadProperty(this, ContentKeysPropertyName);

                        this._contentKeyCollection = new LinkCollection<IContentKey, ContentKeyData>(dataContext, this, ContentKeysPropertyName, this.ContentKeys);
                    }

                    return this._contentKeyCollection;
                }
            }
        }

        /// <summary>
        /// Gets the Locators associated with this asset.
        /// </summary>
        /// <value>A Collection of <see cref="ILocator"/> that are associated with the Asset.</value>
        /// <remarks>This collection is not modifiable. Instead a SAS locator is created from calling <see cref="LocatorBaseCollection.CreateSasLocator(IAsset,IAccessPolicy)"/>.</remarks>
        ReadOnlyCollection<ILocator> IAsset.Locators
        {
            get
            {
                if (((this._locatorCollection == null) || (this.Locators == null)) && !string.IsNullOrWhiteSpace(this.Id))
                {

                    IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(AssetCollection.AssetSet, this);
                    dataContext.LoadProperty(this, LocatorsPropertyName);
                    if (this.Locators != null)
                    {
                        this._locatorCollection = this.Locators.ToList<ILocator>().AsReadOnly();
                    }
                    else
                    {
                        return new ReadOnlyCollection<ILocator>(new List<ILocator>());
                    }
                }

                return this._locatorCollection;
            }
        }

        /// <summary>
        /// Gets the parent assets that were used to create the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAsset"/> associated with the Asset.</value>
        ReadOnlyCollection<IAsset> IAsset.ParentAssets
        {
            get
            {
                if ((this._parentAssetCollection == null) && !string.IsNullOrWhiteSpace(this.Id))
                {
                    IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(AssetCollection.AssetSet, this);
                    dataContext.LoadProperty(this, ParentAssetsPropertyName);

                    this._parentAssetCollection = this.ParentAssets.ToList<IAsset>().AsReadOnly();
                }

                return this._parentAssetCollection;
            }
        }

       

        /// <summary>
        /// Inits the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            this._cloudMediaContext = context;
            InvalidateLocatorsCollection();
            InvalidateContentKeysCollection();
            InvalidateFilesCollection();
            this._fileCollection = new AssetFileCollection(context,this);
        }

        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the Asset
        /// </summary>
        IStorageAccount IAsset.StorageAccount
        {
            get
            {
                return this._cloudMediaContext.StorageAccounts.Where(c => c.Name == this.StorageAccountName).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the asset storage container Uri.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        Uri IAsset.Uri
        {
            get
            {
                System.Uri uri;
                if (System.Uri.TryCreate(this.Uri, UriKind.Absolute, out uri))
                {
                    return uri;
                }
                else
                {
                    throw new UriFormatException(StringTable.InvalidAssetUriException);
                }
                
            } 
        }

        /// <summary>
        /// Asynchronously updates this asset instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UpdateAsync()
        {
            AssetCollection.VerifyAsset(this);

            IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AssetCollection.AssetSet, this);
            dataContext.UpdateObject(this);

            return dataContext.SaveChangesAsync(this).ContinueWith<IAsset>(
                    t =>
                        {
                            t.ThrowIfFaulted();
                            AssetData data = (AssetData) t.Result.AsyncState;
                            return data;
                        });
        }

        /// <summary>
        /// Updates this asset instance.
        /// </summary>  
        public void Update()
        {
            try
            {
                this.UpdateAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Asynchronously deletes this asset instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            AssetCollection.VerifyAsset(this);

            IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AssetCollection.AssetSet, this);
            this.InvalidateContentKeysCollection();
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this);
        }

        /// <summary>
        /// Invalidates the content key collection.
        /// </summary>
        internal void InvalidateContentKeysCollection()
        {
            this.ContentKeys.Clear();
            this._contentKeyCollection = null;
        }

        /// <summary>
        /// Deletes this asset instance.
        /// </summary>
        public void Delete()
        {
            try
            {
                this.DeleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Invalidates the locators collection.
        /// </summary>
        internal void InvalidateLocatorsCollection()
        {
            this.Locators.Clear();
            this._locatorCollection = null;
        }

        private static AssetState GetExposedState(int state)
        {
            return (AssetState)state;
        }

        private static AssetCreationOptions GetExposedOptions(int options)
        {
            return (AssetCreationOptions)options;
        }

        private void InvalidateFilesCollection()
        {
            this.Files.Clear();
            this._fileCollection = null;
        }
    }
}
