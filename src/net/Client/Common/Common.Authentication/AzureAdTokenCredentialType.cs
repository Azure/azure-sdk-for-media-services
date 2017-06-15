//-----------------------------------------------------------------------
// <copyright file="AzureAdTokenCredentialType.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the AAD token crendential type.
    /// </summary>
    internal enum AzureAdTokenCredentialType
    {
        /// <summary>
        /// User Credential by prompting user for user name and password.
        /// </summary>
        UserCredential,

        /// <summary>
        /// Service Principal with the symmetric key credential.
        /// </summary>
        ServicePrincipalWithClientSymmetricKey,

        /// <summary>
        /// Service Principal with the certificate credential.
        /// </summary>
        ServicePrincipalWithClientCertificate
    }
}