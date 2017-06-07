//-----------------------------------------------------------------------
// <copyright file="AzureAdClientCertificate.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
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
// </license>

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes Azure AD client certificate credential.
    /// </summary>
    public class AzureAdClientCertificate
    {
        /// <summary>
        /// Gets the client ID.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the client certificate thumbprint.
        /// </summary>
        public string ClientCertificateThumbprint { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdClientCertificate"/> class.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientCertificateThumbprint">The client certificate thumbprint.</param>
        public AzureAdClientCertificate(string clientId, string clientCertificateThumbprint)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("clientId");
            }

            if (string.IsNullOrWhiteSpace(clientCertificateThumbprint))
            {
                throw new ArgumentException("clientCertificateThumbprint");
            }

            ClientId = clientId;
            ClientCertificateThumbprint = clientCertificateThumbprint;
        }
    }
}