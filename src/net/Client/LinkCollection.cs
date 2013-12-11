//-----------------------------------------------------------------------
// <copyright file="LinkCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A collection of links.
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface.</typeparam>
    /// <typeparam name="TType">The type of the type.</typeparam>
    internal class LinkCollection<TInterface, TType> : ObservableCollection<TInterface>
    {
        private readonly IMediaDataServiceContext _dataContext;
        private readonly string _propertyName;
        private readonly BaseEntity _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkCollection&lt;TInterface, TType&gt;"/> class.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="items">The items.</param>
        public LinkCollection(IMediaDataServiceContext dataContext, BaseEntity parent, string propertyName, IEnumerable<TInterface> items)
            : base(items)
        {
            this._dataContext = dataContext;
            this._propertyName = propertyName;
            this._parent = parent;
        }

        /// <summary>
        /// Inserts the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, TInterface item)
        {
            ValidateItem(item);

            this._dataContext.AttachTo(GetEntitySetName(typeof(TInterface)), item);
            this._dataContext.AddLink(this._parent, this._propertyName, item);

            MediaRetryPolicy retryPolicy = this._parent.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            retryPolicy.ExecuteAction<IMediaDataServiceResponse>(() => _dataContext.SaveChanges());

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            this._dataContext.DeleteLink(this._parent, this._propertyName, this[index]);

            MediaRetryPolicy retryPolicy = this._parent.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            retryPolicy.ExecuteAction<IMediaDataServiceResponse>(() => _dataContext.SaveChanges());

            base.RemoveItem(index);
        }

        /// <summary>
        /// Sets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, TInterface item)
        {
            throw new NotSupportedException();
        }

        private static void ValidateItem(TInterface item)
        {
            if (!(item is TType))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidLinkType);
            }
        }

        private static string GetEntitySetName(Type type)
        {
            if (type == typeof(IAsset))
            {
                return AssetCollection.AssetSet;
            }

            if (type == typeof(IAssetFile))
            {
                return AssetFileCollection.FileSet;
            }

            if (type == typeof(ILocator))
            {
                return LocatorBaseCollection.LocatorSet;
            }

            if (type == typeof(IAccessPolicy))
            {
                return AccessPolicyBaseCollection.AccessPolicySet;
            }

            if (type == typeof(IJob))
            {
                return JobBaseCollection.JobSet;
            }

            if (type == typeof(IJobTemplate))
            {
                return JobTemplateBaseCollection.JobTemplateSet;
            }

            if (type == typeof(IContentKey))
            {
                return ContentKeyCollection.ContentKeySet;
            }

            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Not supported type: {0}.", type), "type");
        }
    }
}
