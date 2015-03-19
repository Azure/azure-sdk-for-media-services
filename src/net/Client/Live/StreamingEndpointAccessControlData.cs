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
        /// Gets or sets the Akamai access control.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public AkamaiAccessControlData Akamai { get; set; }
        
        /// <summary>
        /// Gets or sets the IP access control.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IPAccessControlData IP { get; set; }

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
                Akamai = new AkamaiAccessControlData
                {
                    AkamaiSignatureHeaderAuthenticationKeyList =
                        accessControl.AkamaiSignatureHeaderAuthenticationKeyList.ToList()

                };
            }

            if (accessControl.IPAllowList != null)
            {
                IP = new IPAccessControlData
                {
                    Allow = accessControl.IPAllowList
                        .Select(a => a == null ? null : new IPRangeData(a))
                        .ToList()
                };
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

            var result = new StreamingEndpointAccessControl();

            if (accessControl.Akamai != null)
            {
                result.AkamaiSignatureHeaderAuthenticationKeyList =
                    accessControl.Akamai.AkamaiSignatureHeaderAuthenticationKeyList;
            }

            if (accessControl.IP != null && accessControl.IP.Allow != null)
            {
                result.IPAllowList = accessControl.IP.Allow
                    .Select(a => (IPRange)a)
                    .ToList();
            }

            return result;
        }
    }
}
