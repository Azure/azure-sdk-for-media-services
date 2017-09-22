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
    /// The options to create a channel, which contains
    /// all parameters and settings to configure a channel
    /// </summary>
    public class ChannelCreationOptions
    {
        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the channel.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cross site access policies for the channel.
        /// </summary>
        public CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        public ChannelInput Input { get; set; }

        /// <summary>
        /// Gets or sets state of the channel.
        /// </summary>
        public ChannelState State { get; set; }

        /// <summary>
        /// Gets or sets the channel preview properties.
        /// </summary>
        public ChannelPreview Preview { get; set; }

        /// <summary>
        /// Gets or sets the channel output properties.
        /// </summary>
        public ChannelOutput Output { get; set; }

        /// <summary>
        /// Gets or sets the channel encoding properties.
        /// </summary>
        public ChannelEncoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the channel encoding type property.
        /// </summary>
        public ChannelEncodingType EncodingType { get; set; }

        /// <summary>
        /// Gets or sets the channel vanity url property.
        /// </summary>
        public bool VanityUrl { get; set; }

        /// <summary>
        /// Gets or sets the channel slate.
        /// </summary>
        public ChannelSlate Slate { get; set; }

        /// <summary>
        /// Creates an instance of ChannelCreationOptions class.
        /// </summary>
        public ChannelCreationOptions() {}

        /// <summary>
        /// Creates an instance of ChannelCreationOptions class.
        /// </summary>
        /// <param name="name">Name of the channel to be created</param>
        /// <param name="inputStreamingProtocol">the Streaming Protocol of the channel input</param>
        /// <param name="inputIPAllowList">the IP allow list for the channel input access control</param>
        public ChannelCreationOptions(
            string name, 
            StreamingProtocol inputStreamingProtocol,
            IEnumerable<IPRange> inputIPAllowList)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }
            if (inputIPAllowList == null)
            {
                throw new ArgumentNullException("inputIPAllowList");
            }

            Name = name;
            Input = new ChannelInput
            {
                StreamingProtocol = inputStreamingProtocol,
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = (inputIPAllowList as IList<IPRange>) ?? inputIPAllowList.ToList()
                }
            };
        }
    }
}
