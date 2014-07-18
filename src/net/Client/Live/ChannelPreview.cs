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
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies channel preview settings.
    /// </summary>
    public class ChannelPreview
    {
        /// <summary>
        /// Gets or sets channel preview access control
        /// </summary>
        public ChannelAccessControl AccessControl { get; set; }

        /// <summary>
        /// Gets the list of the channel preview endpoints.
        /// </summary>
        public ReadOnlyCollection<ChannelEndpoint> Endpoints { get; internal set; }
    }

    /// <summary>
    /// Specifies channel preview settings.
    /// </summary>
    internal class ChannelServicePreview
    {
        /// <summary>
        /// Gets or sets channel preview access control (for REST)
        /// </summary>
        public ChannelServiceAccessControl AccessControl { get; set; }
        
        /// <summary>
        /// Gets the list of the channel preview endpoints.
        /// </summary>
        public List<ChannelServiceEndpoint> Endpoints { get; set; }

        /// <summary>
        /// Creates an instance of ChannelServicePreview class.
        /// </summary>
        public ChannelServicePreview() { }

        /// <summary>
        /// Creates an instance of ChannelServicePreview class from an instance of ChannelPreview.
        /// </summary>
        /// <param name="preview">Channel Preview to copy into newly created instance.</param>
        public ChannelServicePreview(ChannelPreview preview)
        {
            if (preview == null)
            {
                throw new ArgumentNullException("preview");
            }

            AccessControl = preview.AccessControl == null
                ? null
                : new ChannelServiceAccessControl(preview.AccessControl);

            if (preview.Endpoints != null)
            {
                Endpoints = new List<ChannelServiceEndpoint>(preview.Endpoints.Count);
                foreach (var endpoint in preview.Endpoints)
                {
                    Endpoints.Add(endpoint == null ? null : new ChannelServiceEndpoint(endpoint));
                }
            }
        }

        /// <summary>
        /// Casts ChannelServicePreview to ChannelPreview.
        /// </summary>
        /// <param name="preview">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelPreview(ChannelServicePreview preview)
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
