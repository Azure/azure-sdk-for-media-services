// Copyright 2015 Microsoft Corporation
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
    /// An entity class for storing the slate properties of a channel.
    /// </summary>
    public class ChannelSlate
    {
        /// <summary>
        /// Indicates whether or not slate is inserted automatically on Ad marker.
        /// </summary>
        public bool InsertSlateOnAdMarker { get; set; }

        /// <summary>
        /// The Id of the Asset to be used for default slate image.
        /// </summary>
        public string DefaultSlateAssetId { get; set; }
    }
}
