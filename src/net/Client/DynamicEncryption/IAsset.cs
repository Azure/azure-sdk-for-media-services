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
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    public partial interface IAsset
    {

        /// <summary>
        /// Gets the delivery policies associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAssetDeliveryPolicy"/> associated with the Asset.</value>
        IList<IAssetDeliveryPolicy> DeliveryPolicies { get; }

        AssetEncryptionState GetEncryptionState(AssetDeliveryProtocol protocolsToCheck);

        AssetType AssetType { get; }

        bool IsStreamable { get; }

        bool SupportsDynamicEncryption { get; }
    }
}
