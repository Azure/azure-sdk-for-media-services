//-----------------------------------------------------------------------
// <copyright file="OAuthDataServiceAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Specialized;
using System.Data.Services.Client;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.OAuth
{

    /// <summary>
    /// An OAuth adapter for a data service.
    /// </summary>
    public class OAuthDataServiceAdapter
    {
        private const string AuthorizationHeader = "Authorization";
        private const string BearerTokenFormat = "Bearer {0}";

        private readonly MediaServicesCredentials _credentials;
        private readonly object _acsRefreshLock = new object();
        private readonly string _trustedRestCertificateHash;
        private readonly string _trustedRestSubject;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="credentials">Microsoft WindowsAzure Media Services credentials.</param>
        /// <param name="trustedRestCertificateHash">The trusted rest certificate hash.</param>
        /// <param name="trustedRestSubject">The trusted rest subject.</param>
        public OAuthDataServiceAdapter(MediaServicesCredentials credentials, string trustedRestCertificateHash, string trustedRestSubject)
        {
            this._credentials = credentials;
            this._trustedRestCertificateHash = trustedRestCertificateHash;
            this._trustedRestSubject = trustedRestSubject;

            #if DEBUG
            ServicePointManager.ServerCertificateValidationCallback = this.ValidateCertificate;
            #endif
        }

        /// <summary>
        /// Adapts the specified data service context.
        /// </summary>
        /// <param name="dataServiceContext">The data service context.</param>
        public void Adapt(DataServiceContext dataServiceContext)
        {
            dataServiceContext.SendingRequest += this.OnSendingRequest;
        }

        /// <summary>
        /// Adds the access token to request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void AddAccessTokenToRequest(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers[AuthorizationHeader] == null)
            {
                lock (this._acsRefreshLock)
                {
                    if (DateTime.UtcNow > this._credentials.TokenExpiration)
                    {
                        this._credentials.RefreshToken();
                    }
                }

                request.Headers.Add(AuthorizationHeader, string.Format(CultureInfo.InvariantCulture, BearerTokenFormat, this._credentials.AccessToken));
            }
        }

        private bool ValidateCertificate(object s, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            if (error.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch) || error.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {

                // This is for local deployments. DevFabric generates its own certificate for load-balancing / port forwarding.
                const string AzureDevFabricCertificateSubject = "CN=127.0.0.1, O=TESTING ONLY, OU=Windows Azure DevFabric";
                if (cert.Subject == AzureDevFabricCertificateSubject)
                {
                    return true;
                }
                var cert2 = new X509Certificate2(cert);
                if (this._trustedRestSubject == cert2.Subject && cert2.Thumbprint == this._trustedRestCertificateHash)
                {
                    return true;
                }
            }

            return error == SslPolicyErrors.None;
        }

        /// <summary> 
        /// When sending Http Data requests to the Azure Marketplace, inject authorization header based on the current Access token.
        /// </summary> 
        /// <param name="sender">Event sender.</param> 
        /// <param name="e">Event arguments.</param> 
        private void OnSendingRequest(object sender, SendingRequestEventArgs e)
        {
            this.AddAccessTokenToRequest(e.Request);
        }
    }
}
