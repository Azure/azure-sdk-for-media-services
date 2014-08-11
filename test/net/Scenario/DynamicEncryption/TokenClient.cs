//-----------------------------------------------------------------------
// <copyright file="TokenServiceClient.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.DynamicEncryption
{
    public class TokenServiceClient
    {
        private const string AccessRequest = @"<TokenRestriction issuer='http://testacs.com' 
                                audience = 'urn:test' >
                            <VerificationKeys>
                                    <VerificationKey type='Symmetric' value='IRPQMJ006zlzV/Y1gbyoKJPKwLGOCAO7M5/17gfh4XU=' 
                                    IsPrimary='true' /> 
                            </VerificationKeys> 
                                <RequiredClaims> 
                                <Claim type='urn:microsoft:azure:mediaservices:contentkeyidentifier' /> 
                                </RequiredClaims> 
                            </TokenRestriction> ";
        private const string TestScope = "urn:test";
        private const string TestIssuerEndpoint = "http://testacs.com";
        private const string SignaturePrefix = "&HMACSHA256=";
        private const string SymmetricKeyString = "IRPQMJ006zlzV/Y1gbyoKJPKwLGOCAO7M5/17gfh4XU=";

        private const string Template =
            @"urn:microsoft:azure:mediaservices:contentkeyidentifier=CONTENTKEY&urn%3aServiceAccessible=service&http%3a%2f%2fschemas.microsoft.com%2faccesscontrolservice%2f2010%2f07%2fclaims%2fidentityprovider=https%3a%2f%2fnimbuslkgglobacs.accesscontrol.windows.net%2f&Audience=SCOPE&ExpiresOn=EXPIRY&Issuer=ISSUER";

        private static readonly DateTime SwtBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private static readonly byte[] SymmetricKeyBytes = Convert.FromBase64String(SymmetricKeyString);

        private TokenServiceClient()
        {
        }

        public static string GetConfiguration()
        {
            return AccessRequest;
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "key")]
        public static string GetAuthTokenForKey(string rawContentKeyId)
        {
            if (rawContentKeyId == null)
            {
                throw new ArgumentNullException("rawContentKeyId");
            }

            string unsignedToken = Template.Replace("SCOPE", HttpUtility.UrlEncode(TestScope));
            unsignedToken = unsignedToken.Replace("EXPIRY", GenerateTokenExpiry(DateTime.UtcNow.AddDays(10)));
            unsignedToken = unsignedToken.Replace("ISSUER", TestIssuerEndpoint);
            unsignedToken = unsignedToken.Replace("CONTENTKEY", rawContentKeyId);
            unsignedToken = unsignedToken.Replace("SYMKEY", SymmetricKeyString);

           
            string signature = String.Empty;
            using (var signatureAlgorithm = new HMACSHA256(SymmetricKeyBytes))
            {
                signature = HttpUtility.UrlEncode(Convert.ToBase64String(signatureAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(unsignedToken))));
            }

            return unsignedToken + SignaturePrefix + signature;
        }

        private static string GenerateTokenExpiry(DateTime expiry)
        {
            return ((long) expiry.Subtract(SwtBaseTime).TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}