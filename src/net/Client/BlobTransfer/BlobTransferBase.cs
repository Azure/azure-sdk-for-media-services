using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal abstract class BlobTransferBase
    {
        private const int SpeedCalculatorCapacity = 100;
        private const int MaxSasSignatureRetry = 30;
		private readonly TimeSpan SasSignatureRetryTime = TimeSpan.FromSeconds(1);
		private readonly TimeSpan SasPolicyActivationMaxTime = TimeSpan.FromSeconds(30);

        private readonly BlobTransferSpeedCalculator _uploadDownloadSpeedCalculator = 
            new BlobTransferSpeedCalculator(SpeedCalculatorCapacity);

        public event EventHandler<BlobTransferCompleteEventArgs> TransferCompleted;

        public event EventHandler<BlobTransferProgressChangedEventArgs> TransferProgressChanged;

        protected MemoryManagerFactory MemoryManagerFactory { get; set; }

        protected BlobTransferBase(MemoryManagerFactory memoryManagerFactory)
        {
            if (memoryManagerFactory == null)
            {
                throw new ArgumentNullException("memoryManagerFactory");
            }
            MemoryManagerFactory = memoryManagerFactory;
        }

        protected void SetConnectionLimits(Uri url)
        {
            ServicePointModifier.SetConnectionPropertiesForSmallPayloads(url);
        }

        private const int ConnectionLimitMultiplier = 8;
        protected const int ParallelUploadDownloadThreadCountMultiplier = 3;

        protected struct SuccessfulOrRetryableResult
        {
            public bool IsSuccessful { get; set; }
            public bool IsRetryable { get; set; }

            public static bool operator ==(SuccessfulOrRetryableResult left, SuccessfulOrRetryableResult right)
            {
                return left.IsSuccessful == right.IsSuccessful && left.IsRetryable == right.IsRetryable;
            }

            public static bool operator !=(SuccessfulOrRetryableResult left, SuccessfulOrRetryableResult right)
            {
                return left.IsSuccessful != right.IsSuccessful || left.IsRetryable != right.IsRetryable;
            }

            public override bool Equals(object obj)
            {
                return obj is SuccessfulOrRetryableResult && this == (SuccessfulOrRetryableResult) obj;
            }

            public override int GetHashCode()
            {
                return IsRetryable.GetHashCode() + IsSuccessful.GetHashCode();
            }
        }

        protected SuccessfulOrRetryableResult IsActionSuccessfulOrRetryable<T>(
            BlobTransferContext transferContext, 
            Func<T> action,
            out T returnValue)
        {
            T value = default(T);

            SuccessfulOrRetryableResult result =
                IsActionSuccessfulOrRetryable(
                    transferContext,
                    () => value = action());

            returnValue = value;

            return result;
        }

        protected SuccessfulOrRetryableResult IsActionSuccessfulOrRetryable(BlobTransferContext transferContext, Action action)
        {
            if (transferContext == null)
            {
                throw new ArgumentNullException("transferContext");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }


            SuccessfulOrRetryableResult result = 
                new SuccessfulOrRetryableResult
                {
                    IsRetryable = false,
                    IsSuccessful = false
                };

            try
            {
                action();
            }
            catch (Exception exception)
            {
                WebException webEx = exception.FindInnerException<WebException>();

                if (webEx == null)
                {
                    transferContext.Exceptions.Add(exception);
                    transferContext.IsComplete = true;
                    return result;
                }

                if (transferContext.GetSharedAccessSignature != null)
                {
                    if (webEx.Response is HttpWebResponse)
                    {
                        var httpex = (HttpWebResponse)webEx.Response;
                        if (httpex.StatusCode == HttpStatusCode.Forbidden)
                        {
                            Interlocked.Increment(ref transferContext.SasRetryCount);

                            if (transferContext.SasRetryCount > MaxSasSignatureRetry)
                            {
                                transferContext.Exceptions.Add(exception); 
                                transferContext.IsComplete = true;
                                return result;
                            }

							Thread.Sleep(SasSignatureRetryTime);
                            result.IsRetryable = true;
                            return result;
                        }
                    }
                }

                transferContext.Exceptions.Add(exception);
                transferContext.IsComplete = true;
                return result;
            }

            result.IsSuccessful = true;
            return result;
        }

        protected void InvokeProgressCallback(BlobTransferContext transferContext, long bytesProcessed, long lastBlockSize)
        {
            if (transferContext == null)
            {
                throw new ArgumentNullException("transferContext");
            }

            int progress = (int)((double)bytesProcessed / transferContext.Length * 100);
            double speed = _uploadDownloadSpeedCalculator.UpdateCountersAndCalculateSpeed(bytesProcessed);

            BlobTransferProgressChangedEventArgs eArgs = new BlobTransferProgressChangedEventArgs(
                bytesProcessed,
				lastBlockSize,
                transferContext.Length,
                progress,
                speed,
                transferContext.Blob.Uri,
                transferContext.LocalFilePath,
                null);

            OnTaskProgressChanged(eArgs);
        }

        protected void ApplyEncryptionTransform(FileEncryption fileEncryption, ulong initializationVector, long beginFilePosition, byte[] buffer, int bytesToWrite)
        {
            if (fileEncryption != null)
            {
                lock (fileEncryption)
                {
                    using (
                        FileEncryptionTransform encryptor = fileEncryption.GetTransform(initializationVector, beginFilePosition))
                    {
                        encryptor.TransformBlock(inputBuffer: buffer, inputOffset: 0, inputCount: bytesToWrite, outputBuffer: buffer, outputOffset: 0);
                    }
                }
            }
        }

        protected void ApplyEncryptionTransform(FileEncryption fileEncryption, string fileName, long beginFilePosition, byte[] buffer, int bytesToWrite)
        {
            if (fileEncryption != null)
            {
                lock (fileEncryption)
                {
                    using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(fileName, beginFilePosition))
                    {
                        encryptor.TransformBlock(inputBuffer: buffer, inputOffset: 0, inputCount: bytesToWrite, outputBuffer: buffer, outputOffset: 0);
                    }
                }
            }
        }

        protected MemoryStream GetMemoryStream(ConcurrentDictionary<byte[], MemoryStream> bufferMetadata, byte[] streamBuffer)
        {
            if (bufferMetadata == null)
            {
                throw new ArgumentNullException("bufferMetadata");
            }
            if (streamBuffer == null)
            {
                throw new ArgumentNullException("streamBuffer");
            }

            MemoryStream memoryStream;
            if (!bufferMetadata.TryGetValue(streamBuffer, out memoryStream))
            {
                memoryStream = new MemoryStream(streamBuffer);

                bufferMetadata[streamBuffer] = memoryStream;
            }
            else
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            return memoryStream;
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected ConcurrentQueue<KeyValuePair<long, int>> PrepareUploadDownloadQueue(long blobLength, int bufferLength, ref int numThreads, long initialOffset = 0)
        {
            // Prepare a queue of chunks to be downloaded. Each queue item is a key-value pair 
            // where the 'key' is start offset in the blob and 'value' is the chunk length.
            var queue = new ConcurrentQueue<KeyValuePair<long, int>>();
            long offset = initialOffset;
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

        protected void TaskCompletedCallback(bool isCanceled, Exception ex, BlobTransferType transferType, string localFile, Uri url)
        {
            if (TransferCompleted != null)
            {
                TransferCompleted(this, new BlobTransferCompleteEventArgs(ex, isCanceled, null, localFile, url, transferType));
            }
            else
            {
                if (ex != null)
                {
                    throw ex;
                }
            }
        }

        protected virtual void OnTaskProgressChanged(BlobTransferProgressChangedEventArgs e)
        {
            if (TransferProgressChanged != null)
            {
                TransferProgressChanged(this, e);
            }
        }

		protected void BlobPolicyActivationWait(Action request)
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

			while (stopwatch.Elapsed < SasPolicyActivationMaxTime)
			{
				try
				{
					request();
					break;
				}
				catch (StorageException x)
				{
                    WebException webException = x.FindInnerException<WebException>();

					if (webException == null || !(webException.Response is HttpWebResponse))
					{
						throw;
					}
					var status = ((HttpWebResponse)webException.Response).StatusCode;
					if (status != HttpStatusCode.Forbidden)
					{
						throw;
					}
					Thread.Sleep(SasSignatureRetryTime);
				}
			}
		}
    }
}
