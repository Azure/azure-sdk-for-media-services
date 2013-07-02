// Copyright 2012 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Origin settings.
    /// </summary>
    public class OriginSettings
    {
        /// <summary>
        /// Playback settings.
        /// </summary>
        public PlaybackEndpointSettings Playback { get; set; }
    }

    /// <summary>
    /// Describes playback settings.
    /// </summary>
    public class PlaybackEndpointSettings
    {
        /// <summary>
        /// Maximum cache age in minutes.
        /// </summary>
        public long MaxCacheAgeInMins { get; set; }

        /// <summary>
        /// Security settings.
        /// </summary>
        public SecuritySettings Security { get; set; }
    }
}
