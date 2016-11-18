//-----------------------------------------------------------------------
// <copyright file="ChannelHeartbeat.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A channel heartbeat metric.
    /// </summary>
    internal class ChannelHeartbeat : IChannelHeartbeat
    {
        /// <summary>
        /// Gets the partition key of the record.
        /// </summary>
        public string PartitionKey { get; private set; }

        /// <summary>
        /// Gets the row key of the record.
        /// </summary>
        public string RowKey { get; private set; }

        /// <summary>
        /// Gets the Media Services account ID.
        /// </summary>
        public Guid AccountId { get; private set; }

        /// <summary>
        /// Gets the Media Services Channel ID.
        /// </summary>
        public Guid ChannelId { get; private set; }

        /// <summary>
        /// Gets the observed time of the metric.
        /// </summary>
        public DateTime ObservedTime { get; private set; }

        /// <summary>
        /// Gets the custom attributes.
        /// </summary>
        public string CustomAttributes { get; private set; }

        /// <summary>
        /// Gets the track type.
        /// </summary>
        public string TrackType { get; private set; }

        /// <summary>
        /// Gets the track name.
        /// </summary>
        public string TrackName { get; private set; }

        /// <summary>
        /// Gets the bitrate.
        /// </summary>
        public int Bitrate { get; private set; }

        /// <summary>
        /// Gets the incoming bitrate.
        /// </summary>
        public int IncomingBitrate { get; private set; }

        /// <summary>
        /// Gets the overlap count.
        /// </summary>
        public int OverlapCount { get; private set; }

        /// <summary>
        /// Gets the discontinuity count.
        /// </summary>
        public int DiscontinuityCount { get; private set; }

        /// <summary>
        /// Gets the last time stamp.
        /// </summary>
        public ulong LastTimestamp { get; private set; }

        /// <summary>
        /// Gets a count of fragments discarded due to nonincreasing timestamp.
        /// </summary>
        public int NonincreasingCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether key frames are unaligned across different streams.
        /// </summary>
        public bool UnalignedKeyFrames { get; private set; }

        /// <summary>
        /// Gets a value indicating whether presentation time is unaligned across different streams.
        /// </summary>
        public bool UnalignedPresentationTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether calculated ingest bitrate for this stream is significantly different from the bitrate defined in the stream headers.
        /// </summary>
        public bool UnexpectedBitrate { get; private set; }

        /// <summary>
        /// Gets a value indicating whether channel is healthy.
        /// </summary>
        public bool Healthy { get; private set; }

        /// <summary>
        /// Creates a ChannelHeartbeat object from a Azure Table Storage row.
        /// </summary>
        /// <param name="entity">The Azure Table Storage row.</param>
        /// <returns>The new ChannelHeartbeat object.</returns>
        public static ChannelHeartbeat FromTableEntity(DynamicTableEntity entity)
        {
            var partitionKeyParts = entity.PartitionKey.Split('_');
            
            return new ChannelHeartbeat(
                entity.PartitionKey,
                entity.RowKey,
                Guid.ParseExact(partitionKeyParts[0], "N"),
                entity.Properties["ServiceId"].GuidValue.GetValueOrDefault(),
                entity.Properties["ObservedTime"].DateTime.GetValueOrDefault(),
                entity.Properties["CustomAttributes"].StringValue,
                entity.Properties["TrackType"].StringValue,
                entity.Properties["TrackName"].StringValue,
                entity.Properties["Bitrate"].Int32Value.GetValueOrDefault(),
                entity.Properties["IncomingBitrate"].Int32Value.GetValueOrDefault(),
                entity.Properties["OverlapCount"].Int32Value.GetValueOrDefault(),
                entity.Properties["DiscontinuityCount"].Int32Value.GetValueOrDefault(),
                (ulong)entity.Properties["LastTimestamp"].Int64Value.GetValueOrDefault(),
                entity.Properties["NonincreasingCount"].Int32Value.GetValueOrDefault(),
                entity.Properties["UnalignedKeyFrames"].BooleanValue.GetValueOrDefault(),
                entity.Properties["UnalignedPresentationTime"].BooleanValue.GetValueOrDefault(),
                entity.Properties["UnexpectedBitrate"].BooleanValue.GetValueOrDefault(),
                entity.Properties["Healthy"].BooleanValue.GetValueOrDefault()
                );
        }

        /// <summary>
        /// Initializes a new instance of the ChannelHeartbeat class.
        /// </summary>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="rowKey">The row key.</param>
        /// <param name="accountId">The Media Services account ID.</param>
        /// <param name="channelId">The Channel ID.</param>
        /// <param name="observedTime">The observed time of the metric.</param>
        /// <param name="customAttributes">The custom attributes.</param>
        /// <param name="trackType">The track type.</param>
        /// <param name="trackName">The track name.</param>
        /// <param name="bitrate">The bitrate.</param>
        /// <param name="incomingBitrate">The incoming bitrate.</param>
        /// <param name="overlapCount">The overlap count.</param>
        /// <param name="discontinuityCount">The discontinuity count.</param>
        /// <param name="lastTimestamp">The last time stamp.</param>
        public ChannelHeartbeat(
            string partitionKey, 
            string rowKey, 
            Guid accountId, 
            Guid channelId, 
            DateTime observedTime, 
            string customAttributes, 
            string trackType, 
            string trackName, 
            int bitrate, 
            int incomingBitrate, 
            int overlapCount, 
            int discontinuityCount, 
            ulong lastTimestamp,
            int nonincreasingCount,
            bool unalignedKeyFrames,
            bool unalignedPresentationTime,
            bool unexpectedBitrate,
            bool healthy)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            AccountId = accountId;
            ChannelId = channelId;
            ObservedTime = observedTime;
            CustomAttributes = customAttributes;
            TrackType = trackType;
            TrackName = trackName;
            Bitrate = bitrate;
            IncomingBitrate = incomingBitrate;
            OverlapCount = overlapCount;
            DiscontinuityCount = discontinuityCount;
            LastTimestamp = lastTimestamp;
            NonincreasingCount = nonincreasingCount;
            UnalignedKeyFrames = unalignedKeyFrames;
            UnalignedPresentationTime = unalignedPresentationTime;
            UnexpectedBitrate = unexpectedBitrate;
            Healthy = healthy;
        }
    }
}
