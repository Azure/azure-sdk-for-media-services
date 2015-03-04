//-----------------------------------------------------------------------
// <copyright file="BlobTransferSpeedCalculator.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Calculates the speed of the blob transfer.
    /// </summary>
    internal class BlobTransferSpeedCalculator
    {
        private readonly int _capacity;
        private readonly Queue<long> _bytesUploadQueue;
        private readonly Queue<long> _timeUploadQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferSpeedCalculator"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public BlobTransferSpeedCalculator(int capacity)
        {
            this._capacity = capacity;
            this._bytesUploadQueue = new Queue<long>(this._capacity);
            this._timeUploadQueue = new Queue<long>(this._capacity);
        }

        /// <summary>
        /// Updates the counters and calculate speed.
        /// </summary>
        /// <param name="bytesSent">The bytes sent.</param>
        /// <returns>The speed.</returns>
        public double UpdateCountersAndCalculateSpeed(long bytesSent)
        {
            lock (this._timeUploadQueue)
            {
                double speed = 0;

                if (this._timeUploadQueue.Count >= 80)
                {
                    this._timeUploadQueue.Dequeue();
                    this._bytesUploadQueue.Dequeue();
                }

                this._timeUploadQueue.Enqueue(DateTime.Now.Ticks);
                this._bytesUploadQueue.Enqueue(bytesSent);

                if (this._timeUploadQueue.Count > 2)
                {
                    speed = (this._bytesUploadQueue.Max() - this._bytesUploadQueue.Min()) / TimeSpan.FromTicks(this._timeUploadQueue.Max() - this._timeUploadQueue.Min()).TotalSeconds;
                }

                return speed;
            }
        }
    }
}
