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
    /// Specifies channel preview settings.
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    internal class ChannelPreviewData
    {
        /// <summary>
        /// Gets or sets channel preview access control (for REST)
        /// </summary>
        public ChannelAccessControlData AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel preview endpoints.
        /// </summary>
        public List<ChannelEndpointData> Endpoints { get; set; }

        /// <summary>
        /// Creates an instance of ChannelPreviewData class.
        /// </summary>
        public ChannelPreviewData() { }

        /// <summary>
        /// Creates an instance of ChannelPreviewData class from an instance of ChannelPreview.
        /// </summary>
        /// <param name="preview">Channel Preview to copy into newly created instance.</param>
        public ChannelPreviewData(ChannelPreview preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException("preview");
            }

            AccessControl = preview.AccessControl == null
                ? null
                : new ChannelAccessControlData(preview.AccessControl);

            if (preview.Endpoints != null)
            {
                Endpoints = preview.Endpoints
                    .Select(e => e == null ? null : new ChannelEndpointData(e))
                    .ToList();
            }
        }

        /// <summary>
        /// Casts ChannelPreviewData to ChannelPreview.
        /// </summary>
        /// <param name="preview">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelPreview(ChannelPreviewData preview)
        {
            if (preview == null)
            {
                return null;
            }

            var result = new ChannelPreview
            {
                AccessControl = (ChannelAccessControl)preview.AccessControl
            };

            if (preview.Endpoints != null)
            {
                result.Endpoints = preview.Endpoints.Select(e => ((ChannelEndpoint)e)).ToList().AsReadOnly();
            }

            return result;
        }
    }
}
