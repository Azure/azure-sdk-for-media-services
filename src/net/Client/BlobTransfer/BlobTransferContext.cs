using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal class BlobTransferContext
    {
        public ConcurrentQueue<KeyValuePair<long, int>> BlocksToTransfer { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public string LocalFilePath { get; set; }

        public string ContentType { get; set; }

        public string BlobSubFolder { get; set; }

        public CloudBlockBlob Blob { get; set; }

        public BlobRequestOptions BlobRequestOptions { get; set; }

        public long Length { get; set; }

        public long BytesBlobIOCompleted;

        public long BytesWrittenOrReadToFile;

        public Action OnComplete { get; set; }

        public MemoryManager MemoryManager { get; set; }

        public ConcurrentDictionary<int, byte[]> BlocksForFileIO { get; set; }

        public ConcurrentDictionary<long, int> PartialFileIOState { get; set; }

        public int NextFileIOBlock;

        public long NumInProgressUploadDownloads;

        public bool IsComplete { get; set; }

        public int BlockSize { get; set; }

        public volatile bool IsReadingOrWriting;

        public CloudBlobClient Client { get; set; }

        public IRetryPolicy RetryPolicy { get; set; }

        public Func<string> GetSharedAccessSignature { get; set; }

        public int SasRetryCount;

        public bool ShouldDoFileIO { get; set; }

        public ConcurrentDictionary<byte[], MemoryStream> BufferStreams { get; set; }

        public string ClientRequestId { get; set; }

        public ConcurrentBag<Exception> Exceptions { get; set; }

        public FileEncryption FileEncryption { get; set; }

        public ulong InitializationVector { get; set; }

        public long InitialOffset { get; set; }
    }
}
