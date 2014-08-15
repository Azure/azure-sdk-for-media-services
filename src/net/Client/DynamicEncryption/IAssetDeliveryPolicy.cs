//-----------------------------------------------------------------------
// <copyright file="IDeliveryPolicy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption
{
    /// <summary>
    /// Describes the polices applied to assets for delivery.
    /// </summary>
    public interface IAssetDeliveryPolicy
    {
        /// <summary>
        /// Gets Unique identifier of the policy.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets name of the policy.
        /// An optional friendly name for the policy. It can used by the policy 
        /// creator to help remember what the policy represents or is used for. 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Delivery protocol.
        /// </summary>
        AssetDeliveryProtocol AssetDeliveryProtocol { get; set; }

        /// <summary>
        /// Policy type.
        /// </summary>
        AssetDeliveryPolicyType AssetDeliveryPolicyType { get; set; }

        /// <summary>
        /// Mapping from the way of obtaining a configuration to the configuration string.
        /// </summary>
        IDictionary<AssetDeliveryPolicyConfigurationKey, string> AssetDeliveryConfiguration { get; set; }

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<IAssetDeliveryPolicy> UpdateAsync();

        /// <summary>
        /// Updates this instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        void Delete();

        /// <summary>
        /// Asynchronously deletes this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task<IMediaDataServiceResponse> DeleteAsync();
    }
}
