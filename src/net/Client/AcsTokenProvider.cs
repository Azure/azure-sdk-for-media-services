//-----------------------------------------------------------------------
// <copyright file="AcsTokenProvider.cs" company="Microsoft">Copyright 2016 Microsoft Corporation</copyright>
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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public abstract class AcsTokenProvider : ITokenProvider
    {
        // Token related constants
        private static readonly DateTime TokenBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private const char parameterSeparator = '&';
        private const char nameValueSeparator = '=';
        private const string ExpiresOnLabel = "ExpiresOn";

        // ACS related constants
        private static readonly Uri _mediaServicesAcsBaseAddress1 = new Uri("https://wamsprodglobal001acs.accesscontrol.windows.net");
        private static readonly Uri _mediaServicesAcsBaseAddress2 = new Uri("https://wamsprodglobal002acs.accesscontrol.windows.net");
        protected static IList<string> PublicAcsBaseAddressList = new List<string> {
            _mediaServicesAcsBaseAddress1.AbsoluteUri,
            _mediaServicesAcsBaseAddress2.AbsoluteUri }.AsReadOnly();

        protected const string MediaServicesAccessScope = "urn:WindowsAzureMediaServices";
        private const string BearerTokenFormat = "Bearer {0}";
        private const string GrantType = "client_credentials";
        private List<string> _acsBaseAddressList;
        private Random _random = new Random();
        private readonly static object _acsRefreshLock = new object();
        private const int ExpirationTimeBufferInSeconds = 600;  // The token has an expiration time in hours,
                                                                // so setting the buffer as 10 minutes is safe for
                                                                // the network latency and clock skew.

        /// <summary>
        /// The access control endpoints to authenticate against.
        /// </summary>
        public IList<string> AcsBaseAddressList
        {
            get { return _acsBaseAddressList.AsReadOnly(); }
        }

        /// <summary>
        /// The access control endpoint to authenticate against.
        /// </summary>
        public string AcsBaseAddress
        {
            get { return _acsBaseAddressList[0]; }
            set { _acsBaseAddressList.Clear(); _acsBaseAddressList.Add(value); }
        }

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


        // For an ACS client the ClientId is the media services account name.
        public string MediaServicesAccountName
        {
            get
            {
                return ClientId;
            }
        }

        public AcsTokenProvider(string clientId, string clientSecret, string scope, IList<string> acsBaseAddressList)
        {
            ValidateStringArgumentIsNotNullOrEmpty(clientId, "clientId");
            ValidateStringArgumentIsNotNullOrEmpty(clientSecret, "clientSecret");
            ValidateStringArgumentIsNotNullOrEmpty(scope, "scope");
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scope = scope;
            _acsBaseAddressList = new List<string>(acsBaseAddressList);
            RefreshTokenRetryPolicy = new RetryPolicy(new WebRequestTransientErrorDetectionStrategy(),
                RetryStrategyFactory.DefaultStrategy());
        }

        /// <summary>
        /// Gets OAuth Access Token to be used for web requests.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets Expiration time in UTC of OAuth Access Token used in web requests.
        /// </summary>
        public DateTime TokenExpiration { get; set; }

        /// <summary>
        /// RetryPolicy used for acquiring a token
        /// </summary>
        public RetryPolicy RefreshTokenRetryPolicy { get; set; }


        private static void ValidateStringArgumentIsNotNullOrEmpty(string parameterValue, string parameterName)
        {
            if (String.IsNullOrWhiteSpace(parameterValue))
            {
                string message = String.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, parameterName);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Requests ACS token from the server and stores it for future use.
        /// </summary>
        public void RefreshToken()
        {
            int index = _random.Next(_acsBaseAddressList.Count);
            using (WebClient client = new WebClient())
            {
                var oauthRequestValues = new NameValueCollection
                        {
                            {"grant_type", GrantType},
                            {"client_id", ClientId},
                            {"client_secret", ClientSecret},
                            {"scope", Scope},
                        };
                RefreshTokenRetryPolicy.ExecuteAction(
                    () =>
                    {
                        index = (++index) % (_acsBaseAddressList.Count);
                        client.BaseAddress = _acsBaseAddressList[index];
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
                AccessToken = tokenResponse.AccessToken;
                TokenExpiration = ParseTokenExpiration(tokenResponse.AccessToken);
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
            ValidateStringArgumentIsNotNullOrEmpty(token, "token");

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

            if (!string.IsNullOrWhiteSpace(expireOnValue))
            {
                return DecodeExpiry(expireOnValue);
            }
            else
            {
                throw new ArgumentException(StringTable.UnableToParseExpirationFromToken, "token");
            }
        }

        /// <summary>
        /// Gets the access token to use.
        /// </summary>
        /// <returns>A tuple of access token and its expiration time.</returns>
        public Tuple<string, DateTimeOffset> GetAccessToken()
        {
            lock (_acsRefreshLock)
            {
                if (DateTime.UtcNow.AddSeconds(ExpirationTimeBufferInSeconds) > TokenExpiration)
                {
                    RefreshToken();
                }
            }
            return new Tuple<string, DateTimeOffset>(AccessToken, TokenExpiration);
        }

        public string GetAuthorizationHeader()
        {
            var token = GetAccessToken();
            return string.Format(CultureInfo.InvariantCulture, BearerTokenFormat, token.Item1);
        }
    }
}
