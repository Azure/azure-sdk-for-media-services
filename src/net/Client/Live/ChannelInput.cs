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
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Live channel input.
    /// This is the public class exposed to SDK interfaces and used by users
    /// </summary>
    public class ChannelInput
    {
        /// <summary>
        /// Gets or sets Key Frame Distance HNS. MinValue = PT1.9S, MaxValue = PT6.1S
        /// </summary>
        public TimeSpan? KeyFrameInterval { get; set; }

        /// <summary>
        /// Gets or sets the channel input streaming protocol.
        /// </summary>
        public StreamingProtocol StreamingProtocol { get; set; }

        /// <summary>
        /// Gets or sets channel input access control
        /// </summary>
        public ChannelAccessControl AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel input endpoints.
        /// </summary>
        public ReadOnlyCollection<ChannelEndpoint> Endpoints { get; internal set; }
    }
}
