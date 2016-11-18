//-----------------------------------------------------------------------
// <copyright file="IChannelHeartbeat.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A channel heartbeat metric.
    /// </summary>
    public interface IChannelHeartbeat
    {
        /// <summary>
        /// Gets the partition key of the record.
        /// </summary>
        string PartitionKey { get; }

        /// <summary>
        /// Gets the row key of the record.
        /// </summary>
        string RowKey { get; }

        /// <summary>
        /// Gets the Media Services account ID.
        /// </summary>
        Guid AccountId { get; }

        /// <summary>
        /// Gets the Media Services Channel ID.
        /// </summary>
        Guid ChannelId { get; }

        /// <summary>
        /// Gets the observed time of the metric.
        /// </summary>
        DateTime ObservedTime { get; }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        string CustomAttributes { get; }

        /// <summary>
        /// Gets the track type.
        /// </summary>
        string TrackType { get; }

        /// <summary>
        /// Gets the track name.
        /// </summary>
        string TrackName { get; }

        /// <summary>
        /// Gets the bitrate.
        /// </summary>
        int Bitrate { get; }

        /// <summary>
        /// Gets the incoming bitrate.
        /// </summary>
        int IncomingBitrate { get; }

        /// <summary>
        /// Gets the overlap count.
        /// </summary>
        int OverlapCount { get; }

        /// <summary>
        /// Gets the discontinuity count.
        /// </summary>
        int DiscontinuityCount { get; }

        /// <summary>
        /// Gets the last time stamp.
        /// </summary>
        ulong LastTimestamp { get; }

        /// <summary>
        /// Gets a count of fragments discarded due to nonincreasing timestamp.
        /// </summary>
        int NonincreasingCount { get; }

        /// <summary>
        /// Gets a value indicating whether key frames are unaligned across different streams.
        /// </summary>
        bool UnalignedKeyFrames { get; }

        /// <summary>
        /// Gets a value indicating whether presentation time is unaligned across different streams.
        /// </summary>
        bool UnalignedPresentationTime { get; }

        /// <summary>
        /// Gets a value indicating whether calculated ingest bitrate for this stream is significantly different from the bitrate defined in the stream headers.
        /// </summary>
        bool UnexpectedBitrate { get; }

        /// <summary>
        /// Gets a value indicating whether channel is healthy.
        /// </summary>
        bool Healthy { get; }
    }
}
