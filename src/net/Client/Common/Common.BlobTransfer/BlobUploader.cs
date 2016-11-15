//-----------------------------------------------------------------------
// <copyright file="BlobUploader.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class BlobUploader : BlobTransferBase
    {
        public BlobUploader(MemoryManagerFactory memoryManagerFactory)
            : base(memoryManagerFactory)
        {
        }

        public Task UploadBlob(
            Uri url,
            string name,
            Stream stream,
            FileEncryption fileEncryption,
            CancellationToken cancellationToken,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            string contentType = null,
            string subDirectory = "",
            Func<string> getSharedAccessSignature = null,
            int parallelTransferThreadCount = 10,
            int numberOfConcurrentTransfers = default(int))
        {
            SetConnectionLimits(url, numberOfConcurrentTransfers);
            return Task.Factory.StartNew(
                () => 
                UploadFileToBlob(
                    cancellationToken,
                    url,
                    name,
                    stream,
                    contentType,
                    subDirectory,
                    fileEncryption,
                    client,
                    retryPolicy,
                    getSharedAccessSignature,
                    parallelTransferThreadCount),
                    cancellationToken);

        }


        private void UploadFileToBlob(
            CancellationToken cancellationToken,
            Uri uri,
            string name,
            Stream stream,
            string contentType,
            string subDirectory,
            FileEncryption fileEncryption,
            CloudBlobClient client,
            IRetryPolicy retryPolicy,
            Func<string> getSharedAccessSignature,
            int parallelTransferThreadCount,
            bool shouldDoFileIO = true)
        {
            BlobTransferContext transferContext = new BlobTransferContext();
            transferContext.Exceptions = new ConcurrentBag<Exception>();
            try
            {
                ManualResetEvent uploadCompletedSignal = new ManualResetEvent(false);
                BlobRequestOptions blobRequestOptions = new BlobRequestOptions
                {
                    RetryPolicy = retryPolicy,
                    ServerTimeout = TimeSpan.FromSeconds(90)
                };

                CloudBlockBlob blob = GetCloudBlockBlob(uri, client, subDirectory, name, contentType, getSharedAccessSignature);

                transferContext.Length = stream.Length;
                transferContext.LocalFilePath = name;
                transferContext.OnComplete = () => uploadCompletedSignal.Set();
                transferContext.Blob = blob;
                transferContext.FileEncryption = fileEncryption;
                if (stream.Length == 0)
                {
                    blob.UploadFromByteArray(new byte[1], 0, 0, options: blobRequestOptions);
                }
                else if (stream.Length < cloudBlockBlobUploadDownloadSizeLimit)
                {
                    AccessCondition accessCondition = AccessCondition.GenerateEmptyCondition();
                    OperationContext operationContext = new OperationContext();
                    operationContext.ClientRequestID = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);

                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        byte[] fileContent = memoryStream.ToArray();
                        ApplyEncryptionTransform(
                            transferContext.FileEncryption,
                            Path.GetFileName(transferContext.LocalFilePath),
                            0,
                            fileContent,
                            Convert.ToInt32(stream.Length));

                        using (var uploadMemoryStream = new MemoryStream(fileContent))
                        {
                            blob.UploadFromStream(uploadMemoryStream, accessCondition: accessCondition, options: blobRequestOptions, operationContext: operationContext);
                        }
                    }
                    InvokeProgressCallback(transferContext, stream.Length, stream.Length);
                    transferContext.OnComplete();
                }
                else
                {
                    int numThreads = parallelTransferThreadCount;
                    int blockSize = GetBlockSize(stream.Length);

                    transferContext.BlocksToTransfer = PrepareUploadDownloadQueue(stream.Length, blockSize, ref numThreads);

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
                    transferContext.ContentType = contentType;
                    transferContext.BlobSubFolder = subDirectory;
                    transferContext.NextFileIOBlock = 0;
                    transferContext.PartialFileIOState = new ConcurrentDictionary<long, int>();

                    RunUploadLoop(transferContext, stream, numThreads);
                }
            }
            catch (Exception e)
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
                    BlobTransferType.Upload,
                    name,
                    uri);
            }
        }

        private static CloudBlockBlob GetCloudBlockBlob(
            Uri uri,
            CloudBlobClient client,
            string subFolder,
            string localFile,
            string contentType,
            Func<string> getSharedAccessSignature)
        {
            CloudBlobContainer blobContainer = null;
            CloudBlockBlob blob = null;
            if (client != null)
            {
                blobContainer = new CloudBlobContainer(uri, client.Credentials);
            }
            else
            {
                if (getSharedAccessSignature != null)
                {
                    string signature = getSharedAccessSignature();
                    blobContainer = new CloudBlobContainer(uri, new StorageCredentials(signature));
                }
                else
                {
                    blobContainer = new CloudBlobContainer(uri);
                }
            }

            string blobFileName = Path.Combine(subFolder, Path.GetFileName(localFile));
            blob = blobContainer.GetBlockBlobReference(blobFileName);
            blob.Properties.ContentType = contentType;
            return blob;
        }

        private void RunUploadLoop(
            BlobTransferContext transferContext,
            Stream fileStream,
            int numThreads)
        {
            SpinWait spinWait = new SpinWait();

            while (!transferContext.IsComplete && !transferContext.CancellationToken.IsCancellationRequested)
            {
                if (!transferContext.IsReadingOrWriting)
                {
                    DoSequentialRead(transferContext, fileStream);
                }

                if (!transferContext.IsComplete &&
                    transferContext.NumInProgressUploadDownloads < numThreads)
                {
                    TryUploadingBlocks(transferContext);
                }
                spinWait.SpinOnce();
            }

            while (transferContext.NumInProgressUploadDownloads > 0 || transferContext.IsReadingOrWriting)
            {
                spinWait.SpinOnce();
            }

            //Release any buffers that are still in queue to be written to file but could not because there was
            // a complete signal for this file upload due to some error in the one of the other block uploads in the same transfer context.
            //If this is not cleaned, used buffers will hit the cap of 16 and future uploads will hang for lack of memory buffers.
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
                        Debug.WriteLine("Exception occured while releasing memory buffer ", ex.Message);
                    }

                }
            }
            foreach (var memoryStream in transferContext.BufferStreams.Values)
            {
                memoryStream.Dispose();
            }

            transferContext.OnComplete();

        }

        private void BeginUploadStream(
            BlobTransferContext transferContext,
            KeyValuePair<long, int> startAndLength,
            MemoryStream memoryStream,
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

            string blockId =
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "BlockId{0:d7}", (startAndLength.Key / transferContext.BlockSize))));

            memoryStream.SetLength(startAndLength.Value);

            transferContext.Blob.BeginPutBlock(
                blockId,
                memoryStream,
                null,
                AccessCondition.GenerateEmptyCondition(),
                transferContext.BlobRequestOptions,
                operationContext,
                ar =>
                {
                    SuccessfulOrRetryableResult wasWriteSuccessful = EndPutBlock(transferContext, ar);
                    Interlocked.Decrement(ref transferContext.NumInProgressUploadDownloads);

                    if (wasWriteSuccessful.IsRetryable)
                    {
                        BeginUploadStream(transferContext, startAndLength, memoryStream, streamBuffer);
                        return;
                    }

                    transferContext.MemoryManager.ReleaseBuffer(streamBuffer);

                    if (!wasWriteSuccessful.IsSuccessful)
                    {
                        return;
                    }

                    Interlocked.Add(ref transferContext.BytesBlobIOCompleted, startAndLength.Value);

                    InvokeProgressCallback(transferContext, transferContext.BytesBlobIOCompleted, startAndLength.Value);

                    if (transferContext.BytesBlobIOCompleted >= transferContext.Length)
                    {
                        BeginPutBlockList(transferContext);
                    }

                },
                state: null);
        }

        protected virtual SuccessfulOrRetryableResult EndPutBlock(BlobTransferContext transferContext, IAsyncResult ar)
        {
            return IsActionSuccessfulOrRetryable(transferContext, () => transferContext.Blob.EndPutBlock(ar));
        }

        private void TryUploadingBlocks(BlobTransferContext transferContext)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (transferContext.NextFileIOBlock >= transferContext.BlocksForFileIO.Count)
            {
                return;
            }

            byte[] streamBuffer;
            int nextBlock = transferContext.NextFileIOBlock;

            if (transferContext.BlocksForFileIO.TryGetValue(nextBlock, out streamBuffer)
                && streamBuffer != null)
            {
                Interlocked.Increment(ref transferContext.NextFileIOBlock);

                MemoryStream memoryStream = GetMemoryStream(transferContext.BufferStreams, streamBuffer);

                long beginFilePosition = (long)nextBlock * transferContext.BlockSize;
                long nextBeginFilePosition = beginFilePosition + transferContext.BlockSize;

                nextBeginFilePosition =
                    nextBeginFilePosition > transferContext.Length
                        ? transferContext.Length
                        : nextBeginFilePosition;

                int bytesToRead = (int)(nextBeginFilePosition - beginFilePosition);

                BeginUploadStream(
                    transferContext,
                    new KeyValuePair<long, int>(beginFilePosition, bytesToRead),
                    memoryStream,
                    streamBuffer);
            }
        }


        private void BeginPutBlockList(BlobTransferContext transferContext)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            OperationContext operationContext = new OperationContext();
            operationContext.ClientRequestID = transferContext.ClientRequestId;

            List<string> blockids = new List<string>();
            for (int i = 0; i < (transferContext.Length + transferContext.BlockSize - 1) / transferContext.BlockSize; i++)
            {
                blockids.Add(
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture, "BlockId{0:d7}", i))));
            }

            transferContext.Blob.BeginPutBlockList(
                blockids,
                AccessCondition.GenerateEmptyCondition(),
                transferContext.BlobRequestOptions,
                operationContext,
                ar =>
                {
                    SuccessfulOrRetryableResult wasWriteSuccessful = EndPutBlockList(transferContext, ar);
                    Interlocked.Decrement(ref transferContext.NumInProgressUploadDownloads);

                    if (wasWriteSuccessful.IsRetryable)
                    {
                        BeginPutBlockList(transferContext);
                        return;
                    }

                    transferContext.IsComplete = true;
                },
                state: null);
        }

        private SuccessfulOrRetryableResult EndPutBlockList(BlobTransferContext transferContext, IAsyncResult ar)
        {
            return IsActionSuccessfulOrRetryable(transferContext, () => transferContext.Blob.EndPutBlockList(ar));
        }

        private void DoSequentialRead(
            BlobTransferContext transferContext,
            Stream stream,
            byte[] streamBuffer = null,
            KeyValuePair<long, int>? inputStartAndLength = null)
        {
            if (transferContext.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (streamBuffer == null)
            {
                streamBuffer = transferContext.MemoryManager.RequireBuffer();
            }

            if (streamBuffer == null)
            {
                return;
            }

            KeyValuePair<long, int> startAndLength;

            if (inputStartAndLength == null)
            {
                if (!transferContext.BlocksToTransfer.TryDequeue(out startAndLength))
                {
                    transferContext.MemoryManager.ReleaseBuffer(streamBuffer);
                    return;
                }
            }
            else
            {
                startAndLength = inputStartAndLength.Value;
            }
            // Catch any exceptions and cleanup the buffer. Otherwise uncleaned buffers will result in hang for future
            // uploads as memorymanager is being shared.
            // Also mark the transfer as complete and add exceptions to the transfercontext exception list.
            try
            {
                if (!transferContext.PartialFileIOState.ContainsKey(startAndLength.Key))
                {
                    transferContext.PartialFileIOState[startAndLength.Key] = 0;
                }

                transferContext.IsReadingOrWriting = true;

                long beginFilePosition = startAndLength.Key;
                long nextBeginFilePosition = startAndLength.Key + transferContext.BlockSize;

                nextBeginFilePosition =
                    nextBeginFilePosition > transferContext.Length
                        ? transferContext.Length
                        : nextBeginFilePosition;

                beginFilePosition = beginFilePosition + transferContext.PartialFileIOState[startAndLength.Key];
                int bytesToRead = (int)(nextBeginFilePosition - beginFilePosition);

                stream.BeginRead(
                    streamBuffer,
                    transferContext.PartialFileIOState[startAndLength.Key],
                    bytesToRead,
                    result3 =>
                    {
                        int bytesRead;

                        SuccessfulOrRetryableResult wasReadSuccessful =
                            IsActionSuccessfulOrRetryable(transferContext, () => stream.EndRead(result3), out bytesRead);

                        if (!wasReadSuccessful.IsSuccessful)
                        {
                            transferContext.IsReadingOrWriting = false;

                            transferContext.MemoryManager.ReleaseBuffer(streamBuffer);
                        }
                        else if (bytesRead != bytesToRead)
                        {
                            transferContext.PartialFileIOState[startAndLength.Key] += bytesRead;

                            DoSequentialRead(transferContext, stream, streamBuffer, startAndLength);
                        }
                        else
                        {
                            transferContext.IsReadingOrWriting = false;

                            ApplyEncryptionTransform(
                                transferContext.FileEncryption,
                                Path.GetFileName(transferContext.LocalFilePath),
                                beginFilePosition,
                                streamBuffer,
                                bytesToRead);

                            transferContext.BlocksForFileIO[(int)(startAndLength.Key / transferContext.BlockSize)] = streamBuffer;
                        }
                    },
                    null);
            }
            catch (Exception ex)
            {
                transferContext.IsComplete = true;
                transferContext.MemoryManager.ReleaseBuffer(streamBuffer);
                transferContext.Exceptions.Add(ex);
            }

        }

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
                throw new ArgumentException(CommonStringTable.ErrorBlobTooBigToUpload);
            }

            return (int)blocksize;
        }
    }
}
