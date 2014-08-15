//-----------------------------------------------------------------------
// <copyright file="ILocator.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents the application of an access policy to an asset.
    /// </summary>
    /// <remarks>A locator provides access to an asset using the <see cref="Path"/> property.</remarks>
    public partial interface ILocator
    {
        /// <summary>
        /// Gets the <see cref="IAccessPolicy"/> that defines this locator.
        /// </summary>
        IAccessPolicy AccessPolicy { get; }

        /// <summary>
        /// Gets the <see cref="IAsset"/> that this locator is attached to.
        /// </summary>
        IAsset Asset { get; }

        /// <summary>
        /// Asynchronously updates the expiration time of an Origin locator.
        /// </summary>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        Task UpdateAsync(DateTime expiryTime);

        /// <summary>
        /// Updates the expiration time of an Origin locator.
        /// </summary>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        void Update(DateTime expiryTime);

        /// <summary>
        /// Asynchronously updates the start time or expiration time of an Origin locator.
        /// </summary>
        /// <param name="startTime">The new start time for the origin locator.</param>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        Task UpdateAsync(DateTime? startTime, DateTime expiryTime);

        /// <summary>
        /// Updates the start time or expiration time of an Origin locator.
        /// </summary>
        /// <param name="startTime">The new start time for the origin locator.</param>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        void Update(DateTime? startTime, DateTime expiryTime);

        /// <summary>
        /// Asynchronously revokes the specified Locator, denying any access it provided.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes the specified Locator, revoking any access it provided.
        /// </summary>
        void Delete();
    }
}
