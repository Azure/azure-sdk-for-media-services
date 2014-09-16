using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

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
            DataContractSerializer serializer = GetSerializer();

            return MediaServicesLicenseTemplateSerializer.SerializeToXml(template, serializer);
        }

        private static XmlSchemaSet _cacheSchemaSet;
        private static XmlSchemaSet GetOldTokenFormatSchemaSet()
        {
            if (_cacheSchemaSet == null)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                XmlSchemaSet schemaSets = new XmlSchemaSet();
                using (Stream oldTokenTemplateFormatSchemaStream = assembly.GetManifestResourceStream("Microsoft.Cloud.Media.KeyDelivery.Templates.OldTokenTemplateFormat.xsd"))
                {
                    XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

                    XmlReader oldTokenTemplateFormatSchemaReader = XmlReader.Create(oldTokenTemplateFormatSchemaStream, xmlReaderSettings);

                    XmlSchema oldTokenTemplateFormatSchema = XmlSchema.Read(oldTokenTemplateFormatSchemaReader, null);

                    schemaSets.Add(oldTokenTemplateFormatSchema);
                }

                _cacheSchemaSet = schemaSets;
            }

            return _cacheSchemaSet;
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

            builder.AppendFormat("Audience={0}&", HttpUtility.UrlEncode(tokenTemplate.Audience.AbsoluteUri));
            builder.AppendFormat("ExpiresOn={0}&", GenerateTokenExpiry(tokenExpiration.Value));
            builder.AppendFormat("Issuer={0}", HttpUtility.UrlEncode(tokenTemplate.Issuer.AbsoluteUri));

            SymmetricVerificationKey signingKey = (SymmetricVerificationKey)signingKeyToUse;
            using (var signatureAlgorithm = new HMACSHA256(signingKey.KeyValue))
            {
                byte[] unsignedTokenAsBytes = Encoding.UTF8.GetBytes(builder.ToString());

                byte[] signatureBytes = signatureAlgorithm.ComputeHash(unsignedTokenAsBytes);

                string signatureString = Convert.ToBase64String(signatureBytes);

                builder.Insert(0, "Bearer=");
                builder.AppendFormat("&HMACSHA256={0}", HttpUtility.UrlEncode(signatureString));
            }

            return builder.ToString();
        }
    }
}
