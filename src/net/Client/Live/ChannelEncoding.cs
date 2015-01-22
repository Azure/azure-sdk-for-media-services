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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies channel Encoding settings.
    /// </summary>
    public class ChannelEncoding
    {
        /// <summary>
        /// Gets or sets the Encoding profile.
        /// </summary>
        public string SystemPreset { get; set; }

        /// <summary>
        /// Gets or sets the source video streams.
        /// </summary>
        public ReadOnlyCollection<VideoStream> VideoStreams { get; set; }

        /// <summary>
        /// Gets or sets the source audio streams.
        /// </summary>
        public ReadOnlyCollection<AudioStream> AudioStreams { get; set; }
    }
}
