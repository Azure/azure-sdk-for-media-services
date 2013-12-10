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
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    internal partial class AssetData : BaseEntity<IAsset>, IAsset
    {
        private IList<IAssetDeliveryPolicy> _deliveryPolicyCollection;

        /// <summary>
        /// Gets the delivery policies associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAssetDeliveryPolicy"/> associated with the Asset.</value>
        public List<AssetDeliveryPolicyData> DeliveryPolicies { get; set; }

        IList<IAssetDeliveryPolicy> IAsset.DeliveryPolicies
        {
            get
            {
                lock (_deliveryPolicyLocker)
                {
                    if ((this._deliveryPolicyCollection == null) && !string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(AssetCollection.AssetSet, this);
                        LoadProperty(dataContext, DeliveryPoliciesPropertyName);

                        this._deliveryPolicyCollection = new LinkCollection<IAssetDeliveryPolicy, AssetDeliveryPolicyData>(dataContext, this, DeliveryPoliciesPropertyName, this.DeliveryPolicies);
                    }

                    return this._deliveryPolicyCollection;
                }
            }
        }

        /// <summary>
        /// Invalidates the content key collection.
        /// </summary>
        internal void InvalidateDeliveryPoliciesCollection()
        {
            this.DeliveryPolicies.Clear();
            this._deliveryPolicyCollection = null;
        }
    }
}
