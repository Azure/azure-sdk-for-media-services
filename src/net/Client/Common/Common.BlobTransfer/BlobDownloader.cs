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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class BlobDownloader : BlobTransferBase
    {
        public BlobDownloader(MemoryManagerFactory memoryManagerFactory)
            : base(memoryManagerFactory)
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
            long length = -1,
            int parallelTransferThreadCount = 10,
            int numberOfConcurrentTransfers = default(int))
        {
            if (client != null && getSharedAccessSignature != null)
            {
                throw new InvalidOperationException("The arguments client and getSharedAccessSignature cannot both be non-null");
            }

            SetConnectionLimits(uri, numberOfConcurrentTransfers);

            Task task =
                Task.Factory.StartNew(
                    () =>
                        DownloadFileFromBlob(uri, localFile, fileEncryption, initializationVector, client,
                            cancellationToken, retryPolicy, getSharedAccessSignature, start: start, length: length,
                            parallelTransferThreadCount: parallelTransferThreadCount));
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
            long length = -1,
            int parallelTransferThreadCount = 10)
        {
            ManualResetEvent downloadCompletedSignal = new ManualResetEvent(false);
            BlobRequestOptions blobRequestOptions = new BlobRequestOptions { RetryPolicy = retryPolicy };
            CloudBlockBlob blob = null;
            BlobTransferContext transferContext = new BlobTransferContext();
            transferContext.Exceptions = new ConcurrentBag<Exception>();

            try
            {
                blob = GetCloudBlockBlob(uri, client, retryPolicy, getSharedAccessSignature);

                long initialOffset = start;
                long sizeToDownload = blob.Properties.Length;

                if (length != -1)
                {
                    if (length > blob.Properties.Length)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            "Size {0} is beyond the Length of Blob {1}", length, blob.Properties.Length));
                    }

                    if (start + length > blob.Properties.Length)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                            "Size {0} plus offset {1} is beyond the Length of Blob {2}", length, start,
                            blob.Properties.Length));
                    }

                    sizeToDownload = length;
                }
                transferContext.Length = sizeToDownload;
                transferContext.LocalFilePath = localFile;
                transferContext.OnComplete = () => downloadCompletedSignal.Set();
                transferContext.Blob = blob;
                transferContext.FileEncryption = fileEncryption;
                transferContext.InitializationVector = initializationVector;
                transferContext.InitialOffset = start;

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

                }
                else if (sizeToDownload < cloudBlockBlobUploadDownloadSizeLimit)
                {
                    AccessCondition accessCondition = AccessCondition.GenerateEmptyCondition();
                    OperationContext operationContext = new OperationContext();
                    operationContext.ClientRequestID = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                    using (FileStream fileStream = new FileStream(
                        transferContext.LocalFilePath,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.Read
                        ))
                    {
                        blob.DownloadToStream(fileStream, accessCondition: accessCondition, options: blobRequestOptions, operationContext: operationContext);
                        if (fileEncryption != null)
                        {
                            using (MemoryStream msDecrypt = new MemoryStream())
                            {
                                //Using CryptoTransform APIs per Quintin's suggestion.
                                using (FileEncryptionTransform fileEncryptionTransform = fileEncryption.GetTransform(initializationVector, 0))
                                {
                                    fileStream.Position = 0;
                                    fileStream.CopyTo(msDecrypt);
                                    msDecrypt.Position = 0;
                                    fileStream.Position = 0;
                                    using (CryptoStream csEncrypt = new CryptoStream(msDecrypt, fileEncryptionTransform, CryptoStreamMode.Read))
                                    {
                                        csEncrypt.CopyTo(fileStream);                                   
                                    }
                                }
                            }
                            
                        }
                   }
                    InvokeProgressCallback(transferContext, sizeToDownload, sizeToDownload);
                    transferContext.OnComplete();
                }
                else
                {
                    int numThreads = parallelTransferThreadCount;
                    int blockSize = GetBlockSize(blob.Properties.Length);

                    transferContext.BlocksToTransfer = PrepareUploadDownloadQueue(sizeToDownload, blockSize,
                        ref numThreads, initialOffset);

                    transferContext.BlocksForFileIO = new ConcurrentDictionary<int, byte[]>();
                    for (int i = 0; i < transferContext.BlocksToTransfer.Count(); i++)
                    {
                        transferContext.BlocksForFileIO[i] = null;
                    }
                    transferContext.BlockSize = blockSize;
                    transferContext.CancellationToken = cancellationToken;
                    transferContext.BlobRequestOptions = blobRequestOptions;
                    transferContext.MemoryManager = MemoryManagerFactory.GetMemoryManager(blockSize);
                    transferContext.Client = client;
                    transferContext.RetryPolicy = retryPolicy;
                    transferContext.GetSharedAccessSignature = getSharedAccessSignature;
                    transferContext.ShouldDoFileIO = shouldDoFileIO;
                    transferContext.BufferStreams = new ConcurrentDictionary<byte[], MemoryStream>();
                    transferContext.ClientRequestId = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);

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
                }
            }
            catch(Exception e)
            {
                //Add the exception to the exception list.
                transferContext.Exceptions.Add(e);
            }
            finally
            {
                 // We should to be able to releaseunusedbuffers if memorymanager was initialized by then 
                if (transferContext.MemoryManager != null)
                {
                    transferContext.MemoryManager.ReleaseUnusedBuffers();
                }
                //TaskCompletedCallback should be called to populate exceptions if relevant and other eventargs for the user.
                    TaskCompletedCallback(
                        cancellationToken.IsCancellationRequested,
                        transferContext.Exceptions != null && transferContext.Exceptions.Count > 0
                            ? new AggregateException(transferContext.Exceptions)
                            : null,
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
            //Release any buffers that are still in queue to be written to file but could not because there was
            // a complete signal for this file download due to some error in the one of the other block downloads in the same transfer context.
            //If this is not cleaned, used buffers will hit the cap of 16 and future downloads will hang for lack of memory buffers.
            for (int currentBlock = transferContext.NextFileIOBlock; currentBlock <= transferContext.BlocksForFileIO.Count(); currentBlock++)
            {
                byte[] buffer = null;

                if (transferContext.BlocksForFileIO.TryGetValue(currentBlock, out buffer) && (buffer != null))
                {
                    try
                    {
                        transferContext.MemoryManager.ReleaseBuffer(buffer);
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.WriteLine("Exception occured while releasing memory buffer ",ex.Message);
                    }

                }
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
                        transferContext.BlocksForFileIO[(int)(startAndLength.Key / transferContext.BlockSize)] = streamBuffer;
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
            //This is where memory is allocated, any code after this should make sure releasebuffer happens on all positive/negative conditions
            byte[] streamBuffer = transferContext.MemoryManager.RequireBuffer();

            if (streamBuffer == null)
            {
                return;
            }
            // Catch any exceptions and cleanup the buffer. Otherwise uncleaned buffers will result in hang for future
            // downloads as memorymanager is being shared.
            // Also mark the transfer as complete and add exceptions to the transfercontext exception list.
            try
            {
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
            catch (Exception ex)
            {
                transferContext.IsComplete = true;
                transferContext.MemoryManager.ReleaseBuffer(streamBuffer);
                transferContext.Exceptions.Add(ex);
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
                throw new ArgumentException(CommonStringTable.ErrorBlobTooBigToUpload);
            }

            return (int)blocksize;
        }
    }
}
