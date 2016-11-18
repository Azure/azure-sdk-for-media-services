//-----------------------------------------------------------------------
// <copyright path="IFileInfo.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this path except in compliance with the License.
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a file belonging to an Asset.
    /// </summary>
    /// <see cref="IAsset.AssetFiles"/>
    public partial interface IAssetFile
    {
        /// <summary>
        /// Occurs when a file download progresses.
        /// </summary>
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Occurs when the upload progress is updated.
        /// </summary>
        event EventHandler<UploadProgressChangedEventArgs> UploadProgressChanged;

        /// <summary>
        /// Gets the asset that this file belongs to.
        /// </summary>
        /// <value>The parent <see cref="IAsset"/>.</value>
        IAsset Asset { get; }

        /// <summary>
        /// Asynchronously downloads the represented file to the specified destination path.
        /// </summary>
        /// <param name="destinationPath">The path to download the file to.</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to download files.</param>
        /// <param name="locator">An asset <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task.
        /// </returns>
        Task DownloadAsync(string destinationPath, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken cancellationToken);
        

        /// <summary>
        /// Downloads the represented file to the specified destination path.
        /// </summary>
        /// <param name="destinationPath">The path to download the file to.</param>
        void Download(string destinationPath);

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Saves this instance.
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
        Task DeleteAsync();

        /// <summary>
        /// Uploads the file with given path asynchronously
        /// </summary>
        /// <param name="path">The path of a file to upload. The file name will be used as the asset file's name in Azure. </param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to upload files.</param>
        /// <param name="locator">A locator <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to use for canceling upload operation.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task UploadAsync(string path, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken token);

        /// <summary>
        /// Uploads a stream asynchronously
        /// </summary>
        /// <param name="stream">Stream to be uploaded. Must have the position set to the start of the data.</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to upload files.</param>
        /// <param name="locator">A locator <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to use for canceling upload operation.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task UploadAsync(Stream stream, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken token);

        /// <summary>
        /// Uploads the file with given path 
        /// </summary>
        /// <param name="path">The path of a file to upload.</param>
        void Upload(string path);

        /// <summary>
        /// Uploads a stream
        /// </summary>
        /// <param name="stream">Stream to be uploaded. Must have the position set to the start of the data.</param>
        void Upload(Stream stream);

    }
}
