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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.StorageClient.Protocol;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a client to operate on Windows Azure Blobs.
    /// </summary>
    public class BlobTransferClient
    {
        private const int Timeout = 60;
        private const int Capacity = 100;

        private readonly BlobTransferSpeedCalculator _downloadSpeedCalculator = new BlobTransferSpeedCalculator(Capacity);
        private readonly BlobTransferSpeedCalculator _uploadSpeedCalculator = new BlobTransferSpeedCalculator(Capacity);

        private object lockobject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferClient"/> class.
        /// </summary>
        public BlobTransferClient()
        {
            this.ParallelTransferThreadCount = 10;
            this.NumberOfConcurrentTransfers = 2;
        }

        /// <summary>
        /// Occurs when upload/download operation has been completed or cancelled.
        /// </summary>
        public event EventHandler<BlobTransferCompleteEventArgs> TransferCompleted;

        /// <summary>
        /// Occurs when file transfer progress changed.
        /// </summary>
        public event EventHandler<BlobTransferProgressChangedEventArgs> TransferProgressChanged;

        /// <summary>
        /// Gets or sets the number of threads to use to for each blob transfer.
        /// </summary>
        /// <remarks>The default value is 10.</remarks>
        public int ParallelTransferThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the number of concurrent blob transfers allowed.
        /// </summary>
        /// <remarks>The default value is 2.</remarks>
        public int NumberOfConcurrentTransfers { get; set; }

        /// <summary>
        /// Uploads file to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded.If blob has private write permissions then appropriate sas url need to be passed.</param>
        /// <param name="localFile">The full path of local file.</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UploadBlob(Uri url, string localFile, FileEncryption fileEncryption, CancellationToken cancellationToken, RetryPolicy retryPolicy)
        {
            return this.UploadBlob(url, localFile, fileEncryption, cancellationToken, null, retryPolicy);
        }

        /// <summary>
        /// Uploads file to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded.If blob has private write permissions then appropriate sas url need to be passed.</param>
        /// <param name="localFile">The full path of local file.</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="client">The client which will be used to upload file. Use client if request need to be signed with client credentials. When upload performed using Sas url, then client can be null.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UploadBlob(Uri url, string localFile, FileEncryption fileEncryption, CancellationToken cancellationToken, CloudBlobClient client, RetryPolicy retryPolicy)
        {
            this.SetMaxConnectionLimit(url);
            return Task.Factory.StartNew(() => this.UploadFileToBlob(cancellationToken, url, localFile, fileEncryption, client, retryPolicy), cancellationToken);
        }

        /// <summary>
        /// Downloads the specified blob to the specified location.
        /// </summary>
        /// <param name="uri">The blob url  from which file  needs to be downloaded.If blob has private read permissions then appropriate sas url need to be passed.</param>
        /// <param name="localFile">The full path where file will be saved.</param>
        /// <param name="fileEncryption">The file encryption if file has been encrypted. Pass null if no encryption has been used.</param>
        /// <param name="initializationVector">The initialization vector if encryption has been used.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DownloadBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CancellationToken cancellationToken, RetryPolicy retryPolicy)
        {
            return this.DownloadBlob(uri, localFile, fileEncryption, initializationVector, null, cancellationToken, retryPolicy);
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
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies.</param>
        /// <returns>A task that downloads the specified blob.</returns>
        public Task DownloadBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CloudBlobClient client, CancellationToken cancellationToken, RetryPolicy retryPolicy)
        {
            this.SetMaxConnectionLimit(uri);
            Task task = Task.Factory.StartNew(() => this.DownloadFileFromBlob(uri, localFile, fileEncryption, initializationVector, client, cancellationToken, retryPolicy));
            return task;
        }

        /// <summary>
        /// Occurrs when the progress of a blob transfer operation changes.
        /// </summary>
        /// <param name="e">An <see cref="BlobTransferProgressChangedEventArgs"/> that contains the progress information.</param>
        protected virtual void OnTaskProgressChanged(BlobTransferProgressChangedEventArgs e)
        {
            EventHandler<BlobTransferProgressChangedEventArgs> transferProgressChangedEvent = this.TransferProgressChanged;
            if (transferProgressChangedEvent != null)
            {
                transferProgressChangedEvent(this, e);
            }
        }

        private static HttpWebRequest BlobGetRequest(KeyValuePair<long, int> blockOffsetAndLength, CloudBlockBlob blob)
        {
            StorageCredentials credentials = blob.ServiceClient.Credentials;
            Uri transformedUri = new Uri(credentials.TransformUri(blob.Uri.ToString()));

            // Prepare the HttpWebRequest to download data from the chunk.
            HttpWebRequest blobGetRequest = BlobRequest.Get(transformedUri, Timeout, null, null);

            // Add header to specify the range.
            blobGetRequest.Headers.Add("x-ms-range", string.Format(CultureInfo.InvariantCulture, "bytes={0}-{1}", blockOffsetAndLength.Key, blockOffsetAndLength.Key + blockOffsetAndLength.Value - 1));

            if (credentials.CanSignRequest)
            {
                credentials.SignRequest(blobGetRequest);
            }

            return blobGetRequest;
        }

        private static CloudBlockBlob InitializeCloudBlockBlob(Uri uri, CloudBlobClient client, RetryPolicy retryPolicy)
        {
            CloudBlockBlob blob = null;
            blob = client != null ? new CloudBlockBlob(uri.AbsoluteUri, client) : new CloudBlockBlob(uri.AbsoluteUri);

            bool fetch = false;
            bool shouldRetry = true;
            TimeSpan delay;
            int retryCount = 0;
            StorageClientException lastException = null;

            while (!fetch && shouldRetry)
            {
                try
                {
                    blob.FetchAttributes(new BlobRequestOptions() { RetryPolicy = retryPolicy });
                    fetch = true;
                }
                catch (StorageClientException ex)
                {
                    retryCount++;
                    lastException = ex;
                    shouldRetry = retryPolicy()(retryCount, lastException, out delay);
                    Thread.Sleep(delay);
                }
            }

            if (!fetch)
            {
                throw lastException;
            }

            return blob;
        }

        private static ConcurrentQueue<KeyValuePair<long, int>> PrepareDownloadQueue(long blobLength, int bufferLength, ref int numThreads)
        {
            // Prepare a queue of chunks to be downloaded. Each queue item is a key-value pair 
            // where the 'key' is start offset in the blob and 'value' is the chunk length.
            var queue = new ConcurrentQueue<KeyValuePair<long, int>>();
            long offset = 0;
            while (blobLength > 0)
            {
                var chunkLength = (int)Math.Min(bufferLength, blobLength);
                queue.Enqueue(new KeyValuePair<long, int>(offset, chunkLength));
                offset += chunkLength;
                blobLength -= chunkLength;
            }

            if (queue.Count < numThreads)
            {
                numThreads = queue.Count;
            }

            return queue;
        }

        private static ConcurrentQueue<KeyValuePair<int, int>> PreapreUploadQueue(int maxBlockSize, long fileSize, ref int numThreads, out List<string> blockList)
        {
            var queue = new ConcurrentQueue<KeyValuePair<int, int>>();
            blockList = new List<string>();
            int blockId = 0;
            while (fileSize > 0)
            {
                int blockLength = (int)Math.Min(maxBlockSize, fileSize);
                string blockIdString = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "BlockId{0}", blockId.ToString("0000000", CultureInfo.InvariantCulture))));
                var kvp = new KeyValuePair<int, int>(blockId++, blockLength);
                queue.Enqueue(kvp);
                blockList.Add(blockIdString);
                fileSize -= blockLength;
            }

            if (queue.Count < numThreads)
            {
                numThreads = queue.Count;
            }

            return queue;
        }

        // Blob Upload Code
        // 200 GB max blob size
        // 50,000 max blocks
        // 4 MB max block size
        // Try to get close to 100k block size in order to offer good progress update response.
        private static int GetBlockSize(long fileSize)
        {
            const long KB = 1024;
            const long MB = 1024 * KB;
            const long MaxBlocks = 50000;
            const long MaxBlockSize = 4 * MB;

            long blocksize = 1 * MB;
            long blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            while (blockCount > MaxBlocks - 1)
            {
                blocksize += 1 * MB;
                blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            }

            if (blocksize > MaxBlockSize)
            {
                throw new ArgumentException(StringTable.ErrorBlobTooBigToUpload);
            }

            return (int)blocksize;
        }

        private static string GetMd5HashFromStream(byte[] data)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] blockHash = md5.ComputeHash(data);
                return Convert.ToBase64String(blockHash, 0, 16);
            }
        }

        private void SetMaxConnectionLimit(Uri url)
        {
            var servicePoint = ServicePointManager.FindServicePoint(url);
            if (servicePoint != null)
            {
                servicePoint.ConnectionLimit = this.NumberOfConcurrentTransfers * this.ParallelTransferThreadCount;
            }
        }

        private void DownloadFileFromBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CloudBlobClient client, CancellationToken cancellationToken, RetryPolicy retryPolicy)
        {
            int exceptionCount = 0;
            int numThreads = this.ParallelTransferThreadCount;
            Exception lastException = null;
            long bytesDownloaded = 0;

            CloudBlockBlob blob = InitializeCloudBlockBlob(uri, client, retryPolicy);

            long blobLength = blob.Properties.Length;
            int bufferLength = GetBlockSize(blobLength);
            var queue = PrepareDownloadQueue(blobLength, bufferLength, ref numThreads);

            if (cancellationToken.IsCancellationRequested)
            {
                this.OnTaskCompleted(null, true, localFile, uri, BlobTransferType.Download);
                cancellationToken.ThrowIfCancellationRequested();
            }

            using (var fs = new FileStream(localFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                var tasks = new List<Task>();

                Action action =
                    () =>
                    {
                        KeyValuePair<long, int> blockOffsetAndLength;
                        int exceptionPerThread = 0;

                        // A buffer to fill per read request.
                        var buffer = new byte[bufferLength];

                        while (queue.TryDequeue(out blockOffsetAndLength))
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            try
                            {
                                var blobGetRequest = BlobGetRequest(blockOffsetAndLength, blob);

                                using (var response = blobGetRequest.GetResponse() as HttpWebResponse)
                                {
                                    if (response != null)
                                    {
                                        ReadResponseStream(fileEncryption, initializationVector, fs, buffer, response, blockOffsetAndLength, ref bytesDownloaded);
                                        var progress = (int)((double)bytesDownloaded / blob.Attributes.Properties.Length * 100);

                                        // Raise the progress changed event.
                                        var eArgs = new BlobTransferProgressChangedEventArgs(bytesDownloaded, blockOffsetAndLength.Value, blob.Attributes.Properties.Length, progress, this._downloadSpeedCalculator.UpdateCountersAndCalculateSpeed(bytesDownloaded), uri, localFile, null);
                                        OnTaskProgressChanged(eArgs);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                bool ok = ex is WebException || ex is ObjectDisposedException;
                                if (!ok)
                                {
                                    throw;
                                }

                                TimeSpan tm;
                                exceptionCount++;
                                exceptionPerThread++;
                                if (!retryPolicy()(exceptionPerThread, ex, out tm))
                                {
                                    lastException = new AggregateException(string.Format(CultureInfo.InvariantCulture, "Received {0} exceptions while downloading. Canceling download.", exceptionCount), ex);
                                    break;
                                }

                                Thread.Sleep(tm);

                                // Add block back to queue.
                                queue.Enqueue(blockOffsetAndLength);
                            }
                        }
                    };

                // Launch threads to download chunks.
                for (int idxThread = 0; idxThread < numThreads; idxThread++)
                {
                    tasks.Add(Task.Factory.StartNew(
                        action,
                        cancellationToken,
                        System.Threading.Tasks.TaskCreationOptions.AttachedToParent,
                        TaskScheduler.Current));
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    this.OnTaskCompleted(lastException, true, localFile, uri, BlobTransferType.Download);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                Task.WaitAll(tasks.ToArray(), cancellationToken);
                this.OnTaskCompleted(lastException, cancellationToken.IsCancellationRequested, localFile, uri, BlobTransferType.Download);
            }
        }

        private long ReadResponseStream(FileEncryption fileEncryption, ulong initializationVector, FileStream fs, byte[] buffer, HttpWebResponse response, KeyValuePair<long, int> blockOffsetAndLength, ref long bytesDownloaded)
        {
            using (Stream stream = response.GetResponseStream())
            {
                int offsetInChunk = 0;
                int remaining = blockOffsetAndLength.Value;
                while (remaining > 0)
                {
                    int read = stream.Read(buffer, offsetInChunk, remaining);
                    lock (this.lockobject)
                    {
                        fs.Position = blockOffsetAndLength.Key + offsetInChunk;
                        if (fileEncryption != null)
                        {
                            lock (fileEncryption)
                            {
                                using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(initializationVector, blockOffsetAndLength.Key + offsetInChunk))
                                {
                                    encryptor.TransformBlock(inputBuffer: buffer, inputOffset: offsetInChunk, inputCount: read, outputBuffer: buffer, outputOffset: offsetInChunk);
                                }
                            }
                        }

                        fs.Write(buffer, offsetInChunk, read);
                    }

                    offsetInChunk += read;
                    remaining -= read;
                    Interlocked.Add(ref bytesDownloaded, read);
                }
            }

            return bytesDownloaded;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Following active collection pattern")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Caller expects exception type.")]
        private void UploadFileToBlob(CancellationToken cancellationToken, Uri uri, string localFile, FileEncryption fileEncryption, CloudBlobClient client, RetryPolicy retryPolicy)
        {
            // Attempt to open the file first so that we throw an exception before getting into the async work.
            using (new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
            }

            Exception lastException = null;

            // Stats from azurescope show 10 to be an optimal number of transfer threads.
            int numThreads = this.ParallelTransferThreadCount;
            var file = new FileInfo(localFile);
            long fileSize = file.Length;

            int maxBlockSize = GetBlockSize(fileSize);

            // Prepare a queue of blocks to be uploaded. Each queue item is a key-value pair where
            // the 'key' is block id and 'value' is the block length.
            List<string> blockList;
            var queue = PreapreUploadQueue(maxBlockSize, fileSize, ref numThreads, out blockList);
            int exceptionCount = 0;

            CloudBlobContainer blobContainer = client != null ? new CloudBlobContainer(uri.AbsoluteUri, client) : new CloudBlobContainer(uri.AbsoluteUri);
            CloudBlockBlob blob = blobContainer.GetBlobReference(Path.GetFileName(localFile)).ToBlockBlob;

            var options = new BlobRequestOptions
            {
                RetryPolicy = retryPolicy,
                Timeout = TimeSpan.FromSeconds(90)
            };

            int currentRetryCount = -1;
            TimeSpan delay;
            for (; ; )
            {
                try
                {
                    currentRetryCount++;
                    blob.DeleteIfExists(options);
                    break;
                }
                catch (StorageException ex)
                {

                    ShouldRetry shouldRetry = retryPolicy();

                    if (!shouldRetry(currentRetryCount, ex, out delay))
                    {
                        throw;
                    }

                    Thread.Sleep((int) delay.TotalMilliseconds);
                }
            }

            blob.Properties.ContentType = AssetFileData.GetMimeType(localFile);

            if (cancellationToken.IsCancellationRequested)
            {
                this.OnTaskCompleted(null, true, localFile, uri, BlobTransferType.Upload);
                cancellationToken.ThrowIfCancellationRequested();
            }

           

            // Launch threads to upload blocks.
            IList<Task> tasks = new List<Task>();
            long bytesSent = 0;
            Action action =
                () =>
                {
                    List<Exception> exceptions = new List<Exception>();
                    if (queue.Count > 0)
                    {
                        using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
                        {
                            KeyValuePair<int, int> blockIdAndLength;
                            while (queue.TryDequeue(out blockIdAndLength))
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                try
                                {
                                    var buffer = new byte[blockIdAndLength.Value];
                                    var binaryReader = new BinaryReader(fs);

                                    // Move the file system reader to the proper position.
                                    fs.Seek(blockIdAndLength.Key * (long)maxBlockSize, SeekOrigin.Begin);
                                    int readSize = binaryReader.Read(buffer, 0, blockIdAndLength.Value);

                                    if (fileEncryption != null)
                                    {
                                        lock (fileEncryption)
                                        {
                                            using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(file.Name, blockIdAndLength.Key * (long)maxBlockSize))
                                            {
                                                encryptor.TransformBlock(buffer, 0, readSize, buffer, 0);
                                            }
                                        }
                                    }

                                    using (var ms = new MemoryStream(buffer, 0, blockIdAndLength.Value))
                                    {
                                        string blockIdString = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "BlockId{0}", blockIdAndLength.Key.ToString("0000000", CultureInfo.InvariantCulture))));
                                        string blockHash = GetMd5HashFromStream(buffer);
                                        if (blob != null)
                                        {
                                            blob.PutBlock(blockIdString, ms, blockHash, options);
                                        }
                                    }

                                    Interlocked.Add(ref bytesSent, blockIdAndLength.Value);
                                    var progress = (int)((double)bytesSent / file.Length * 100);
                                    var eArgs = new BlobTransferProgressChangedEventArgs(bytesSent, blockIdAndLength.Value, file.Length, progress, this._uploadSpeedCalculator.UpdateCountersAndCalculateSpeed(bytesSent), uri, localFile, null);

                                    this.OnTaskProgressChanged(eArgs);
                                }
                                catch (StorageException ex)
                                {
                                    TimeSpan tm;
                                    exceptionCount++;
                                    exceptions.Add(ex);
                                    if (!retryPolicy()(exceptions.Count, ex, out tm))
                                    {
                                        lastException = new AggregateException(string.Format(CultureInfo.InvariantCulture, "Received {0} exceptions while uploading. Canceling upload.", exceptions.Count), exceptions);
                                        throw lastException;
                                    }

                                    Thread.Sleep(tm);
                                    queue.Enqueue(blockIdAndLength);
                                }
                            }
                        }
                    }
                };

            for (int idxThread = 0; idxThread < numThreads; idxThread++)
            {
                tasks.Add(Task.Factory.StartNew(
                       action,
                       cancellationToken,
                       System.Threading.Tasks.TaskCreationOptions.AttachedToParent,
                       TaskScheduler.Current));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                this.OnTaskCompleted(lastException, true, localFile, uri, BlobTransferType.Upload);
                cancellationToken.ThrowIfCancellationRequested();
            }

            Task.Factory
                .ContinueWhenAll(
                    tasks.ToArray(),
                    result =>
                    {
                        if (result.Any(t => t.IsFaulted))
                        {
                            return;
                        }

                        blob.PutBlockList(blockList, options);
                    },
                    TaskContinuationOptions.None)
                .Wait(cancellationToken);

            this.OnTaskCompleted(lastException, cancellationToken.IsCancellationRequested, localFile, uri, BlobTransferType.Upload);
        }

        private void OnTaskCompleted(Exception ex, bool isCancelled, string localFile, Uri uri, BlobTransferType transferType)
        {
            EventHandler<BlobTransferCompleteEventArgs> transferCompletedEvent = this.TransferCompleted;
            if (transferCompletedEvent != null)
            {
                transferCompletedEvent(this, new BlobTransferCompleteEventArgs(ex, isCancelled, null, localFile, uri, transferType));
            }
        }
    }
}
