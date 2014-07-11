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
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Live channel input.
    /// </summary>
    internal class ChannelInput : IChannelInput
    {
        /// <summary>
        /// Gets or sets Key Frame Distance HNS. MinValue = 19000000, MaxValue = 61000000
        /// </summary>
        public long? KeyFrameDistanceHns { get; set; }

        /// <summary>
        /// Gets or sets the streaming protocol (for REST).
        /// </summary>
        public string StreamingProtocol { get; set; }

        /// <summary>
        /// Get or sets the streaming protocol.
        /// </summary>
        StreamingProtocol IChannelInput.StreamingProtocol
        {
            get { return (StreamingProtocol) Enum.Parse(typeof (StreamingProtocol), StreamingProtocol, true); }
            set { StreamingProtocol = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets channel input access control (for REST)
        /// </summary>
        public ChannelServiceAccessControl AccessControl { get; set; }

        /// <summary>
        /// Get or sets channel input access control.
        /// </summary>
        ChannelAccessControl IChannelInput.AccessControl
        {
            get { return (ChannelAccessControl) AccessControl; }
            set { AccessControl = new ChannelServiceAccessControl(value); }
        }

        /// <summary>
        /// Gets the list of the channel input endpoints.
        /// </summary>
        public IList<ChannelServiceEndpoint> Endpoints { get; set; }
        
        /// <summary>
        /// Gets the <see cref="ChannelEndpoint"/> object containing the ingest metrics of the channel.
        /// </summary>
        ReadOnlyCollection<ChannelEndpoint> IChannelInput.Endpoints
        {
            get { return Endpoints.Select(e => ((ChannelEndpoint) e)).ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Creates an instance of ChannelInput class.
        /// </summary>
        public ChannelInput() { }

        /// <summary>
        /// Creates an instance of ChannelInput class from an instance of IChannelInput.
        /// </summary>
        /// <param name="input">Channel Input to copy into newly created instance.</param>
        public ChannelInput(IChannelInput input)
        {
            if (input == null) return;

            KeyFrameDistanceHns = input.KeyFrameDistanceHns;
            StreamingProtocol = input.StreamingProtocol.ToString();
            AccessControl = new ChannelServiceAccessControl(input.AccessControl);
        }
    }
}
