//-----------------------------------------------------------------------
// <copyright file="IIngestManifestAsset.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an ingest manifest asset information describing what files need to be processed for a given asset
    /// </summary>
    public partial interface IIngestManifestAsset
    {
        /// <summary>
        /// Gets the manifest asset files.
        /// </summary> 
        IngestManifestFileCollection IngestManifestFiles { get; }

        /// <summary>
        /// Deletes the manifest asset and manifest asset files asynchronously.
        /// </summary>
        Task DeleteAsync();

        /// <summary>
        /// Deletes manifest asset and manifest asset files synchronously.
        /// </summary>
        void Delete();

        /// <summary>
        /// Gets the <see cref="IAsset"/> that this manifest asset is attached to.
        /// </summary>
        IAsset Asset { get; }
    }
}