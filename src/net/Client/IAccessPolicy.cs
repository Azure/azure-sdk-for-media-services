//-----------------------------------------------------------------------
// <copyright file="IAccessPolicy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Defines an access policy to an <see cref="IAsset"/> in the system.
    /// </summary>    
    public partial interface IAccessPolicy
    {
        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Asynchronously deletes an access policy.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes an access policy.
        /// </summary>
        void Delete();
    }
}
