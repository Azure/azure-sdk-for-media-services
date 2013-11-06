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
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Origin settings.
    /// </summary>
    public class OriginSettings
    {
        /// <summary>
        /// Gets or sets playback settings.
        /// </summary>
        public PlaybackEndpointSettings Playback { get; set; }
    }

    /// <summary>
    /// Describes playback endpoint settings.
    /// </summary>
    public class PlaybackEndpointSettings
    {
        /// <summary>
        /// Gets or sets maximum age of the cache.
        /// </summary>
        public TimeSpan? MaxCacheAge { get; set; }

        /// <summary>
        /// Gets or sets security settings.
        /// </summary>
        public PlaybackEndpointSecuritySettings Security { get; set; }
    }

    /// <summary>
    /// Describes Ingest endpoint security settings.
    /// </summary>
    public class PlaybackEndpointSecuritySettings
    {
        /// <summary>
        /// Gets or sets the list of IP-s allowed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Ipv4> IPv4AllowList { get; set; }

        /// <summary>
        /// Gets or sets the list of Akamai Signature Header Authentication keys.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<AkamaiSignatureHeaderAuthenticationKey> AkamaiSignatureHeaderAuthentication { get; set; }
    }
}
