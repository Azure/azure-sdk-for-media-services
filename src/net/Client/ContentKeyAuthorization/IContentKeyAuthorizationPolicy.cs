//-----------------------------------------------------------------------
// <copyright file="IContentKeyAuthorizationPolicy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    public interface IContentKeyAuthorizationPolicy
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<IContentKeyAuthorizationPolicy> UpdateAsync();

        /// <summary>
        /// Updates this instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Deletes the IContentKeyAuthorizationPolicy.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the IContentKeyAuthorizationPolicy asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<IMediaDataServiceResponse> DeleteAsync();

        /// <summary>
        /// Gets a collection of <see cref="IContentKeyAuthorizationPolicyOption"/> contained by the <see cref="IContentKeyAuthorizationPolicy"/>
        /// </summary>
        /// <value>A collection of files contained by the Asset.</value>
        IList<IContentKeyAuthorizationPolicyOption> Options { get; }
    }
}