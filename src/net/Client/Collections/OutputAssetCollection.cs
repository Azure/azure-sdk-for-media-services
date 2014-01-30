//-----------------------------------------------------------------------
// <copyright file="OutputAssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A collection of output assets.
    /// </summary>
    public class OutputAssetCollection : IEnumerable<IAsset>
    {
        private readonly List<IAsset> _assets;

        private ITask _task;
        private readonly MediaContextBase _cloudMediaContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputAssetCollection"/> class.
        /// </summary>
        public OutputAssetCollection()
        {
            this._assets = new List<IAsset>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputAssetCollection"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="assets">The assets.</param>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal OutputAssetCollection(ITask task, IEnumerable<IAsset> assets, MediaContextBase cloudMediaContext)
        {
            this._assets = new List<IAsset>(assets);
            this._task = task;
            this._cloudMediaContext = cloudMediaContext;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get { return this._assets.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is read only.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return !string.IsNullOrEmpty(this._task.Id); }
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.WindowsAzure.MediaServices.Client.IAsset"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The asset.</returns>
        public IAsset this[int index]
        {
            get { return this._assets[index]; }
        }

        #region IEnumerable<IAsset> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IAsset> GetEnumerator()
        {
            return this._assets.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds the new output asset.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="options">The options.</param>
        /// <returns>The new asset.</returns>
        public IAsset AddNew(string assetName,  AssetCreationOptions options)
        {
            if (this._cloudMediaContext.DefaultStorageAccount == null)
            {
                throw new InvalidOperationException(StringTable.DefaultStorageAccountIsNull);
            }
           return this.AddNew(assetName, _cloudMediaContext.DefaultStorageAccount.Name, options);
        }

        /// <summary>
        /// Adds the new output asset.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="storageAccountName">he name of storage account where asset will be hosted</param>
        /// <param name="options">The options.</param>
        /// <returns>The new asset.</returns>
        public IAsset AddNew(string assetName, string storageAccountName, AssetCreationOptions options)
        {
            this.CheckIfTaskIsPersistedAndThrowNotSupported();

            var asset = new OutputAsset
            {
                Name = assetName,
                Options = options,
                StorageAccountName = storageAccountName
            };

            this._assets.Add(asset);

            return asset;
        }

        /// <summary>
        /// Adds the new output asset.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>The new asset.</returns>
        public IAsset AddNew(string assetName)
        {
            return this.AddNew(assetName, AssetCreationOptions.StorageEncrypted);
        }

        private void CheckIfTaskIsPersistedAndThrowNotSupported()
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorReadOnlyCollectionToSubmittedTask, "OutputMediaAssets"));
            }
        }
    }
}