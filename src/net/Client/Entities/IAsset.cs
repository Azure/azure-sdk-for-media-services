//-----------------------------------------------------------------------
// <copyright file="IAsset.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    public partial interface IAsset
    {
        /// <summary>
        /// Gets a collection of files contained by the asset.
        /// </summary>
        /// <value>A collection of files contained by the Asset.</value>
        AssetFileBaseCollection AssetFiles { get; }

        /// <summary>
        /// Get a collection of filters for this asset
        /// </summary>
        AssetFilterBaseCollection AssetFilters { get; }

        /// <summary>
        /// Gets the Locators associated with this asset.
        /// </summary>
        /// <value>A Collection of <see cref="ILocator"/> that are associated with the Asset.</value>
        /// <remarks>This collection is not modifiable. Instead a SAS locator is created from calling <see cref="LocatorBaseCollection.CreateSasLocator(IAsset,IAccessPolicy)"/>.</remarks>
        ReadOnlyCollection<ILocator> Locators { get; }

        /// <summary>
        /// Gets the Content Keys associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IContentKey"/> associated with the Asset.</value>
        IList<IContentKey> ContentKeys { get; }

        /// <summary>
        /// Gets the parent assets that were used to create the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAsset"/> associated with the Asset.</value>
        ReadOnlyCollection<IAsset> ParentAssets { get; }

        /// <summary>
        /// Gets storage account name associated with the Asset
        /// </summary>
        string StorageAccountName { get; }

        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the Asset
        /// </summary>
        IStorageAccount StorageAccount { get; } 

        /// <summary>
        /// Asynchronously updates this asset instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Updates this asset instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Asynchronously deletes this asset instance including underlying azure storage container
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Asynchronously deletes this asset instance.
        /// </summary>
        /// <param name="keepAzureStorageContainer">Instructs if azure storage container for asset need to be preserved during delete operation</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task<IMediaDataServiceResponse> DeleteAsync(bool keepAzureStorageContainer);

        /// <summary>
        /// Deletes this asset instance including underlying azure storage container
        /// </summary>
        void Delete();
        
        /// <summary>
        /// Deletes this asset instance 
        /// <param name="keepAzureStorageContainer">Instructs if azure storage container for asset need to be preserved during delete operation</param>
        /// </summary>
        void Delete(bool keepAzureStorageContainer);
      
    }
}
