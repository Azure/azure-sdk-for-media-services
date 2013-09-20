//-----------------------------------------------------------------------
// <copyright file="IContentKeyAuthorizationPolicyOption.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using System.Data.Services.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// A list of different ways a client can be authorized to access the content key.
    /// </summary>
    public interface IContentKeyAuthorizationPolicyOption
    {
        /// <summary>
        /// Gets Unique identifier of the ContentKeyAuthorizationPolicyOption.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets name of ContentKeyAuthorizationPolicyOption.
        /// An optional friendly name for the policy. It can used by the policy 
        /// creator to help remember what the policy represents or is used for. 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the delivery method of the content key to the client.
        /// </summary>
        ContentKeyDeliveryType KeyDeliveryType { get; set; }

        /// <summary>
        /// Xml data, specific to the key delivery type that defines how the key is delivered to the client.
        /// </summary>
        string KeyDeliveryConfiguration { get; set; }        

        /// <summary>
        /// Gets or sets the restrictions.
        /// The requirements of each  restriction MUST be met in order to deliver the key using the key delivery data. 
        /// </summary>
        List<ContentKeyAuthorizationPolicyRestriction> Restrictions { get; set; }

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<IContentKeyAuthorizationPolicyOption> UpdateAsync();

        /// <summary>
        /// Updates this instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Deletes the ContentKeyAuthorizationPolicyOption.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the ContentKeyAuthorizationPolicyOption asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<DataServiceResponse> DeleteAsync();
    }
}
