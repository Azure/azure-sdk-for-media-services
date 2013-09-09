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

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Rest
{
    /// <summary>
    /// Describes Origin settings.
    /// </summary>
    internal class OriginServiceSettings
    {
        /// <summary>
        /// Creates an instance of OriginServiceSettings class.
        /// </summary>
        public OriginServiceSettings() { }

        /// <summary>
        /// Creates an instance of OriginServiceSettings class from an instance of OriginSettings.
        /// </summary>
        /// <param name="settings">Settings to copy into newly created instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public OriginServiceSettings(OriginSettings settings) 
        {
            if (settings != null && settings.Playback != null)
            {
                Playback = new PlaybackEndpointSettings { Security = settings.Playback.Security };

                if (settings.Playback.MaxCacheAge.HasValue)
                {
                    Playback.MaxCacheAge = (long)settings.Playback.MaxCacheAge.Value.TotalSeconds;
                }
            }
        }

        /// <summary>
        /// Gets or sets playback settings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public PlaybackEndpointSettings Playback { get; set; }

        /// <summary>
        /// Casts OriginServiceSettings to OriginSettings.
        /// </summary>
        /// <param name="settings">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator OriginSettings(OriginServiceSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            var result = new OriginSettings();

            if (settings.Playback != null)
            {
                result.Playback = new Client.PlaybackEndpointSettings { Security = settings.Playback.Security };

                if (settings.Playback.MaxCacheAge.HasValue)
                {
                    result.Playback.MaxCacheAge = TimeSpan.FromSeconds(settings.Playback.MaxCacheAge.Value);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Describes playback endpoint settings.
    /// </summary>
    internal class PlaybackEndpointSettings
    {
        /// <summary>
        /// Gets or sets maximum age of the cache in seconds.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public long? MaxCacheAge { get; set; }

        /// <summary>
        /// Gets or sets security settings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public PlaybackEndpointSecuritySettings Security { get; set; }
    }
}
