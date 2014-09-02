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
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Live channel input.
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    internal class ChannelInputData
    {
        /// <summary>
        /// Gets or sets Key Frame Distance HNS. MinValue = PT1.9S, MaxValue = PT6.1S
        /// </summary>
        public TimeSpan? KeyFrameInterval { get; set; }

        /// <summary>
        /// Gets or sets the streaming protocol (for REST).
        /// </summary>
        public string StreamingProtocol { get; set; }

        /// <summary>
        /// Gets or sets channel input access control (for REST)
        /// </summary>
        public ChannelAccessControlData AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel input endpoints.
        /// </summary>
        public List<ChannelEndpointData> Endpoints { get; set; }

        /// <summary>
        /// Creates an instance of ChannelInputData class.
        /// </summary>
        public ChannelInputData() { }

        /// <summary>
        /// Creates an instance of ChannelInputData class from an instance of ChannelInput.
        /// </summary>
        /// <param name="input">Channel Input to copy into newly created instance.</param>
        public ChannelInputData(ChannelInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            KeyFrameInterval = input.KeyFrameInterval;
            StreamingProtocol = input.StreamingProtocol.ToString();
            AccessControl = input.AccessControl == null
                ? null
                : new ChannelAccessControlData(input.AccessControl);

            if (input.Endpoints != null)
            {
                Endpoints = input.Endpoints
                    .Select(e => e == null ? null : new ChannelEndpointData(e))
                    .ToList();
            }
        }

        /// <summary>
        /// Casts ChannelInputData to ChannelInput.
        /// </summary>
        /// <param name="input">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelInput(ChannelInputData input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ChannelInput
            {
                KeyFrameInterval = input.KeyFrameInterval,
                StreamingProtocol =
                    (StreamingProtocol)Enum.Parse(typeof(StreamingProtocol), input.StreamingProtocol, true),
                AccessControl = (ChannelAccessControl)input.AccessControl
            };

            if (input.Endpoints != null)
            {
                result.Endpoints = input.Endpoints.Select(e => ((ChannelEndpoint)e)).ToList().AsReadOnly();
            }

            return result;
        }
    }
}
