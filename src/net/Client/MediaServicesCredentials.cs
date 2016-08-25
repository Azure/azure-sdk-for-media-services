//-----------------------------------------------------------------------
// <copyright file="MediaServicesCredentials.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class MediaServicesCredentials : AcsTokenProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        public MediaServicesCredentials(string clientId, string clientSecret)
            : this(clientId, clientSecret, MediaServicesAccessScope, PublicAcsBaseAddressList)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="acsBaseAddress">The acs base address.</param>
        public MediaServicesCredentials(string clientId, string clientSecret, string scope, string acsBaseAddress):
            this(clientId, clientSecret, scope, new List<string> { acsBaseAddress })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="acsBaseAddressList">List of acs base address.</param>
        public MediaServicesCredentials(string clientId, string clientSecret, string scope, IList<string> acsBaseAddressList):
            base(clientId, clientSecret, scope, acsBaseAddressList)
        {
        }
    }
}
