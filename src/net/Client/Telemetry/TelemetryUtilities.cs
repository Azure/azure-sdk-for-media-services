//-----------------------------------------------------------------------
// <copyright file="TelemetryUtilities.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    internal static class TelemetryUtilities
    {
        /// <summary>
        /// The prefix for the streaming endpoint Id.
        /// </summary>
        private const string StreamingEndPointIdentifierPrefix = "nb:oid:UUID:";

        /// <summary>
        /// The prefix for the channel Id.
        /// </summary>
        private const string ChannelIdentifierPrefix = "nb:chid:UUID:";

        /// <summary>
        /// Get storage account name from the given endpoint address
        /// </summary>
        /// <param name="endpointAddress"></param>
        /// <returns></returns>
        public static string GetStorageAccountName(string endpointAddress)
        {
            var uriBuilder = new UriBuilder(endpointAddress);
            var entries = uriBuilder.Host.Split('.');
            if (entries.Length < 1)
            {
                throw new UriFormatException("endpointAddress");
            }
            return entries[0];
        }

        /// <summary>
        /// Validate and parse the streaming endpoint Id to Guid format.
        /// </summary>
        /// <param name="streamingEndpointId">The streaming endpoint Id.</param>
        /// <returns>The Guid format of the streaming endpoint Id.</returns>
        public static Guid ParseStreamingEndPointId(string streamingEndpointId)
        {
            if (String.IsNullOrWhiteSpace(streamingEndpointId))
            {
                throw new ArgumentException("streamingEndpointId");
            }

            if (streamingEndpointId.StartsWith(StreamingEndPointIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                streamingEndpointId = streamingEndpointId.Remove(0, StreamingEndPointIdentifierPrefix.Length);
            }

            Guid streamingEndpointIdGuid;
            if (!Guid.TryParse(streamingEndpointId, out streamingEndpointIdGuid))
            {
                throw new ArgumentException(StringTable.InvalidStreamingEndPointInput);
            }
            return streamingEndpointIdGuid;
        }

        /// <summary>
        /// Validate and parse the channel Id to Guid format.
        /// </summary>
        /// <param name="channelId">The Channel Id.</param>
        /// <returns>The Guid format of the streaming endpoint Id.</returns>
        public static Guid ParseChannelId(string channelId)
        {
            if (String.IsNullOrWhiteSpace(channelId))
            {
                throw new ArgumentException("channelId.");
            }

            if (channelId.StartsWith(ChannelIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                channelId = channelId.Remove(0, ChannelIdentifierPrefix.Length);
            }

            Guid channelIdGuid;
            if (!Guid.TryParse(channelId, out channelIdGuid))
            {
                throw new ArgumentException(StringTable.InvalidChannelInput);
            }
            return channelIdGuid;
        }
    }
}
