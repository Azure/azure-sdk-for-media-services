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
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a channel streaming endpoint (ingest or preview)
    /// This is the public class exposed to SDK interfaces and used by users
    /// </summary>
    public class ChannelEndpoint
    {
        /// <summary>
        /// Gets or sets the endpoint protocol
        /// </summary>
        public StreamingProtocol Protocol { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL.
        /// </summary>
        public Uri Url { get; set; }
    }

    /// <summary>
    /// Represents a channel streaming endpoint (ingest or preview)
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    internal class ChannelServiceEndpoint
    {
        /// <summary>
        /// Gets or sets the endpoint protocol
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "OData layer compatibility.")]
        public string Url { get; set; }

        /// <summary>
        /// Casts ChannelServiceEndpoint to ChannelEndpoint.
        /// </summary>
        /// <param name="endpoint">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelEndpoint(ChannelServiceEndpoint endpoint)
        {
            if (endpoint == null) return null;

            return new ChannelEndpoint
            {
                Protocol = (StreamingProtocol) Enum.Parse(typeof (StreamingProtocol), endpoint.Protocol, true),
                Url = new Uri(endpoint.Url)
            };
        }
    }
}
