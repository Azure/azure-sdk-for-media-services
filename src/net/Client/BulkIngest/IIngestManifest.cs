//-----------------------------------------------------------------------
// <copyright file="IIngestManifest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents bulk ingest manifest
    /// </summary>
    public partial interface IIngestManifest
    {
        /// <summary>
        /// Gets the manifest assets.
        /// </summary>
        IngestManifestAssetCollection IngestManifestAssets { get; }

        /// <summary>
        /// Deletes manifest
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes manifest asyncroniously.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task DeleteAsync();

        /// <summary>
        /// Encrypts manifest files asyncroniously.
        /// </summary>
        /// <param name="outputPath">The output path where all encrypted files will be located.</param>
        /// <param name="overwriteExistingEncryptedFiles">if set to <c>true</c> method will override files in ouput folder.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task EncryptFilesAsync(string outputPath, bool overwriteExistingEncryptedFiles,CancellationToken cancellationToken);

        /// <summary>
        /// Encrypts all newly added manifest files asyncroniously. All files will be overriden if output folder has files with same names 
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        Task EncryptFilesAsync(string outputPath, CancellationToken token);

        /// <summary>
        /// Encrypts manifest files.
        /// </summary>
        /// <param name="outputPath">The output path where all encrypted files will be located.</param>
        /// <param name="overrideExistingEncryptedFiles">if set to <c>true</c> method will override files in ouput folder.</param>
        void EncryptFiles(string outputPath, bool overrideExistingEncryptedFiles);

        /// <summary>
        /// Encrypts all newly added manifest files. All files will be overriden if output folder has files with same names 
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        void EncryptFiles(string outputPath);


        /// <summary>
        /// Updates manifest.
        /// </summary>
        void Update();

        /// <summary>
        /// Updates manifest asyncroniously.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task UpdateAsync();

        /// <summary>
        /// Gets the manifest statistics.
        /// </summary>
        IIngestManifestStatistics Statistics { get; }

        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the <see cref="IIngestManifest"/> 
        /// </summary>
        IStorageAccount StorageAccount { get; } 
    }
}