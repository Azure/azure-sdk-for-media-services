//-----------------------------------------------------------------------
// <copyright file="BlobDownloader.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class BlobDownloader : BlobTransferBase
    {
        public BlobDownloader(MemoryManagerFactory memoryManagerFactory) : base(memoryManagerFactory) 
        {
        }

        public Task DownloadBlob(
            Uri uri, 
            string localFile, 
            FileEncryption fileEncryption, 
            ulong initializationVector, 
            CloudBlobClient client, 
            CancellationToken cancellationToken, 
            IRetryPolicy retryPolicy, 
            Func<string> getSharedAccessSignature = null,
            long start = 0,
            long length = -1)
        {
            if (client != null && getSharedAccessSignature != null)
            {
                throw new InvalidOperationException("The arguments client and getSharedAccessSignature cannot both be non-null");
            }

            SetConnectionLimits(uri);

            Task task = Task.Factory.StartNew(() => DownloadFileFromBlob(uri, localFile, fileEncryption, initializationVector, client, cancellationToken, retryPolicy, getSharedAccessSignature, start:start, length:length));
            return task;
        }
        
        private void DownloadFileFromBlob(
            Uri uri,
            string localFile,
            FileEncryption fileEncryption,
            ulong initializationVector,
            CloudBlobClient client,
            CancellationToken cancellationToken,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature,
            bool shouldDoFileIO = true,
            long start = 0,
            long length = -1)
        {
            int numThreads = Environment.ProcessorCount * ParallelUploadDownloadThreadCountMultiplier;
            ManualResetEvent downloadCompletedSignal = new ManualResetEvent(false);
            BlobRequestOptions blobRequestOptions = new BlobRequestOptions { RetryPolicy = retryPolicy };
            
			CloudBlockBlob blob = GetCloudBlockBlob(uri, client, retryPolicy, getSharedAccessSignature);

            long initialOffset = start;
            long sizeToDownload = blob.Properties.Length;

            if (length != -1)
            {
                if (length > blob.Properties.Length)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Size {0} is beyond the Length of Blob {1}", length, blob.Properties.Length));
                }

                if (start + length > blob.Properties.Length)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Size {0} plus offset {1} is beyond the Length of Blob {2}", length, start, blob.Properties.Length));
                }

                sizeToDownload = length;
            }

            if (sizeToDownload == 0)
            {
                using (FileStream stream =
                    new FileStream(
                        localFile,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.Read
                        ))
                {
                }

                TaskCompletedCallback(
                    cancellationToken.IsCancellationRequested,
                    null,
                    BlobTransferType.Download,
                    localFile,
                    uri);
            }
            else
            {
                int blockSize = GetBlockSize(blob.Properties.Length);

                BlobTransferContext transferContext = new BlobTransferContext();

                transferContext.BlocksToTransfer = PrepareUploadDownloadQueue(sizeToDownload, blockSize, ref numThreads, initialOffset);

                transferContext.BlocksForFileIO = new ConcurrentDictionary<int, byte[]>();
                for (int i = 0; i < transferContext.BlocksToTransfer.Count(); i++)
                {
                    transferContext.BlocksForFileIO[i] = null;
                }
                transferContext.BlockSize = blockSize;
                transferContext.CancellationToken = cancellationToken;
                transferContext.Blob = blob;
                transferContext.BlobRequestOptions = blobRequestOptions;
                transferContext.Length = sizeToDownload;

                transferContext.LocalFilePath = localFile;
                transferContext.OnComplete = () => downloadCompletedSignal.Set();
                transferContext.MemoryManager = MemoryManagerFactory.GetMemoryManager(blockSize);
                transferContext.Client = client;
                transferContext.RetryPolicy = retryPolicy;
                transferContext.GetSharedAccessSignature = getSharedAccessSignature;
                transferContext.ShouldDoFileIO = shouldDoFileIO;
                transferContext.BufferStreams = new ConcurrentDictionary<byte[], MemoryStream>();
                transferContext.ClientRequestId = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                transferContext.Exceptions = new ConcurrentBag<Exception>();
                transferContext.FileEncryption = fileEncryption;
                transferContext.InitializationVector = initializationVector;
                transferContext.InitialOffset = start;

                using (FileStream stream = new FileStream(
                        transferContext.LocalFilePath,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.Read
                        ))
                {
                    stream.SetLength(sizeToDownload);
                     RunDownloadLoop(transferContext, stream, numThreads);
                }

                transferContext.MemoryManager.ReleaseUnusedBuffers();

                TaskCompletedCallback(
                    cancellationToken.IsCancellationRequested,
                    transferContext.Exceptions != null && transferContext.Exceptions.Count > 0 ? new AggregateException(transferContext.Exceptions) : null,
                    BlobTransferType.Download,
                    localFile,
                    uri);
            }
        }

        private void RunDownloadLoop(
            BlobTransferContext transferContext,
            FileStream stream,
            int numThreads)
        {
            SpinWait spinWait = new SpinWait();

            while (!transferContext.IsComplete && !transferContext.CancellationToken.IsCancellationRequested)
            {
                if (!transferContext.IsReadingOrWriting)
                {
                    DoSequentialWrite(transferContext, stream);
                }

                if (!transferContext.IsComplete && transferContext.NumInProgressUploadDownloads < numThreads)
                {
                    TryDownloadingBlocks(transferContext);
                }

                spinWait.SpinOnce();
            }

            while (transferContext.NumInProgressUploadDownloads > 0 || transferContext.IsReadingOrWriting)
            {
                spinWait.SpinOnce();
            }

            foreach (var memoryStream in transferContext.BufferStreams.Values)
            {
                memoryStream.Dispose();
            }

            transferContext.OnComplete();
        }

        private void BeginDownloadStream(
            BlobTransferContext transferContext,
            MemoryStream memoryStream,
            KeyValuePair<long, int> startAndLength, 
            byte[] streamBuffer)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            OperationContext operationContext = new OperationContext();
            operationContext.ClientRequestID = transferContext.ClientRequestId;

            Interlocked.Increment(ref transferContext.NumInProgressUploadDownloads);

            transferContext.Blob.BeginDownloadRangeToStream(
                memoryStream,
                startAndLength.Key,
                startAndLength.Value,
                AccessCondition.GenerateEmptyCondition(),
                transferContext.BlobRequestOptions,
                operationContext,
                ar =>
                {
                    SuccessfulOrRetryableResult wasWriteSuccessful = EndDownloadStream(transferContext, ar);
                    Interlocked.Decrement(ref transferContext.NumInProgressUploadDownloads);

                    if (wasWriteSuccessful.IsRetryable)
                    {
                        BeginDownloadStream(transferContext, memoryStream, startAndLength, streamBuffer);
                        return;
                    }

                    if (!wasWriteSuccessful.IsSuccessful)
                    {
                        transferContext.MemoryManager.ReleaseBuffer(streamBuffer);

                        return;
                    }

                    Interlocked.Add(ref transferContext.BytesBlobIOCompleted, startAndLength.Value);

                    TryDownloadingBlocks(transferContext);

                    if (transferContext.ShouldDoFileIO)
                    {
                        transferContext.BlocksForFileIO[(int) (startAndLength.Key/transferContext.BlockSize)] = streamBuffer;
                    }
                    else
                    {
                        transferContext.MemoryManager.ReleaseBuffer(streamBuffer);

                        if (transferContext.BytesBlobIOCompleted >= transferContext.Length)
                        {
                            transferContext.IsComplete = true;
                        }
                    }
                },
                null);
        }

        protected virtual SuccessfulOrRetryableResult EndDownloadStream(BlobTransferContext transferContext, IAsyncResult ar)
        {
            return IsActionSuccessfulOrRetryable(transferContext, () => transferContext.Blob.EndDownloadRangeToStream(ar));
        }

        private void DoSequentialWrite(
            BlobTransferContext transferContext,
            FileStream stream)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            byte[] buffer = null;

            if (transferContext.BlocksForFileIO.TryGetValue(transferContext.NextFileIOBlock, out buffer) && buffer != null)
            {
                transferContext.IsReadingOrWriting = true;
                long endOfRange = transferContext.Length + transferContext.InitialOffset;

                long beginFilePosition = (long)transferContext.NextFileIOBlock * transferContext.BlockSize + transferContext.InitialOffset;
                beginFilePosition = beginFilePosition > endOfRange
                    ? endOfRange
                    : beginFilePosition;
                          
                long nextBeginFilePosition = (transferContext.NextFileIOBlock + 1) * (long)transferContext.BlockSize + transferContext.InitialOffset;
                nextBeginFilePosition = nextBeginFilePosition > endOfRange
                                            ? endOfRange
                                            : nextBeginFilePosition;

                int bytesToWrite = (int)(nextBeginFilePosition - beginFilePosition);

                ApplyEncryptionTransform(transferContext.FileEncryption, transferContext.InitializationVector, beginFilePosition, buffer, bytesToWrite);

                stream.BeginWrite(
                    buffer,
                    0,
                    bytesToWrite,
                    result3 =>
                    {
                        SuccessfulOrRetryableResult wasWriteSuccessful = 
                            IsActionSuccessfulOrRetryable(transferContext, () => stream.EndWrite(result3));
                        
                        transferContext.MemoryManager.ReleaseBuffer(buffer);

                        if (!wasWriteSuccessful.IsSuccessful)
                        {
                            transferContext.IsReadingOrWriting = false;
                            return;
                        }
                        
                        transferContext.NextFileIOBlock++;

                        Interlocked.Add(ref transferContext.BytesWrittenOrReadToFile, bytesToWrite);

						InvokeProgressCallback(transferContext, transferContext.BytesWrittenOrReadToFile, bytesToWrite);

                        transferContext.IsReadingOrWriting = false;

                        if (transferContext.BytesWrittenOrReadToFile >= transferContext.Length)
                        {
                            transferContext.IsComplete = true;
                            transferContext.OnComplete();
                        }
                    },
                    null);
            }
        }

        private void TryDownloadingBlocks(BlobTransferContext transferContext)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            byte[] streamBuffer = transferContext.MemoryManager.RequireBuffer();

            if (streamBuffer == null)
            {
                return;
            }

            MemoryStream memoryStream = GetMemoryStream(transferContext.BufferStreams, streamBuffer);

            KeyValuePair<long, int> startAndLength;
            if (transferContext.BlocksToTransfer.TryDequeue(out startAndLength))
            {
                BeginDownloadStream(transferContext, memoryStream, startAndLength, streamBuffer);
            }
            else
            {
                transferContext.MemoryManager.ReleaseBuffer(streamBuffer);
            }
        }

        private CloudBlockBlob GetCloudBlockBlob(
            Uri uri,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            Func<String> getSharedAccessSignature)
        {
            CloudBlockBlob blob = null;

            if (getSharedAccessSignature != null)
            {
                string signature = getSharedAccessSignature();
                blob = new CloudBlockBlob(uri, new StorageCredentials(signature));
            }
            else
            {
                if (client != null)
                {
                    blob = new CloudBlockBlob(uri, client.Credentials);
                }
                else
                {
                    blob = new CloudBlockBlob(uri);
                }
            }

			BlobPolicyActivationWait(() => blob.FetchAttributes(options: new BlobRequestOptions() { RetryPolicy = retryPolicy }));
            
            return blob;
        }

        private static int GetBlockSize(long fileSize)
        {
            const long kb = 1024;
            const long mb = 1024 * kb;
            const long maxblocks = 50000;
            const long maxblocksize = 4 * mb;

            long blocksize = 4 * mb;
            long blockCount = ((int)Math.Floor((double)(fileSize / blocksize))) + 1;
            if (blockCount > maxblocks - 1 || (blocksize > maxblocksize))
            {
                throw new ArgumentException(StringTable.ErrorBlobTooBigToUpload);
            }

            return (int)blocksize;
        }
    }
}
