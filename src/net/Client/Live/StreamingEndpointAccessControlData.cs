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
    /// Describes Streaming Endpoint Ingest access control.
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    internal class StreamingEndpointAccessControlData
    {
        /// <summary>
        /// Gets or sets the list of IP-s allowed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<IPAddressData> IPAllowList { get; set; }

        /// <summary>
        /// Gets or sets the list of Akamai Signature Header Authentication keys.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<AkamaiSignatureHeaderAuthenticationKey> AkamaiSignatureHeaderAuthenticationKeyList { get; set; }

        /// <summary>
        /// Creates an instance of StreamingEndpointAccessControlData class.
        /// </summary>
        public StreamingEndpointAccessControlData() { }

        /// <summary>
        /// Creates an instance of StreamingEndpointAccessControlData class from an instance of StreamingEndpointAccessControl.
        /// </summary>
        /// <param name="accessControl">streaming endpoint access control to copy into newly created instance.</param>
        public StreamingEndpointAccessControlData(StreamingEndpointAccessControl accessControl)
        {
            if (accessControl == null)
            {
                throw new ArgumentNullException("accessControl");
            }

            if (accessControl.AkamaiSignatureHeaderAuthenticationKeyList != null)
            {
                AkamaiSignatureHeaderAuthenticationKeyList =
                    accessControl.AkamaiSignatureHeaderAuthenticationKeyList.ToList();
            }

            if (accessControl.IPAllowList != null)
            {
                IPAllowList = accessControl.IPAllowList
                    .Select(a => a == null ? null : new IPAddressData(a))
                    .ToList();
            }
        }

        /// <summary>
        /// Casts StreamingEndpointAccessControlData to StreamingEndpointAccessControl.
        /// </summary>
        /// <param name="accessControl">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator StreamingEndpointAccessControl(StreamingEndpointAccessControlData accessControl)
        {
            if (accessControl == null)
            {
                return null;
            }

            var result = new StreamingEndpointAccessControl
            {
                AkamaiSignatureHeaderAuthenticationKeyList = accessControl.AkamaiSignatureHeaderAuthenticationKeyList
            };

            if (accessControl.IPAllowList != null)
            {
                result.IPAllowList = accessControl.IPAllowList
                    .Select(a => (IPAddress)a)
                    .ToList();
            }

            return result;
        }
    }
}
