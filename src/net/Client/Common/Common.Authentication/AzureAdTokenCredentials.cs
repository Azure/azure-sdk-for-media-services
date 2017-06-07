//-----------------------------------------------------------------------
// <copyright file="AzureAdTokenCredentials.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the credentials used for issuing an AAD token.
    /// </summary>
    public class AzureAdTokenCredentials
    {
        /// <summary>
        /// Gets the tenant.
        /// </summary>
        public string Tenant { get; }

        /// <summary>
        /// Gets the client symmetric key credential.
        /// </summary>
        public ClientCredential ClientKey { get; }

        /// <summary>
        /// Gets the client certificate credential.
        /// </summary>
        public ClientAssertionCertificate ClientCertificate { get; }
        
        /// <summary>
        /// Gets the environment.
        /// </summary>
        public AzureEnvironment AzureEnvironment { get; }

        /// <summary>
        /// Gets the credential type.
        /// </summary>
        internal AzureAdTokenCredentialType CredentialType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdTokenCredentials"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="azureEnvironment">The environment.</param>
        public AzureAdTokenCredentials(string tenant, AzureEnvironment azureEnvironment)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentException("tenant");
            }

            if (azureEnvironment == null)
            {
                throw new ArgumentNullException("azureEnvironment");
            }

            Tenant = tenant;
            AzureEnvironment = azureEnvironment;
            CredentialType = AzureAdTokenCredentialType.UserCredential;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdTokenCredentials"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="clientSymmetricKey">The client symmetric key.</param>
        /// <param name="azureEnvironment">The environment.</param>
        public AzureAdTokenCredentials(string tenant, AzureAdClientSymmetricKey clientSymmetricKey, AzureEnvironment azureEnvironment)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentException("tenant");
            }

            if (clientSymmetricKey == null)
            {
                throw new ArgumentNullException("clientSymmetricKey");
            }

            if (azureEnvironment == null)
            {
                throw new ArgumentNullException("azureEnvironment");
            }

            Tenant = tenant;
            ClientKey = new ClientCredential(clientSymmetricKey.ClientId, clientSymmetricKey.ClientKey);
            AzureEnvironment = azureEnvironment;
            CredentialType = AzureAdTokenCredentialType.ServicePrincipalWithClientSymmetricKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdTokenCredentials"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        /// <param name="clientCertificate">The client certificate.</param>
        /// <param name="azureEnvironment">The environment.</param>
        public AzureAdTokenCredentials(string tenant, AzureAdClientCertificate clientCertificate, AzureEnvironment azureEnvironment)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentException("tenant");
            }

            if (clientCertificate == null)
            {
                throw new ArgumentNullException("clientCertificate");
            }

            if (azureEnvironment == null)
            {
                throw new ArgumentNullException("azureEnvironment");
            }

            var cert = EncryptionUtils.GetCertificateFromStore(clientCertificate.ClientCertificateThumbprint);
            if (cert == null)
            {
                throw new ArgumentException("Invalid ClientCertificateThumbprint in clientCertificate specified");
            }

            Tenant = tenant;
            ClientCertificate = new ClientAssertionCertificate(clientCertificate.ClientId, cert);
            AzureEnvironment = azureEnvironment;
            CredentialType = AzureAdTokenCredentialType.ServicePrincipalWithClientCertificate;
        }
    }
}