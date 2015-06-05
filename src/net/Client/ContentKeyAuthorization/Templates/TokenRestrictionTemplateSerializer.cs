//-----------------------------------------------------------------------
// <copyright file="TokenRestrictionTemplateSerializer.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// This class is used to serialize and deserialize the Media Services Token Restriction Template Format.
    /// </summary>
    public static class TokenRestrictionTemplateSerializer
    {
        private static DataContractSerializer GetSerializer()
        {
            Type[] knownTypeList = 
            {
            typeof(SymmetricVerificationKey),
            typeof(AsymmetricTokenVerificationKey),
            typeof(X509CertTokenVerificationKey),
            typeof(RsaTokenVerificationKey),
            };

            return new DataContractSerializer(typeof(TokenRestrictionTemplate), knownTypeList);
        }

        /// <summary>
        /// Serializes a TokenRestrictionTemplate to a string containing an xml representation of
        /// the token restriction template.
        /// </summary>
        /// <param name="template">TokenRestrictionTemplate instance to serialize to a string</param>
        /// <returns>An xml string representation of the TokenRestrictionTemplate instance</returns>
        public static string Serialize(TokenRestrictionTemplate template)
        {
            if (template.PrimaryVerificationKey == null &&
                template.OpenIdConnectDiscoveryDocument == null)
            {
                throw new InvalidDataContractException(StringTable.PrimaryVerificationKeyAndOpenIdConnectDiscoveryDocumentAreNull);
            }

            if (template.OpenIdConnectDiscoveryDocument != null &&
                String.IsNullOrEmpty(template.OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri))
            {
                throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture,StringTable.ArgumentStringIsNullOrEmpty,"OpenIdDiscoveryUri"));
            }

            Uri openIdDiscoveryUri;
            if (template.OpenIdConnectDiscoveryDocument != null && !Uri.TryCreate(template.OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri, UriKind.Absolute, out openIdDiscoveryUri))
            {
                throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, StringTable.StringIsNotAbsoluteUri, "OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri"));
            }

           

            DataContractSerializer serializer = GetSerializer();

            if (template.TokenType == TokenType.SWT)
            {
                if (!Uri.IsWellFormedUriString(template.Issuer, UriKind.Absolute))
                {
                    throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, StringTable.InvalidAbsoluteUriInSWTToken,"template.Issuer"));
                }
                if (!Uri.IsWellFormedUriString(template.Audience, UriKind.Absolute))
                {
                    throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, StringTable.InvalidAbsoluteUriInSWTToken,"template.Audience"));
                }
            }

            return MediaServicesLicenseTemplateSerializer.SerializeToXml(template, serializer);
        }

        /// <summary>
        /// Deserializes a string containing an Xml representation of a TokenRestrictionTemplate
        /// back into a TokenRestrictionTemplate class instance.
        /// </summary>
        /// <param name="templateXml">A string containing the Xml representation of a TokenRestrictionTemplate</param>
        /// <returns>TokenRestrictionTemplate instance</returns>
        public static TokenRestrictionTemplate Deserialize(string templateXml)
        {
            TokenRestrictionTemplate templateToReturn = null;
            DataContractSerializer serializer = GetSerializer();

            StringReader stringReader = null;
            XmlReader reader = null;
            try
            {
                stringReader = new StringReader(templateXml);

                reader = XmlReader.Create(stringReader);

                templateToReturn = (TokenRestrictionTemplate)serializer.ReadObject(reader);
            }
            finally
            {
                if (reader != null)
                {
                    // This will close the underlying StringReader instance
                    reader.Close();
                }
                else if (stringReader != null)
                {
                    stringReader.Close();
                }
            }

            if (templateToReturn.PrimaryVerificationKey == null && templateToReturn.OpenIdConnectDiscoveryDocument == null)
            {
               throw new InvalidDataContractException(StringTable.PrimaryVerificationKeyAndOpenIdConnectDiscoveryDocumentAreNull);
            }

            if (templateToReturn.OpenIdConnectDiscoveryDocument != null && String.IsNullOrEmpty(templateToReturn.OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri))
            {
                throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, StringTable.ArgumentStringIsNullOrEmpty, "OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri"));
            }

            Uri openIdDiscoveryUri;
            if (templateToReturn.OpenIdConnectDiscoveryDocument != null && !Uri.TryCreate(templateToReturn.OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri, UriKind.Absolute, out openIdDiscoveryUri))
            {
                throw new InvalidDataContractException(String.Format(CultureInfo.InvariantCulture, StringTable.StringIsNotAbsoluteUri, "OpenIdConnectDiscoveryDocument.OpenIdDiscoveryUri"));
            }

            
            return templateToReturn;
        }

        private static readonly DateTime SwtBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private static string GenerateTokenExpiry(DateTime expiry)
        {
            return ((long)expiry.Subtract(SwtBaseTime).TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Used to generate a test token based on the the data in the given TokenRestrictionTemplate.
        /// </summary>
        /// <param name="tokenTemplate">TokenRestrictionTemplate describing the token to generate</param>
        /// <param name="signingKeyToUse">Specifies the specific signing key to use.  If null, the PrimaryVerificationKey from the template is used.</param>
        /// <param name="keyIdForContentKeyIdentifierClaim">Key Identifier used as the value of the Content Key Identifier Claim.  Ignored if no TokenClaim with a ClaimType of TokenClaim.ContentKeyIdentifierClaimType is not present</param>
        /// <param name="tokenExpiration">The Date and Time when the token expires.  Expired tokens are considered invalid by the Key Delivery Service.</param>
        /// <returns>A Simple Web Token (SWT)</returns>
        public static string GenerateTestToken(TokenRestrictionTemplate tokenTemplate, TokenVerificationKey signingKeyToUse = null, Guid? keyIdForContentKeyIdentifierClaim = null, DateTime? tokenExpiration = null)
        {
            if (tokenTemplate == null)
            {
                throw new ArgumentNullException("tokenTemplate");
            }

            if (signingKeyToUse == null)
            {
                signingKeyToUse = tokenTemplate.PrimaryVerificationKey;
            }

            if (!tokenExpiration.HasValue)
            {
                tokenExpiration = DateTime.UtcNow.AddMinutes(10);
            }

            StringBuilder builder = new StringBuilder();

            foreach (TokenClaim claim in tokenTemplate.RequiredClaims)
            {
                string claimValue = claim.ClaimValue;
                if (claim.ClaimType == TokenClaim.ContentKeyIdentifierClaimType)
                {
                    claimValue = keyIdForContentKeyIdentifierClaim.ToString();
                }

                builder.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(claim.ClaimType), HttpUtility.UrlEncode(claimValue));
            }

            builder.AppendFormat("Audience={0}&", HttpUtility.UrlEncode(tokenTemplate.Audience));
            builder.AppendFormat("ExpiresOn={0}&", GenerateTokenExpiry(tokenExpiration.Value));
            builder.AppendFormat("Issuer={0}", HttpUtility.UrlEncode(tokenTemplate.Issuer));

            SymmetricVerificationKey signingKey = (SymmetricVerificationKey)signingKeyToUse;
            using (var signatureAlgorithm = new HMACSHA256(signingKey.KeyValue))
            {
                byte[] unsignedTokenAsBytes = Encoding.UTF8.GetBytes(builder.ToString());

                byte[] signatureBytes = signatureAlgorithm.ComputeHash(unsignedTokenAsBytes);

                string signatureString = Convert.ToBase64String(signatureBytes);

               builder.AppendFormat("&HMACSHA256={0}", HttpUtility.UrlEncode(signatureString));
            }

            return builder.ToString();
        }
    }
}
