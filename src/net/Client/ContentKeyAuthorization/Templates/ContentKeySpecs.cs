//-----------------------------------------------------------------------
// <copyright file="ContentKeySpecs.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.MediaServices.Client.Widevine
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Hdcp
    {
        HDCP_NONE,
        HDCP_V1,
        HDCP_V2
    }

    public class RequiredOutputProtection
    {
        /// <summary>
        /// Indicates whether HDCP is required.
        /// </summary>
        public Hdcp hdcp { get; set; }
    }

    /// <summary>
    /// See Widevine Modular DRM Proxy Integration documentation.
    /// </summary>
    public class ContentKeySpecs
    {
        /// <summary>
        /// A track type name.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string track_type { get; set; }

        /// <summary>
        /// Unique identifier for the key.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string key_id { get; set; }

        /// <summary>
        /// Defines client robustness requirements for playback.
        /// 1 - Software-based whitebox crypto is required.
        /// 2 - Software crypto and an obfuscated decoder is required.
        /// 3 - The key material and crypto operations must be performed 
        /// within a hardware backed trusted execution environment.
        /// 4 - The crypto and decoding of content must be performed within 
        /// a hardware backed trusted execution environment.
        /// 5 - The crypto, decoding and all handling of the media (compressed 
        /// and uncompressed) must be handled within a hardware backed trusted 
        /// execution environment.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? security_level { get; set; }

        /// <summary>
        /// Indicates whether HDCP V1 or V2 is required or not.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RequiredOutputProtection required_output_protection { get; set; }
    }
}
