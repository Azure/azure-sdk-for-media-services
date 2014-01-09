//-----------------------------------------------------------------------
// <copyright file="InputAssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Enumerable for task input assets.
    /// </summary>
    /// <typeparam name="T">The type of element.</typeparam>
    public class InputAssetCollection<T> : ICollection<T>
    {
        private readonly List<T> _assets;
        private ITask _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputAssetCollection&lt;T&gt;"/> class.
        /// </summary>
        public InputAssetCollection()
        {
            this._assets = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputAssetCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="assets">The assets.</param>
        internal InputAssetCollection(ITask task, IEnumerable<T> assets)
        {
            this._assets = new List<T>(assets);
            this._task = task;
        }

        /// <summary>
        /// Gets the count of element within a enumerable.
        /// </summary>
        public int Count
        {
            get { return this._assets.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return !string.IsNullOrEmpty(this._task.Id); }
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.WindowsAzure.MediaServices.Client.IAsset"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public T this[int index]
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
        public IEnumerator<T> GetEnumerator()
        {
            return this._assets.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds the specified item to a collection.
        /// </summary>
        /// <param name="item">The asset.</param>
        public void Add(T item)
        {
            this.CheckIfTaskIsPersistedAndThrowNotSupported();

            this._assets.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            this.CheckIfTaskIsPersistedAndThrowNotSupported();

            this._assets.Clear();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            this.CheckIfTaskIsPersistedAndThrowNotSupported();

            return this._assets.Remove(item);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return this._assets.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this._assets.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Adds the range of assets to a collection.
        /// </summary>
        /// <param name="assets">The assets to add.</param>
        public void AddRange(IEnumerable<T> assets)
        {
            this.CheckIfTaskIsPersistedAndThrowNotSupported();

            this._assets.AddRange(assets);
        }

        /// <summary>
        /// Checks if task is persisted and throw not supported exception.
        /// </summary>
        private void CheckIfTaskIsPersistedAndThrowNotSupported()
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorReadOnlyCollectionToSubmittedTask, "InputMediaAssets"));
            }
        }
    }
}