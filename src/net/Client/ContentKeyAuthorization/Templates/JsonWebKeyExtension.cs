//-----------------------------------------------------------------------
// <copyright file="OpenIDConnectDiscoveryDocument.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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

using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.IdentityModel.Protocols
{
    public static class JsonWebKeyExtension
    {
        public static TokenVerificationKey AsTokenVerificationKey(this Microsoft.IdentityModel.Protocols.JsonWebKey jwk)
        {
            X509Certificate2 cert = null;
            X509CertTokenVerificationKey key = null;

            if (jwk.X5c != null && jwk.X5c.Count > 0)
            {
                cert = new X509Certificate2(Convert.FromBase64String(jwk.X5c.First()));
                key = new X509CertTokenVerificationKey(cert);
                return key;
            }

            if (!String.IsNullOrEmpty(jwk.N) && !String.IsNullOrEmpty(jwk.E))
            {
                RsaTokenVerificationKey rsaToken = new RsaTokenVerificationKey();
                RSAParameters rsaParams = new RSAParameters()
                    {
                        Modulus = EncodeUtilities.Base64UrlDecode(jwk.N),
                        Exponent = EncodeUtilities.Base64UrlDecode(jwk.E)
                    };

                rsaToken.InitFromRsaParameters(rsaParams);
                return rsaToken;
            }
            throw new NotSupportedException(StringTable.NotSupportedJwkToTokenVerificationKeyConversion);

        }
    }




}