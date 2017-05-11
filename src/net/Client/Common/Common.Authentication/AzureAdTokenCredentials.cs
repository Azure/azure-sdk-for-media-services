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
        /// Gets the client ID.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Gets the client secret.
        /// </summary>
        public string ClientSecret { get; }

        /// <summary>
        /// Gets the credential type.
        /// </summary>
        public AzureAdTokenCredentialType CredentialType { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public AzureEnvironment AzureEnvironment { get; }

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
        /// <param name="clientId">The client ID.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="azureEnvironment">The environment.</param>
        public AzureAdTokenCredentials(string tenant, string clientId, string clientSecret, AzureEnvironment azureEnvironment)
        {
            if (string.IsNullOrWhiteSpace(tenant))
            {
                throw new ArgumentException("tenant");
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("clientId");
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("clientSecret");
            }

            if (azureEnvironment == null)
            {
                throw new ArgumentNullException("azureEnvironment");
            }

            Tenant = tenant;
            ClientId = clientId;
            ClientSecret = clientSecret;
            AzureEnvironment = azureEnvironment;
            CredentialType = AzureAdTokenCredentialType.ServicePrincipal;
        }
    }
}