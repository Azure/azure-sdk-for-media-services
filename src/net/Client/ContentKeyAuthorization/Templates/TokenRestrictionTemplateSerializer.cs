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

        public static void ValidateAgainstOldTokenFormatSchema(string template)
        {
            XmlSchemaSet schemaSets = GetOldTokenFormatSchemaSet();

            XDocument templateDocument = XDocument.Parse(template);

            templateDocument.Validate(schemaSets, null);
        }

        public static bool IsOldTokenFormat(string template)
        {
            bool returnValue = false;
            XDocument parsedTemplate = XDocument.Parse(template);

            if (0 == String.Compare(parsedTemplate.Root.Name.LocalName, "TokenRestriction", StringComparison.Ordinal))
            {
                returnValue = true;
            }

            return returnValue;
        }

        private static TokenRestrictionTemplate ConvertFromOldTokenFormat(XmlReader reader)
        {            
            XmlSerializer serializer = new XmlSerializer(typeof(TokenRestriction));

            TokenRestriction tokenRestrictionInOldFormat = (TokenRestriction)serializer.Deserialize(reader);

            TokenRestrictionTemplate templateToReturn = new TokenRestrictionTemplate();
            templateToReturn.Issuer = new Uri(tokenRestrictionInOldFormat.issuer);
            templateToReturn.Audience = new Uri(tokenRestrictionInOldFormat.audience);

            if (tokenRestrictionInOldFormat.RequiredClaims != null)
            {
                foreach (TokenRestrictionClaim currentClaim in tokenRestrictionInOldFormat.RequiredClaims)
                {
                    TokenClaim claim = new TokenClaim(currentClaim.type, currentClaim.value);
                    templateToReturn.RequiredClaims.Add(claim);
                }
            }

            if (tokenRestrictionInOldFormat.VerificationKeys != null)
            {
                foreach (TokenRestrictionVerificationKey verificationKey in tokenRestrictionInOldFormat.VerificationKeys)
                {
                    if (verificationKey.type == VerificationKeyType.Symmetric)
                    {
                        if (verificationKey.IsPrimary && (templateToReturn.PrimaryVerificationKey == null))
                        {
                            templateToReturn.PrimaryVerificationKey = new SymmetricVerificationKey(verificationKey.value);
                        }
                        else
                        {
                            templateToReturn.AlternateVerificationKeys.Add(new SymmetricVerificationKey(verificationKey.value));
                        }
                    }
                }
            }

            return templateToReturn;
        }

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

                if (IsOldTokenFormat(templateXml))
                {
                    templateToReturn = ConvertFromOldTokenFormat(reader);
                }
                else
                {
                    templateToReturn = (TokenRestrictionTemplate)serializer.ReadObject(reader);
                }
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
