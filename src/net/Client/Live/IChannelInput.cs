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

using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Interface of the channel input
    /// </summary>
    public interface IChannelInput
    {
        /// <summary>
        /// Gets or sets Key Frame Distance HNS. MinValue = 19000000, MaxValue = 61000000
        /// </summary>
        long? KeyFrameDistanceHns { get; set; }

        /// <summary>
        /// Gets or sets the channel input streaming protocol.
        /// </summary>
        StreamingProtocol StreamingProtocol { get; set; }

        /// <summary>
        /// Gets or sets channel input access control
        /// </summary>
        ChannelAccessControl AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel input endpoints.
        /// </summary>
        ReadOnlyCollection<ChannelEndpoint> Endpoints { get; }
    }
}
