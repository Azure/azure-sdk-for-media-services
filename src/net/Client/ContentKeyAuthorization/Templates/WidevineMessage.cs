//-----------------------------------------------------------------------
// <copyright file="WidevineMessage.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.MediaServices.Client.Widevine
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AllowedTrackTypes { SD_ONLY, SD_HD }

    /// <summary>
    /// See Widevine Modular DRM Proxy Integration documentation.
    /// </summary>
    public class WidevineMessage
    {
        /// <summary>
        /// Controls which content keys should be included in a license. 
        /// Only one of allowed_track_types and content_key_specs can be specified.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AllowedTrackTypes? allowed_track_types { get; set; }

        /// <summary>
        /// A finer grained control on what content keys to return. 
        /// Only one of allowed_track_types and content_key_specs can be specified.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ContentKeySpecs[] content_key_specs { get; set; }

        /// <summary>
        /// Policy settings for this license. In the event this asset has 
        /// a pre-defined policy, these specified values will be used.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object policy_overrides { get; set; }
    }
}
