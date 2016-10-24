//-----------------------------------------------------------------------
// <copyright file="BlobTransferClient.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a client to operate on Windows Azure Blobs.
    /// </summary>
    public class BlobTransferClient
    {
        private const int Timeout = 60;
        private const int Capacity = 100;
        private object lockobject = new object();
        private readonly TimeSpan _connectionTimeout = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _forceSharedAccessSignatureRetry;

        private readonly BlobTransferSpeedCalculator _downloadSpeedCalculator = new BlobTransferSpeedCalculator(Capacity);
        private readonly BlobTransferSpeedCalculator _uploadSpeedCalculator = new BlobTransferSpeedCalculator(Capacity);

        /// <summary>
        /// Occurs when upload/download operation has been completed or cancelled.
        /// </summary>
        public event EventHandler<BlobTransferCompleteEventArgs> TransferCompleted;

        /// <summary>
        /// Gets or sets the number of threads to use to for each blob transfer.
        /// /// </summary>
        /// <remarks>The default value is 10.</remarks>
        public int ParallelTransferThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the number of concurrent blob transfers allowed.
        /// </summary>
        /// <remarks>The default value is 2.</remarks>
        public int NumberOfConcurrentTransfers { get; set; }

        /// <summary>
        /// Occurs when file transfer progress changed.
        /// </summary>
        public event EventHandler<BlobTransferProgressChangedEventArgs> TransferProgressChanged;

        /// <summary>
        /// Constructs a <see cref="BlobTransferClient"/> object.
        /// </summary>
        public BlobTransferClient(TimeSpan forceSharedAccessSignatureRetry = default(TimeSpan))
        {
            _forceSharedAccessSignatureRetry = forceSharedAccessSignatureRetry;
            ParallelTransferThreadCount = ServicePointModifier.DefaultConnectionLimit();
            NumberOfConcurrentTransfers = ServicePointModifier.DefaultConnectionLimit();
        }

        /// <summary>
        /// Uploads file to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded. If blob has private write permissions then 
        /// appropriate sas url need to be passed</param>
        /// <param name="localFile">The full path of local file.</param>
        /// <param name="contentType">Content type of the blob</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string localFile,
            string contentType,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature = null)
        {
            return UploadBlob(
                url,
                localFile,
                fileEncryption,
                cancellationToken,
                null,
                retryPolicy,
                contentType,
                getSharedAccessSignature: getSharedAccessSignature)
            ;
        }

        /// <summary>
        /// Uploads file to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded.If blob has private write permissions then appropriate sas url need to be passed</param>
        /// <param name="localFile">The full path of local file.</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="client">The client which will be used to upload file. Use client if request need to be signed with client credentials. When upload performed using Sas url,
        /// then client can be null</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="contentType">Content type of the blob</param>
        /// <param name="subDirectory">Virtual subdirectory for this file in the blog container.</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string localFile,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            string contentType = null,
            string subDirectory = "",
            Func<string> getSharedAccessSignature = null
            )
        {
            var fs = new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return UploadBlob(
                    url,
                    localFile,
                    fs,
                    fileEncryption,
                    cancellationToken,
                    client,
                    retryPolicy,
                    contentType,
                    subDirectory,
                    getSharedAccessSignature)
                .ContinueWith(t => fs.Dispose());
            
        }

        /// <summary>
        /// Downloads the specified blob to the specified location.
        /// </summary>
        /// <param name="uri">The blob url  from which file  needs to be downloaded.If blob has private read permissions then appropriate sas url need to be passed</param>
        /// <param name="localFile">The full path where file will be saved </param>
        /// <param name="fileEncryption">The file encryption if file has been encrypted. Pass null if no encryption has been used</param>
        /// <param name="initializationVector">The initialization vector if encryption has been used.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <returns></returns>
        public virtual Task DownloadBlob(
            Uri uri,
            string localFile,
            FileEncryption fileEncryption,
            ulong initializationVector,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature = null
            )
        {
            return DownloadBlob(uri, localFile, fileEncryption, initializationVector, null, cancellationToken, retryPolicy, getSharedAccessSignature);
        }

        /// <summary>
        /// Downloads the specified blob to the specified location.
        /// </summary>
        /// <param name="uri">The blob url from which file a needs should be downloaded. If blob has private read permissions then an appropriate SAS url need to be passed.</param>
        /// <param name="localFile">The full path where file will be saved.</param>
        /// <param name="fileEncryption">The file encryption if file has been encrypted. Pass null if no encryption has been used.</param>
        /// <param name="initializationVector">The initialization vector if encryption has been used.</param>
        /// <param name="client">The azure client to access a blob.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the download operation.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <param name="start">Start pos to download</param>
        /// <param name="length">Number of bytes to download, -1 to download all.</param>
        /// <returns>A task that downloads the specified blob.</returns>
        public virtual Task DownloadBlob(
            Uri uri,
            string localFile,
            FileEncryption fileEncryption,
            ulong initializationVector,
            CloudBlobClient client,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature = null,
            long start = 0,
            long length = -1
            )
        {
            if (client != null && getSharedAccessSignature != null)
            {
                throw new InvalidOperationException("The arguments client and sharedAccessSignature cannot both be non-null");
            }

            if (start < 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Initial offset {0} to download a file must not be negative", start));
            }

            // To download the asset file as a whole or byte-range, the localFile should not be empty
            if (string.IsNullOrEmpty(localFile))
            {
                throw new ArgumentException("Parameter localFile must be set with a full path");
            }

            if (length < -1)
            {
                throw new ArgumentException("Parameter length must be equals or greater than -1");
            }

            if (_forceSharedAccessSignatureRetry != TimeSpan.Zero)
            {
                // The following sleep is for unit test purpose and we will force the shared access signature to expire and hit retry code path
                Thread.Sleep(_forceSharedAccessSignatureRetry);
            }

            BlobDownloader blobDownloader = new BlobDownloader(new MemoryManagerFactory());

            blobDownloader.TransferCompleted += (sender, args) =>
            {
                if (TransferCompleted != null)
                {
                    TransferCompleted(sender, args);
                }
                else if (args.Error != null)
                {
                    throw args.Error;
                }
            };

            blobDownloader.TransferProgressChanged += (sender, args) =>
            {
                if (TransferProgressChanged != null)
                {
                    TransferProgressChanged(sender, args);
                }
            };

            return blobDownloader.DownloadBlob(
                uri,
                localFile,
                fileEncryption,
                initializationVector,
                client,
                cancellationToken,
                retryPolicy,
                getSharedAccessSignature,
                start,
                length,
                parallelTransferThreadCount: ParallelTransferThreadCount,
                numberOfConcurrentTransfers: NumberOfConcurrentTransfers);
        }


        /// <summary>
        /// Uploads stream to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded. If blob has private write permissions then 
        /// appropriate sas url need to be passed</param>
        /// <param name="name">Name for the stream</param>
        /// <param name="stream">Stream to be uploaded</param>
        /// <param name="contentType">Content type of the blob</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string name,
            Stream stream,
            string contentType,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature = null)
        {
            return UploadBlob(
                url,
                name,
                stream,
                fileEncryption,
                cancellationToken,
                null,
                retryPolicy,
                contentType,
                getSharedAccessSignature: getSharedAccessSignature)
            ;
        }


        /// <summary>
        /// Uploads a stream to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded.If blob has private write permissions then appropriate sas url need to be passed</param>
        /// <param name="name">Name for the stream</param>
        /// <param name="stream">Stream to be uploaded</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="client">The client which will be used to upload file. Use client if request need to be signed with client credentials. When upload performed using Sas url,
        /// then client can be null</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <param name="contentType">Content type of the blob</param>
        /// <param name="subDirectory">Virtual subdirectory for this file in the blog container.</param>
        /// <param name="getSharedAccessSignature">A callback function which returns Sas signature for the file to be downloaded</param>
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string name,
            Stream stream,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            string contentType = null,
            string subDirectory = "",
            Func<string> getSharedAccessSignature = null)
        {

            if (_forceSharedAccessSignatureRetry != TimeSpan.Zero)
            {
                // The following sleep is for unit test purpose and we will force the shared access signature to expire and hit retry code path
                Thread.Sleep(_forceSharedAccessSignatureRetry);
            }

            BlobUploader blobuploader = new BlobUploader(new MemoryManagerFactory());

            blobuploader.TransferCompleted += (sender, args) =>
            {
                if (TransferCompleted != null)
                {
                    TransferCompleted(sender, args);
                }
                else if (args.Error != null)
                {
                    throw args.Error;
                }
            };

            blobuploader.TransferProgressChanged += (sender, args) =>
            {
                if (TransferProgressChanged != null)
                {
                    TransferProgressChanged(sender, args);
                }
            };

            return blobuploader.UploadBlob(
                url,
                name,
                stream,
                fileEncryption,
                cancellationToken,
                client,
                retryPolicy,
                contentType,
                subDirectory,
                getSharedAccessSignature: getSharedAccessSignature,
                parallelTransferThreadCount: ParallelTransferThreadCount,
                numberOfConcurrentTransfers: NumberOfConcurrentTransfers);
        }



    }

}
