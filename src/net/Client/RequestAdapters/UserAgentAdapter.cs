//-----------------------------------------------------------------------
// <copyright file="UserAgentAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// UserAgentAdapter is used to add user agent specific information to http request. 
    /// </summary>
    public class UserAgentAdapter : IDataServiceContextAdapter
    {
       
        private const string _userAgentPrefix = "Azure Media Services .NET SDK v";
        private readonly Version _clientVersion;
        private readonly string _userAgentHeaderValue; 
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVersionAdapter"/> class.
        /// </summary>
        /// <param name="clientVersion">The service version.</param>
        public UserAgentAdapter(Version clientVersion)
        {
            this._clientVersion = clientVersion;
            _userAgentHeaderValue = GetUserAgentString();
        }

        /// <summary>
        /// Adapts the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Adapt(DataServiceContext context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }
            context.SendingRequest2 += this.AddRequestUserAgent;
        }

        /// <summary>
        /// Adds the version to request.
        /// </summary>
        /// <param name="request">The request.</param>
        public void AddUserAgentToRequest(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            //User agent can't be set through header collection in HttpWebRequest
            HttpWebRequest httpWebRequest = request as HttpWebRequest;
            if (httpWebRequest != null)
            {
                httpWebRequest.UserAgent = _userAgentHeaderValue;
            }
            else
            {
                request.Headers.Set(HttpRequestHeader.UserAgent, _userAgentHeaderValue);
            }

        }

        /// <summary>
        /// Adds user agent.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.Services.Client.SendingRequestEventArgs" /> instance containing the event data.</param>
        private void AddRequestUserAgent(object sender, SendingRequest2EventArgs e)
        {
            e.RequestMessage.SetHeader(HttpRequestHeader.UserAgent.ToString(), _userAgentHeaderValue);
        }
        
        private string GetUserAgentString()
        {
            return _userAgentPrefix + this._clientVersion.ToString();
        }
    }
    
}