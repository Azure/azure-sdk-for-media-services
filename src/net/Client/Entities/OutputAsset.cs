//-----------------------------------------------------------------------
// <copyright file="OutputAsset.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a Task output asset.
    /// </summary>
    /// <remarks>This is used when creating task to specify properties for a Task's output.</remarks>
    internal partial class OutputAsset : BaseEntity<IAsset>, IAsset
    {
        private IJob _associatedJob;

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>The ID.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the State.
        /// </summary>
        public AssetState State { get { return AssetState.Initialized; } }

        /// <summary>
        /// Gets the date the asset was created.
        /// </summary>
        public DateTime Created { get { return DateTime.UtcNow; } }

        /// <summary>
        /// Gets the date the asset was modified.
        /// </summary>
        public DateTime LastModified { get { return DateTime.MinValue; } }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Alternate ID.
        /// </summary>
        public string AlternateId { get; set; }

        /// <summary>
        /// Sets the job which the output asset belongs to
        /// </summary>
        public IJob AssociatedJob
        {
            set { _associatedJob = value; }
        }

        /// <summary>
        /// Gets the job which the output asset belongs to
        /// </summary>
        /// <returns></returns>
        public IJob GetAssociatedJob()
        {
            return _associatedJob;
        }

        /// <summary>
        /// Gets the asset storage container Uri.
        /// </summary>
        /// <value>
        /// The URL.
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

        string Uri { get; set; }

        /// <summary>
        /// Gets or sets the options for creating the asset.
        /// </summary>
        public AssetCreationOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the asset format option
        /// </summary>
        public AssetFormatOption FormatOption { get; set; }

        /// <summary>
        /// Gets a collection of files contained by the asset.
        /// </summary>
        /// <value>A collection of files contained by the Asset.</value>
        public AssetFileBaseCollection AssetFiles
        {
            get { throw new NotSupportedException(StringTable.NotSupportedFiles); }
        }

        /// <summary>
        /// Gets the Locators associated with this asset.
        /// </summary>
        /// <value>A Collection of <see cref="ILocator"/> that are associated with the Asset.</value>
        /// <remarks>This collection is not modifiable. Instead a SAS locator is created from calling <see cref="LocatorBaseCollection.CreateSasLocator(IAsset,IAccessPolicy)"/>.</remarks>
        public ReadOnlyCollection<ILocator> Locators
        {
            get { throw new NotSupportedException(StringTable.NotSupportedLocators); }
        }

        /// <summary>
        /// Gets the Content Keys associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IContentKey"/> associated with the Asset.</value>
        public IList<IContentKey> ContentKeys
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets or sets the parent assets that were used to create the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAsset"/> associated with the Asset.</value>
        public ReadOnlyCollection<IAsset> ParentAssets { get; set; }

        /// <summary>
        /// Gets storage account name associated with the Asset
        /// </summary>
        public string StorageAccountName { get; internal set; }

        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the Asset
        /// </summary>
        IStorageAccount IAsset.StorageAccount
        {
            get
            {
                if (GetMediaContext() == null)
                {
                    throw new NullReferenceException("Operation can't be performed. CloudMediaContext hasn't been initiliazed for OutputAsset type");
                }
                return this.GetMediaContext().StorageAccounts.Where(c => c.Name == this.StorageAccountName).FirstOrDefault();
            }
        }

        public AssetFilterBaseCollection AssetFilters
        {
            get { throw new NotSupportedException(); }
        }


        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task;.</returns>
        public Task UpdateAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Deletes this asset instance
        /// <param name="keepAzureStorageContainer"> Determines whether or not the underlying storage asset container is preseved during the delete operation</param>
        /// </summary>
        /// <param name="keepAzureStorageContainer">if set to <c>true</c> underlying storage asset container is preserved during the delete operation.</param>
        /// <returns>IMediaDataServiceResponse.</returns>
        public Task<IMediaDataServiceResponse> DeleteAsync(bool keepAzureStorageContainer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes this asset instance
        /// </summary>
        /// <param name="keepAzureStorageContainer">if set to <c>true</c> underlying storage asset container is preserved during the delete operation.</param>
        /// <returns>IMediaDataServiceResponse.</returns>
        public IMediaDataServiceResponse Delete(bool keepAzureStorageContainer)
        {
            throw new NotSupportedException();
        }
    }
}
