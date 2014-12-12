//-----------------------------------------------------------------------
// <copyright file="ClientRequestIdAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters
{
    /// <summary>
    /// ClientRequestIdAdapter is used to add user agent specific information to http request. 
    /// </summary>
    public class ClientRequestIdAdapter : IWebRequestAdapter, IDataServiceContextAdapter
    {

        private const string XMsClientRequestId = "x-ms-client-request-id";
        private Guid _requestId;
        /// <summary>
        /// Adapts the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Adapt(DataServiceContext context)
        {
            _requestId = Guid.NewGuid();
            context.SendingRequest2 += this.AddClientRequestId;
        }

        /// <summary>
        /// Adds client request id.
        /// </summary>
        /// <param name="request">The request.</param>
        public void AddClientRequestId(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            
            request.Headers.Set(XMsClientRequestId,_requestId.ToString());
           
        }

        /// <summary>
        /// Adds the request version.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.Services.Client.SendingRequestEventArgs"/> instance containing the event data.</param>
        private void AddClientRequestId(object sender, SendingRequest2EventArgs e)
        {
            e.RequestMessage.SetHeader(XMsClientRequestId, _requestId.ToString());
        }

        public void ChangeCurrentRequestId()
        {
            _requestId = Guid.NewGuid();
        }
    } 
}