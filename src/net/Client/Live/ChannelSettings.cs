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
namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Live channel settings.
    /// </summary>
    public class ChannelSettings 
    {
        /// <summary>
        /// Gets or sets preview settings.
        /// </summary>
        public PreviewEndpointSettings Preview { get; set; }

        /// <summary>
        /// Gets or sets ingest settings.
        /// </summary>
        public IngestEndpointSettings Ingest { get; set; }

        /// <summary>
        /// Gets or sets input settings
        /// </summary>
        public InputSettings Input { get; set; }

        /// <summary>
        /// Gets or sets output settings
        /// </summary>
        public OutputSettings Output { get; set; }


		/// <summary>
		/// Gets or sets client access policy.
		/// </summary>
		public CrossSiteAccessPolicy ClientAccessPolicy { get; set; }

		/// <summary>
		/// Gets or sets cross domain access policy.
		/// </summary>
		public CrossSiteAccessPolicy CrossDomainPolicy { get; set; }
	}

    /// <summary>
    /// Describes Preview endpoint settings.
    /// </summary>
    public class PreviewEndpointSettings
    {
        /// <summary>
        /// Gets or sets security settings.
        /// </summary>
        public PreviewEndpointSecuritySettings Security { get; set; }
    }

    /// <summary>
    /// Describes Preview endpoint security settings.
    /// </summary>
    public class PreviewEndpointSecuritySettings
    {
        /// <summary>
        /// Gets or sets the list of IP-s allowed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Ipv4> IPv4AllowList { get; set; }
    }

    /// <summary>
    /// Describes Ingest endpoint settings.
    /// </summary>
    public class IngestEndpointSettings
    {
        /// <summary>
        /// Gets or sets security settings.
        /// </summary>
        public IngestEndpointSecuritySettings Security { get; set; }
    }

    /// <summary>
    /// Describes Ingest endpoint security settings.
    /// </summary>
    public class IngestEndpointSecuritySettings
    {
        /// <summary>
        /// Gets or sets the list of IP-s allowed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Ipv4> IPv4AllowList { get; set; }
    }

    /// <summary>
    /// Describes Channel input settings
    /// </summary>
    public class InputSettings
    {
        /// <summary>
        /// Gets or sets FMp4 fragment duration
        /// </summary>
        public TimeSpan? FMp4FragmentDuration { get; set; }
    }

    /// <summary>
    /// Describes Channel output settings
    /// </summary>
    public class OutputSettings
    {
        /// <summary>
        /// Gets or sets the number of fragments per HLS segment
        /// </summary>
        public short? FragmentsPerHlsSegment { get; set; }
    }
}
