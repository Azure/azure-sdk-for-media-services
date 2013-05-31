//-----------------------------------------------------------------------
// <copyright file="AssetFileCollectionBase.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides a base class for all <see cref="IAssetFile"/> collections.
    /// </summary>
    public abstract class AssetFileBaseCollection : BaseCollection<IAssetFile>
    {
        /// <summary>
        /// Creates the <see cref="IAssetFile"/>
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <returns><see cref="IAssetFile"/></returns>
        public abstract IAssetFile Create(string name);
       
        /// <summary>
        /// Creates the <see cref="IAssetFile"/> asyncronously
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="cancellation"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/> of type <see cref="IAssetFile"/></returns>
        public abstract Task<IAssetFile> CreateAsync(string name, CancellationToken cancellation);
    }
}
