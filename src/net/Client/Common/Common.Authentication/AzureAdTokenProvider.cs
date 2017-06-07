//-----------------------------------------------------------------------
// <copyright file="AzureAadTokenProvider.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// An AAD token provider for authorization tokens.
    /// </summary>
    public class AzureAdTokenProvider : ITokenProvider
    {
        private readonly AuthenticationContext _authenticationContext;
        private readonly AzureAdTokenCredentials _tokenCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAdTokenProvider"/> class.
        /// </summary>
        /// <param name="tokenCredentials">The token credentials.</param>
        public AzureAdTokenProvider(AzureAdTokenCredentials tokenCredentials)
        {
            if (tokenCredentials == null)
            {
                throw new ArgumentNullException("tokenCredentials");
            }

            _tokenCredentials = tokenCredentials;

            
            var authority = string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}",
                CanonicalizeUri(_tokenCredentials.AzureEnvironment.ActiveDirectoryEndpoint.ToString()),
                tokenCredentials.Tenant);

            _authenticationContext = new AuthenticationContext(authority);
        }

        /// <summary>
        /// Gets a value for the Authorization header in RFC6750 format.
        /// </summary>
        /// <returns>The authorization header.</returns>
        public string GetAuthorizationHeader()
        {
            var result = GetToken();
            return result.CreateAuthorizationHeader();
        }

        /// <summary>
        /// Gets the access token to use.
        /// </summary>
        /// <returns>A tuple containing access token and its expiration time.</returns>
        public Tuple<string, DateTimeOffset> GetAccessToken()
        {
            var result = GetToken();
            return new Tuple<string, DateTimeOffset>(result.AccessToken, result.ExpiresOn);
        }

        private AuthenticationResult GetToken()
        {
            var mediaServicesResource = _tokenCredentials.AzureEnvironment.MediaServicesResource;

            switch (_tokenCredentials.CredentialType)
            {
                case AzureAdTokenCredentialType.UserCredential:
                    return _authenticationContext.AcquireTokenAsync(
                        mediaServicesResource,
                        _tokenCredentials.AzureEnvironment.MediaServicesSdkClientId,
                        _tokenCredentials.AzureEnvironment.MediaServicesSdkRedirectUri,
                        new PlatformParameters(PromptBehavior.Auto)).Result;

                case AzureAdTokenCredentialType.ServicePrincipalWithClientSymmetricKey:
                    return  _authenticationContext.AcquireTokenAsync(mediaServicesResource, _tokenCredentials.ClientKey).Result;

                case AzureAdTokenCredentialType.ServicePrincipalWithClientCertificate:
                    return _authenticationContext.AcquireTokenAsync(mediaServicesResource, _tokenCredentials.ClientCertificate).Result;

                default:
                    throw new NotSupportedException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Token Credential type {0} is not supported.",
                            _tokenCredentials.CredentialType));
            }
        }

        private static string CanonicalizeUri(string uri)
        {
            if (!string.IsNullOrWhiteSpace(uri) && !uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                uri = uri + "/";
            }

            return uri;
        }
    }
}