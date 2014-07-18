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
    /// This is the public class exposed to SDK interfaces and used by users
    /// </summary>
    public class ChannelInput
    {
        /// <summary>
        /// Gets or sets Key Frame Distance HNS. MinValue = 19000000, MaxValue = 61000000
        /// </summary>
        public long? KeyFrameDistanceHns { get; set; }

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

    /// <summary>
    /// Describes Live channel input.
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    internal class ChannelServiceInput
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
        /// Gets or sets channel input access control (for REST)
        /// </summary>
        public ChannelServiceAccessControl AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel input endpoints.
        /// </summary>
        public List<ChannelServiceEndpoint> Endpoints { get; set; }
        
        /// <summary>
        /// Creates an instance of ChannelServiceInput class.
        /// </summary>
        public ChannelServiceInput() { }

        /// <summary>
        /// Creates an instance of ChannelServiceInput class from an instance of ChannelInput.
        /// </summary>
        /// <param name="input">Channel Input to copy into newly created instance.</param>
        public ChannelServiceInput(ChannelInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");   
            }

            KeyFrameDistanceHns = input.KeyFrameDistanceHns;
            StreamingProtocol = input.StreamingProtocol.ToString();
            AccessControl = input.AccessControl == null
                ? null
                : new ChannelServiceAccessControl(input.AccessControl);

            if (input.Endpoints != null)
            {
                Endpoints = new List<ChannelServiceEndpoint>(input.Endpoints.Count);
                foreach (var endpoint in input.Endpoints)
                {
                    Endpoints.Add(endpoint == null ? null : new ChannelServiceEndpoint(endpoint));
                }
            }
        }
        
        /// <summary>
        /// Casts ChannelServiceInput to ChannelInput.
        /// </summary>
        /// <param name="input">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelInput(ChannelServiceInput input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ChannelInput
            {
                KeyFrameDistanceHns = input.KeyFrameDistanceHns,
                StreamingProtocol =
                    (StreamingProtocol) Enum.Parse(typeof (StreamingProtocol), input.StreamingProtocol, true),
                AccessControl = (ChannelAccessControl) input.AccessControl
            };

            if (input.Endpoints != null)
            {
                result.Endpoints = input.Endpoints.Select(e => ((ChannelEndpoint) e)).ToList().AsReadOnly();
            }

            return result;
        }
    }
}
