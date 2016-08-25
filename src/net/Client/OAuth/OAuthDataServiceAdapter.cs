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
using System.Data.Services.Client;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;

namespace Microsoft.WindowsAzure.MediaServices.Client.OAuth
{

    /// <summary>
    /// An OAuth adapter for a data service.
    /// </summary>
    public class OAuthDataServiceAdapter : IDataServiceContextAdapter
    {
        private const string AuthorizationHeader = "Authorization";
        private readonly ITokenProvider _tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="credentials">Microsoft WindowsAzure Media Services credentials.</param>
        /// <param name="trustedRestCertificateHash">The trusted rest certificate hash.</param>
        /// <param name="trustedRestSubject">The trusted rest subject.</param>
        [Obsolete("Use the constructor which accepts an ITokenProvider interface")]
        public OAuthDataServiceAdapter(MediaServicesCredentials credentials, string trustedRestCertificateHash, string trustedRestSubject):
            this(credentials)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="tokenProvider">Azure Media Services token provider.</param>
        public OAuthDataServiceAdapter(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Adapts the specified data service context.
        /// </summary>
        /// <param name="dataServiceContext">The data service context.</param>
        public void Adapt(DataServiceContext dataServiceContext)
        {
            if (dataServiceContext == null) { throw new ArgumentNullException("dataServiceContext"); }
            dataServiceContext.SendingRequest2 += this.OnSendingRequest;
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
                request.Headers.Add(AuthorizationHeader, _tokenProvider.GetAuthorizationHeader());
            }
        }

        /// <summary>
        /// When sending Http Data requests to the Azure Marketplace, inject authorization header based on the current Access token.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSendingRequest(object sender, SendingRequest2EventArgs e)
        {
            if (e.RequestMessage.GetHeader(AuthorizationHeader) == null)
            {
                e.RequestMessage.SetHeader(AuthorizationHeader, _tokenProvider.GetAuthorizationHeader());
            }
        }
    }
}
