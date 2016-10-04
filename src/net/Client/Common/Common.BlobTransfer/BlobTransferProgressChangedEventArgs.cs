//-----------------------------------------------------------------------
// <copyright file="BlobTransferProgressChangedEventArgs.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.ComponentModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents the progress of a blob tranfer, used by <see cref="BlobTransferClient.TransferProgressChanged"/> event.
    /// </summary>
    public class BlobTransferProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="bytesTransferred">Number of bytes transferred so far.</param>
        /// <param name="lastBlockBytesTransferred">Number of bytes transferred in the last block.</param>
        /// <param name="totalBytesToTransfer">Total number of bytes to transfer.</param>
        /// <param name="progressPercentage">Percentage of bytes that finished transfering.</param>
        /// <param name="speed">Average speed of transfer in bytes per second.</param>
        /// <param name="uri">Uri of the blob location to transfer the data.</param>
        /// <param name="sourceName">Name of the object being transferred.</param>
        /// <param name="userState">User state information to be passed through.</param>
        public BlobTransferProgressChangedEventArgs(long bytesTransferred, long lastBlockBytesTransferred, long totalBytesToTransfer, int progressPercentage, double speed, Uri uri, string sourceName, object userState)
            : base(progressPercentage, userState)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                throw new ArgumentException(CommonStringTable.ErrorLocalFilenameIsNullOrEmpty);
            }

            this.BytesTransferred = bytesTransferred;
            this.LastBlockBytesTransferred = lastBlockBytesTransferred;
            this.TotalBytesToTransfer = totalBytesToTransfer;
            this.TransferRateBytesPerSecond = speed;
            this.Uri = uri;
            this.SourceName = sourceName;
        }

        /// <summary>
        /// Gets the bytes transferred so far.
        /// </summary>
        public long BytesTransferred { get; private set; }

        /// <summary>
        /// Gets the bytes transferred in the last block.
        /// </summary>
        public long LastBlockBytesTransferred { get; private set; }

        /// <summary>
        /// Gets the total bytes to transfer.
        /// </summary>
        public long TotalBytesToTransfer { get; private set; }

        /// <summary>
        /// Gets the transfer speed.
        /// </summary>
        public double TransferRateBytesPerSecond { get; private set; }

        /// <summary>
        /// Gets the time remaining.
        /// </summary>
        public TimeSpan TimeRemaining
        {
            get
            {
                var time = new TimeSpan(0, 0, (int)((this.TotalBytesToTransfer - this.BytesTransferred) / (this.TransferRateBytesPerSecond == 0 ? 1 : this.TransferRateBytesPerSecond)));
                return time;
            }
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Gets the full path of local file or a unique name for a stream.
        /// </summary>
        public string SourceName { get; private set; }
    }
}
