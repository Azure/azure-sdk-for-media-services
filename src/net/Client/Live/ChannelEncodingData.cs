// Copyright 2014 Microsoft Corporation
// 
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies channel Encoding settings. This is object converted from 
    /// <see cref="ChannelEncoding"/> and serialized to REST.
    /// </summary>
    internal class ChannelEncodingData
    {
        /// <summary>
        /// Gets or sets the Encoding profile.
        /// </summary>
        public string SystemPreset { get; set; }

        /// <summary>
        /// Gets or sets the source video streams.
        /// </summary>
        public List<VideoStream> VideoStreams { get; set; }

        /// <summary>
        /// Gets or sets the source audio streams.
        /// </summary>
        public List<AudioStream> AudioStreams { get; set; }

        /// <summary>
        /// Creates an instance of ChannelEncodingData class from an instance of ChannelEncoding.
        /// </summary>
        /// <param name="encoding">Channel Encoding to copy into newly created instance.</param>
        public ChannelEncodingData(ChannelEncoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            SystemPreset = encoding.SystemPreset;

            if (encoding.AudioStreams != null)
            {
                AudioStreams = encoding.AudioStreams.ToList();
            }

            if (encoding.VideoStreams != null)
            {
                VideoStreams = encoding.VideoStreams.ToList();
            }
        }

        /// <summary>
        /// Casts ChannelEncodingData to ChannelEncoding.
        /// </summary>
        /// <param name="encoding">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelEncoding(ChannelEncodingData encoding)
        {
            if (encoding == null)
            {
                return null;
            }

            var result = new ChannelEncoding
            {
                SystemPreset = encoding.SystemPreset
            };

            if (encoding.AudioStreams != null)
            {
                result.AudioStreams = encoding.AudioStreams.AsReadOnly();
            }

            if (encoding.VideoStreams != null)
            {
                result.VideoStreams = encoding.VideoStreams.AsReadOnly();
            }

            return result;
        }
    }
}
