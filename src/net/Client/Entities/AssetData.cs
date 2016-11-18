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
using System.Data.Services.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class AssetData : BaseEntity<IAsset>,IAsset
    {
        private const string ContentKeysPropertyName = "ContentKeys";
        private const string DeliveryPoliciesPropertyName = "DeliveryPolicies";
        private const string LocatorsPropertyName = "Locators";
        private const string ParentAssetsPropertyName = "ParentAssets";
        private const string FilterPropertyName = "AssetFilters";

        private AssetFileCollection _fileCollection;
        private AssetFilterBaseCollection _filterCollection;
        private ReadOnlyCollection<ILocator> _locatorCollection;
        private IList<IContentKey> _contentKeyCollection;
        private ReadOnlyCollection<IAsset> _parentAssetCollection;
        private MediaContextBase _mediaContextBase;

        private readonly object _contentKeyLocker = new object();
        private readonly object _deliveryPolicyLocker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetData"/> class.
        /// </summary>
        public AssetData()
        {
            this.Locators = new List<LocatorData>();
            this.ContentKeys = new List<ContentKeyData>();
            this.DeliveryPolicies = new List<AssetDeliveryPolicyData>();
            this.Files = new List<AssetFileData>();
            this.AssetFilters = new List<AssetFilterData>();
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
                if (_fileCollection == null && _mediaContextBase != null)
                {
                    this._fileCollection = new AssetFileCollection(_mediaContextBase, this);
                }
                return _fileCollection;

            }
        }

        public List<AssetFileData> Files { get; set; }

        /// <summary>
        /// Get a collection of filters for this asset
        /// </summary>
        AssetFilterBaseCollection IAsset.AssetFilters 
        {
            get
            {
                if (((this._filterCollection == null) || (this.AssetFilters == null)) && !string.IsNullOrWhiteSpace(this.Id))
                {
                    IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(AssetCollection.AssetSet, this);
                    LoadProperty(dataContext, FilterPropertyName);

                    this._filterCollection = new AssetFilterBaseCollection(_mediaContextBase, this, this.AssetFilters ?? new List<AssetFilterData>());
                }

                return this._filterCollection;
            }
        }

        public List<AssetFilterData> AssetFilters { get; set; }

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
                        IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(AssetCollection.AssetSet, this);
                        LoadProperty(dataContext, ContentKeysPropertyName);

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

                    IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(AssetCollection.AssetSet, this);
                    LoadProperty(dataContext, LocatorsPropertyName);
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
                    IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(AssetCollection.AssetSet, this);
                    LoadProperty(dataContext, ParentAssetsPropertyName);

                    if (this.ParentAssets != null)
                    {
                        this._parentAssetCollection = this.ParentAssets.ToList<IAsset>().AsReadOnly();
                    }
                    else
                    {
                        return new ReadOnlyCollection<IAsset>(new List<IAsset>());
                    }
                }

                return this._parentAssetCollection;
            }
        }

       

        /// <summary>
        /// Inits the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void InitCloudMediaContext(MediaContextBase context)
        {
            this._mediaContextBase = context;
            InvalidateLocatorsCollection();
            InvalidateContentKeysCollection();
            InvalidateDeliveryPoliciesCollection();
            InvalidateFilesCollection();
            InvalidateFilterCollection();
            if (context != null)
            {
                this._fileCollection = new AssetFileCollection(context, this);
            }
        }

        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the Asset
        /// </summary>
        IStorageAccount IAsset.StorageAccount
        {
            get
            {
                return this._mediaContextBase.StorageAccounts.Where(c => c.Name == this.StorageAccountName).FirstOrDefault();
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
                Uri uri;
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

            IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AssetCollection.AssetSet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this._mediaContextBase.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                   .ContinueWith<IAsset>(
                       t =>
                       {
                           t.ThrowIfFaulted();
                           AssetData data = (AssetData)t.Result.AsyncState;
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
        /// Deletes this asset instance including underlying azure storage container
        /// </summary>
        public Task DeleteAsync()
        {
            return DeleteAsync(false);
        }


        /// <summary>
        /// Asynchronously deletes this asset instance.
        /// </summary>
        /// <param name="keepAzureStorageContainer">if set to <c>true</c> underlying storage asset container is preserved during the delete operation.</param>
        /// <returns>Task of type <see cref="IMediaDataServiceResponse"/></returns>
        public Task<IMediaDataServiceResponse> DeleteAsync(bool keepAzureStorageContainer)
        {
            AssetCollection.VerifyAsset(this);

            AssetDeleteOptionsRequestAdapter deleteRequestAdapter = new AssetDeleteOptionsRequestAdapter(keepAzureStorageContainer);
            IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext(new[] { deleteRequestAdapter });
            dataContext.AttachTo(AssetCollection.AssetSet, this);
            this.InvalidateContentKeysCollection();
            this.InvalidateDeliveryPoliciesCollection();
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this._mediaContextBase.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);
            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
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
        /// Deletes this asset instance
        /// </summary>
        /// <param name="keepAzureStorageContainer">if set to <c>true</c> underlying storage asset container is preserved during the delete operation.</param>
        /// <returns>IMediaDataServiceResponse.</returns>
        public IMediaDataServiceResponse Delete(bool keepAzureStorageContainer)
        {
            try
            {
                return DeleteAsync(keepAzureStorageContainer).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Deletes this asset instance 
        /// </summary>
        public void Delete()
        {
            try
            {
                var result = this.DeleteAsync(false).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten().InnerException;
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

        private static AssetFormatOption GetExposedFormatOption(int formatOption)
        {
            return (AssetFormatOption)formatOption;
        }

        private void InvalidateFilesCollection()
        {
            this.Files.Clear();
            this._fileCollection = null;
        }


        private void InvalidateFilterCollection()
        {
            this.AssetFilters.Clear();
            this._filterCollection = null;
        }

        public override void SetMediaContext(MediaContextBase value)
        {
            InitCloudMediaContext(value);
        }
        
        public override MediaContextBase GetMediaContext()
        {
            return _mediaContextBase;
        }
    }
}
