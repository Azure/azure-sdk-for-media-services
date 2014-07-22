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

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Streaming Endpoint Ingest access control.
    /// This is the public class exposed to SDK interfaces and used by users
    /// </summary>
    public class StreamingEndpointAccessControl
    {
        /// <summary>
        /// Gets or sets the list of IP-s allowed.
        /// This is the public class exposed to SDK interfaces and used by users
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IPRange> IPAllowList { get; set; }

        /// <summary>
        /// Gets or sets the list of Akamai Signature Header Authentication keys.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<AkamaiSignatureHeaderAuthenticationKey> AkamaiSignatureHeaderAuthenticationKeyList { get; set; }
    }
}
