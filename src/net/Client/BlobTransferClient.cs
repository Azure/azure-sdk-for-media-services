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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Auth.Protocol;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using Microsoft.WindowsAzure.Storage.Core.Auth;
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
            ParallelTransferThreadCount = 10;
            NumberOfConcurrentTransfers = 2;
        }



        /// <summary>
        /// Uploads file to a blob storage.
        /// </summary>
        /// <param name="url">The URL where file needs to be uploaded.If blob has private write permissions then appropriate sas url need to be passed</param>
        /// <param name="localFile">The full path of local file.</param>
        /// <param name="contentType">Content type of the blob</param>
        /// <param name="fileEncryption">The file encryption if file needs to be stored encrypted. Pass null if no encryption required</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="retryPolicy">The RetryPolicy delegate returns a ShouldRetry delegate, which can be used to implement a custom retry policy.RetryPolicies class can bee used to get default policies</param>
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string localFile,
            string contentType,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy)
        {
            return UploadBlob(url, localFile, fileEncryption, cancellationToken, null, retryPolicy, contentType);
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
        /// <returns></returns>
        public virtual Task UploadBlob(
            Uri url,
            string localFile,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            string contentType = null,
            string subDirectory = "")
        {
            SetMaxConnectionLimit(url);
            return Task.Factory.StartNew(
                () => UploadFileToBlob(cancellationToken, url, localFile, contentType, subDirectory, fileEncryption, client, retryPolicy),
                cancellationToken);
        }

        private void SetMaxConnectionLimit(Uri url)
        {
            var servicePoint = ServicePointManager.FindServicePoint(url);
            if (servicePoint != null)
            {
                servicePoint.ConnectionLimit = NumberOfConcurrentTransfers * ParallelTransferThreadCount;
            }
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
        /// <returns></returns>
        public virtual Task DownloadBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CancellationToken cancellationToken, IRetryPolicy retryPolicy)
        {
            return DownloadBlob(uri, localFile, fileEncryption, initializationVector, null, cancellationToken, retryPolicy);
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
        /// <returns>A task that downloads the specified blob.</returns>
        public virtual Task DownloadBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CloudBlobClient client, CancellationToken cancellationToken, IRetryPolicy retryPolicy)
        {
            SetMaxConnectionLimit(uri);
            Task task = Task.Factory.StartNew(() => DownloadFileFromBlob(uri, localFile, fileEncryption, initializationVector, client, cancellationToken, retryPolicy));
            return task;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void DownloadFileFromBlob(Uri uri, string localFile, FileEncryption fileEncryption, ulong initializationVector, CloudBlobClient client, CancellationToken cancellationToken, IRetryPolicy retryPolicy)
        {
            int numThreads = ParallelTransferThreadCount;
            List<Exception> exceptions = new List<Exception>();
            AggregateException aggregateException = null;
            long bytesDownloaded = 0;

            CloudBlockBlob blob = InitializeCloudBlockBlob(uri, client, retryPolicy);

            long blobLength = blob.Properties.Length;
            int bufferLength = GetBlockSize(blobLength);
            var queue = PrepareDownloadQueue(blobLength, bufferLength, ref numThreads);

            if (cancellationToken.IsCancellationRequested)
            {
                TaskCompletedCallback(true, null, BlobTransferType.Download, localFile, uri);
                cancellationToken.ThrowIfCancellationRequested();
            }

            using (var fs = new FileStream(localFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                var tasks = new List<Task>();

                Action action = () =>
                {
                    KeyValuePair<long, int> blockOffsetAndLength;
                    int exceptionPerThread = 0;
                    // A buffer to fill per read request.
                    var buffer = new byte[bufferLength];

                    if (_forceSharedAccessSignatureRetry != TimeSpan.Zero)
                    {
                        // The following sleep is for unit test purpose and we will force the shared access signature to expire and hit retry code path
                        Thread.Sleep(_forceSharedAccessSignatureRetry);
                    }

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
                                    var progress = (int)((double)bytesDownloaded / blob.Properties.Length * 100);
                                    // raise the progress changed event
                                    var eArgs = new BlobTransferProgressChangedEventArgs(bytesDownloaded, blockOffsetAndLength.Value, blob.Properties.Length, progress, _downloadSpeedCalculator.UpdateCountersAndCalculateSpeed(bytesDownloaded), uri, localFile, null);
                                    OnTaskProgressChanged(eArgs);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var webEx = ex as WebException;
                            bool ok = (webEx != null) || ex is ObjectDisposedException;
                            if (!ok)
                            {
                                throw;
                            }

                            if (webEx != null)
                            {
                                if (webEx.Response is HttpWebResponse)
                                {
                                    var httpex = (HttpWebResponse)webEx.Response;
                                    if (httpex.StatusCode == HttpStatusCode.Forbidden)
                                    {
                                       blob = InitializeCloudBlockBlob(uri, null, retryPolicy);
                                    }
                                }
                            }

                            TimeSpan tm;
                            exceptionPerThread++;
                            exceptions.Add(ex);
                            if (!retryPolicy.ShouldRetry(exceptionPerThread, 0, ex, out tm, new OperationContext()))
                            {
                                aggregateException = new AggregateException(String.Format(CultureInfo.InvariantCulture, "Received {0} exceptions while downloading. Canceling download.", exceptions.Count), exceptions);
                                throw aggregateException;
                            }

                            Thread.Sleep(tm);
                            // Add block back to queue
                            queue.Enqueue(blockOffsetAndLength);
                        }
                    }
                };


                // Launch threads to download chunks.
                for (int idxThread = 0; idxThread < numThreads; idxThread++)
                {
                    tasks.Add(Task.Factory.StartNew(action));
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    TaskCompletedCallback(true, aggregateException, BlobTransferType.Download, localFile, uri);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                Task.WaitAll(tasks.ToArray(), cancellationToken);
                TaskCompletedCallback(cancellationToken.IsCancellationRequested, aggregateException, BlobTransferType.Download, localFile, uri);
            }
        }



        private long ReadResponseStream(FileEncryption fileEncryption, ulong initializationVector, FileStream fs, byte[] buffer, HttpWebResponse response,
                                               KeyValuePair<long, int> blockOffsetAndLength, ref long bytesDownloaded)
        {
            using (Stream stream = response.GetResponseStream())
            {
                int offsetInChunk = 0;
                int remaining = blockOffsetAndLength.Value;
                while (remaining > 0)
                {
                    int read = stream.Read(buffer, offsetInChunk, remaining);
                    lock (lockobject)
                    {
                        fs.Position = blockOffsetAndLength.Key + offsetInChunk;
                        if (fileEncryption != null)
                        {
                            lock (fileEncryption)
                            {
                                using (
                                    FileEncryptionTransform encryptor = fileEncryption.GetTransform(initializationVector, blockOffsetAndLength.Key + offsetInChunk)
                                    )
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

        private static HttpWebRequest BlobGetRequest(KeyValuePair<long, int> blockOffsetAndLength, CloudBlockBlob blob)
        {
            StorageCredentials credentials = blob.ServiceClient.Credentials;
            var transformedUri = credentials.TransformUri(blob.Uri);

            // Prepare the HttpWebRequest to download data from the chunk.
            HttpWebRequest blobGetRequest = BlobHttpWebRequestFactory.Get(
                transformedUri,
                Timeout,
                snapshot: null,
                offset: blockOffsetAndLength.Key,
                count: blockOffsetAndLength.Value,
                rangeContentMD5: false,
                accessCondition: AccessCondition.GenerateEmptyCondition(),
                operationContext: new OperationContext());

            if (credentials.IsSharedKey)
            {
                IAuthenticationHandler authenticationHandler = new SharedKeyAuthenticationHandler(
                            SharedKeyCanonicalizer.Instance,
                            credentials,
                            credentials.AccountName);
                authenticationHandler.SignRequest(blobGetRequest, new OperationContext());
            }
            return blobGetRequest;
        }

        private static CloudBlockBlob InitializeCloudBlockBlob(Uri uri, CloudBlobClient client, IRetryPolicy retryPolicy)
        {
            CloudBlockBlob blob = null;


            if (client != null)
            {
                blob = new CloudBlockBlob(uri, client.Credentials);
            }
            else
            {
                blob = new CloudBlockBlob(uri);
            }


            bool fetch = false;
            bool shouldRetry = true;
            TimeSpan delay;
            int retryCount = 0;
            StorageException lastException = null;

            while (!fetch && shouldRetry)
            {
                try
                {
                    blob.FetchAttributes(options: new BlobRequestOptions() { RetryPolicy = retryPolicy });
                    fetch = true;
                }
                catch (StorageException ex)
                {
                    retryCount++;
                    lastException = ex;
                    shouldRetry = retryPolicy.ShouldRetry(retryCount, ex.RequestInformation.HttpStatusCode, lastException, out delay, new OperationContext());
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

        private static CloudBlockBlob GetCloudBlockBlob(Uri uri, CloudBlobClient client, string subFolder, string localFile, string contentType)
        {
            CloudBlobContainer blobContainer = null;
            CloudBlockBlob blob = null;
            if (client != null)
            {
                blobContainer = new CloudBlobContainer(uri, client.Credentials);
            }
            else
            {
                    blobContainer = new CloudBlobContainer(uri);
            }

            string blobFileName = Path.Combine(subFolder, Path.GetFileName(localFile));
            blob = blobContainer.GetBlockBlobReference(blobFileName);
            blob.Properties.ContentType = contentType;
            return blob;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void UploadFileToBlob(
            CancellationToken cancellationToken,
            Uri uri,
            string localFile,
            string contentType,
            string subFolder,
            FileEncryption fileEncryption,
            CloudBlobClient client,
            IRetryPolicy retryPolicy)
        {
            //attempt to open the file first so that we throw an exception before getting into the async work
            using (new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
            }

            Exception lastException = null;
            CloudBlockBlob blob = null;
            // stats from azurescope show 10 to be an optimal number of transfer threads
            int numThreads = ParallelTransferThreadCount;
            var file = new FileInfo(localFile);
            long fileSize = file.Length;

            int maxBlockSize = GetBlockSize(fileSize);

            // Prepare a queue of blocks to be uploaded. Each queue item is a key-value pair where
            // the 'key' is block id and 'value' is the block length.
            List<string> blockList;
            var queue = PreapreUploadQueue(maxBlockSize, fileSize, ref numThreads, out blockList);
            int exceptionCount = 0;

            blob = GetCloudBlockBlob(uri, client, subFolder, localFile, contentType);
            blob.DeleteIfExists(options: new BlobRequestOptions() { RetryPolicy = retryPolicy });

            if (cancellationToken.IsCancellationRequested)
            {
                TaskCompletedCallback(true, null, BlobTransferType.Upload, localFile, uri);
                cancellationToken.ThrowIfCancellationRequested();
            }

            var options = new BlobRequestOptions
            {
                RetryPolicy = retryPolicy,
                ServerTimeout = TimeSpan.FromSeconds(90)
            };

            // Launch threads to upload blocks.
            var tasks = new List<Task>();
            long bytesSent = 0;
            Action action = () =>
            {

                List<Exception> exceptions = new List<Exception>();

                if (_forceSharedAccessSignatureRetry != TimeSpan.Zero)
                {
                    Thread.Sleep(_forceSharedAccessSignatureRetry);
                }

                if (queue.Count > 0)
                {
                    FileStream fs = null;

                    try
                    {
                        fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

                        KeyValuePair<int, int> blockIdAndLength;
                        while (queue.TryDequeue(out blockIdAndLength))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            try
                            {
                                var buffer = new byte[blockIdAndLength.Value];
                                var binaryReader = new BinaryReader(fs);

                                // move the file system reader to the proper position
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
                                    if (blob != null) blob.PutBlock(blockIdString, ms, blockHash, options: options);
                                }

                                Interlocked.Add(ref bytesSent, blockIdAndLength.Value);
                                var progress = (int)((double)bytesSent / file.Length * 100);
                                var eArgs = new BlobTransferProgressChangedEventArgs(bytesSent, blockIdAndLength.Value, file.Length, progress, _uploadSpeedCalculator.UpdateCountersAndCalculateSpeed(bytesSent), uri, localFile, null);
                                OnTaskProgressChanged(eArgs);
                            }
                            catch (StorageException ex)
                            {
                                TimeSpan tm;
                                exceptionCount++;
                                exceptions.Add(ex);
                                if (!retryPolicy.ShouldRetry(exceptions.Count, ex.RequestInformation.HttpStatusCode, ex, out tm, new OperationContext()))
                                {
                                    lastException = new AggregateException(String.Format(CultureInfo.InvariantCulture, "Received {0} exceptions while uploading. Canceling upload.", exceptions.Count), exceptions);
                                    throw lastException;
                                }
                                Thread.Sleep(tm);

                                queue.Enqueue(blockIdAndLength);
                            }
                            catch (IOException ex)
                            {
                                TimeSpan tm;
                                exceptionCount++;
                                exceptions.Add(ex);
                                if (!retryPolicy.ShouldRetry(exceptions.Count, 0, ex, out tm, new OperationContext()))
                                {
                                    lastException = new AggregateException(String.Format(CultureInfo.InvariantCulture, "Received {0} exceptions while reading file {1} @ location {2} to be uploaded. Canceling upload.",
                                        exceptions.Count, file.Name, blockIdAndLength.Key * (long)maxBlockSize), exceptions);
                                    throw lastException;
                                }

                                // dispose existing file stream
                                if (fs != null)
                                {
                                    fs.Close();
                                }

                                Thread.Sleep(tm);

                                // try to reopen the file stream again
                                fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                                queue.Enqueue(blockIdAndLength);
                            }
                        }
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
            };

            for (int idxThread = 0; idxThread < numThreads; idxThread++)
            {
                tasks.Add(Task.Factory.StartNew(
                    action,
                    cancellationToken,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Current));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                TaskCompletedCallback(true, lastException, BlobTransferType.Upload, localFile, uri);
                cancellationToken.ThrowIfCancellationRequested();
            }

            Task.Factory.ContinueWhenAll(tasks.ToArray(), (Task[] result) =>
            {
                if (result.Any(t => t.IsFaulted))
                {
                    return;
                }
                blob.PutBlockList(blockList, options: options);
               
            }, TaskContinuationOptions.None).Wait(cancellationToken);

            TaskCompletedCallback(cancellationToken.IsCancellationRequested, lastException, BlobTransferType.Upload, localFile, uri);
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

        private void TaskCompletedCallback(bool isCancelled, Exception ex, BlobTransferType transferType, string localFile, Uri url)
        {
            if (TransferCompleted != null)
            {
                TransferCompleted(this, new BlobTransferCompleteEventArgs(ex, isCancelled, null, localFile, url, transferType));
            }
        }

        /// <summary>
        /// Occurrs when the progress of a blob transfer operation changes.
        /// </summary>
        /// <param name="e">An <see cref="BlobTransferProgressChangedEventArgs"/> that contains the progress information.</param>
        protected virtual void OnTaskProgressChanged(BlobTransferProgressChangedEventArgs e)
        {
            if (TransferProgressChanged != null) TransferProgressChanged(this, e);
        }

        // Blob Upload Code
        // 200 GB max blob size
        // 50,000 max blocks
        // 4 MB max block size
        // Try to get close to 100k block size in order to offer good progress update response.
        private static int GetBlockSize(long fileSize)
        {
            const long kb = 1024;
            const long mb = 1024 * kb;
            const long maxblocks = 50000;
            const long maxblocksize = 4 * mb;

            long blocksize = 1 * mb;
            long blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            while (blockCount > maxblocks - 1)
            {
                blocksize += 1 * mb;
                blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            }

            if (blocksize > maxblocksize)
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
    }
}
