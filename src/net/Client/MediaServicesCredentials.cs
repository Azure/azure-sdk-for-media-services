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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class MediaServicesCredentials
    {
        // Token related constants
        private static readonly DateTime TokenBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private const char parameterSeparator = '&';
        private const char nameValueSeparator = '=';
        private const string ExpiresOnLabel = "ExpiresOn";

        // ACS related constants
        private static readonly Uri _mediaServicesAcsBaseAddress = new Uri("https://wamsprodglobal001acs.accesscontrol.windows.net");
        private const string MediaServicesAccessScope = "urn:WindowsAzureMediaServices";
        private const string AuthorizationHeader = "Authorization";
        private const string BearerTokenFormat = "Bearer {0}";
        private const string GrantType = "client_credentials";

        /// <summary>
        /// The access control endpoint to authenticate against.
        /// </summary>
        public string AcsBaseAddress {get; set; }

        /// <summary>
        /// The Microsoft WindowsAzure Media Services account key to authenticate with.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The Microsoft WindowsAzure Media Services account name to authenticate with.
        /// </summary>
        public string ClientId { get; set; }
        
        /// <summary>
        /// The scope of authorization.
        /// </summary>
        public string Scope { get; set; }

        /// <summary> 
        /// Gets OAuth Access Token to be used for web requests.
        /// </summary> 
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets Expiration time in UTC of OAuth Access Token used in web requests.
        /// </summary>
        public DateTime TokenExpiration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="scope">The scope.</param>
        public MediaServicesCredentials(string clientId, string clientSecret)
            : this(clientId, clientSecret, MediaServicesAccessScope, _mediaServicesAcsBaseAddress.AbsoluteUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthDataServiceAdapter"/> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="acsBaseAddress">The acs base address.</param>
        public MediaServicesCredentials(string clientId, string clientSecret, string scope, string acsBaseAddress)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.Scope = scope;
            this.AcsBaseAddress = acsBaseAddress;
        }

        /// <summary>
        /// Requests ACS token from the server and stores it for future use.
        /// </summary>
        public void RefreshToken()
        {
            using (WebClient client = new WebClient())
            {
                client.BaseAddress = this.AcsBaseAddress;

                var oauthRequestValues = new NameValueCollection
                {
                    {"grant_type", GrantType},
                    {"client_id", this.ClientId},
                    {"client_secret", this.ClientSecret},
                    {"scope", this.Scope},
                };

                RetryPolicy retryPolicy = new RetryPolicy(new WebRequestTransientErrorDetectionStrategy(), RetryStrategyFactory.DefaultStrategy());

                retryPolicy.ExecuteAction(
                    () =>
                    {
                        byte[] responseBytes = client.UploadValues("/v2/OAuth2-13", "POST", oauthRequestValues);
                        SetAcsToken(responseBytes);
                    });
            }
        }

        /// <summary>
        /// Stores ACS token info for future use.
        /// </summary>
        /// <param name="acsResponse">Response received from ACS server.</param>
        public void SetAcsToken(byte[] acsResponse)
        {
            using (var responseStream = new MemoryStream(acsResponse))
            {
                OAuth2TokenResponse tokenResponse = (OAuth2TokenResponse)new DataContractJsonSerializer(typeof(OAuth2TokenResponse)).ReadObject(responseStream);
                this.AccessToken = tokenResponse.AccessToken;
                this.TokenExpiration = ParseTokenExpiration(tokenResponse.AccessToken);
            }
        }

        private static DateTime DecodeExpiry(string expiry)
        {
            long totalSeconds;
            if (!long.TryParse(expiry, out totalSeconds))
            {
                return DateTime.MinValue;
            }

            long maxSeconds = (long)(DateTime.MaxValue - TokenBaseTime).TotalSeconds - 1;
            if (totalSeconds > maxSeconds)
            {
                totalSeconds = maxSeconds;
            }

            return TokenBaseTime + TimeSpan.FromSeconds(totalSeconds);
        }

        public static DateTime ParseTokenExpiration(string token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "token"));
            }

            string expireOnValue = null;

            foreach (string nameValue in token.Split(parameterSeparator))
            {
                string[] keyValueArray = nameValue.Split(nameValueSeparator);

                string key = HttpUtility.UrlDecode(keyValueArray[0].Trim());

                if (0 == String.Compare(key, ExpiresOnLabel, StringComparison.OrdinalIgnoreCase))
                {
                    // Names must be decoded for the claim type case
                    expireOnValue = HttpUtility.UrlDecode(keyValueArray[1].Trim().Trim('"')); // remove any unwanted " 
                    break;
                }
            }

            if (!String.IsNullOrWhiteSpace(expireOnValue))
            {
                return DecodeExpiry(expireOnValue);
            }
            else
            {
                throw new ArgumentException(StringTable.UnableToParseExpirationFromToken, "token");
            }
        }
    }
}
