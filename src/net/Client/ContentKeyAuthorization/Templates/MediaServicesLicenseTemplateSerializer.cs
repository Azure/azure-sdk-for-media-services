using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// This class is used to serialize and deserialize the Media Services License Template Format.
    /// </summary>
    public static class MediaServicesLicenseTemplateSerializer
    {
        private static DataContractSerializer GetSerializer()
        {
            Type[] knownTypeList = 
            {
            typeof(ContentEncryptionKeyFromKeyIdentifier),
            typeof(ContentEncryptionKeyFromHeader)
            };

            return new DataContractSerializer(typeof(PlayReadyLicenseResponseTemplate), knownTypeList);
        }

        internal static string SerializeToXml(object template, DataContractSerializer serializer)
        {
            //
            // Setup the XmlWriter and underlying StringBuilder
            //
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            using (XmlWriter writer = XmlWriter.Create(builder, settings))
            {
                serializer.WriteObject(writer, template);

                //
                //  Flush the XmlWriter and return the string in the builder
                //
                writer.Flush();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Serializes a PlayReadyLicenseResponseTemplate to a string containing an xml representation of
        /// the license response template.
        /// </summary>
        /// <param name="responseTemplate">The PlayReadyLicenseResponseTemplate instance to serialize to a string.</param>
        /// <returns></returns>
        public static string Serialize(PlayReadyLicenseResponseTemplate responseTemplate)
        {
            ValidateLicenseResponseTemplate(responseTemplate);

            DataContractSerializer serializer = GetSerializer();

            return SerializeToXml(responseTemplate, serializer);
        }

        /// <summary>
        /// Deserializes a string containing an Xml representation of a PlayReadyLicenseResponseTemplate
        /// back into a PlayReadyLicenseResponseTemplate class instance.
        /// </summary>
        /// <param name="templateXml">Contains the Xml representation of a PlayReadyLicenseResponseTemplate</param>
        /// <returns></returns>
        public static PlayReadyLicenseResponseTemplate Deserialize(string templateXml)
        {
            PlayReadyLicenseResponseTemplate templateToReturn = null;
            DataContractSerializer serializer = GetSerializer();

            StringReader stringReader = null;
            XmlReader reader = null;
            try
            {
                stringReader = new StringReader(templateXml);

                reader = XmlReader.Create(stringReader);

                templateToReturn = (PlayReadyLicenseResponseTemplate)serializer.ReadObject(reader);
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

            ValidateLicenseResponseTemplate(templateToReturn);

            return templateToReturn;
        }

        private static void ValidateLicenseResponseTemplate(PlayReadyLicenseResponseTemplate templateToValidate)
        {
            // Validate the PlayReadyLicenseResponseTemplate has at least one license
            if (templateToValidate.LicenseTemplates.Count <= 0)
            {
                throw new ArgumentException(ErrorMessages.AtLeastOneLicenseTemplateRequired);
            }

            foreach (PlayReadyLicenseTemplate template in templateToValidate.LicenseTemplates)
            {
                // This is actually enforced in the DataContract with the IsRequired attribute
                // so this check should never fail.
                if (template.ContentKey == null)
                {
                    throw new ArgumentException(ErrorMessages.PlayReadyContentKeyRequired);
                }

                // A PlayReady license must have at least one Right in it.  Today we only
                // support the PlayRight so it is required.  In the future we might support
                // other types of rights (CopyRight, perhaps an extensible Right, whatever)
                // so we enforce this in code and not in the DataContract itself.
                if (template.PlayRight == null)
                {
                    throw new ArgumentException(ErrorMessages.PlayReadyPlayRightRequired);
                }

                //
                //  Per the PlayReady Compliance rules (section 3.8 - Output Control for Unknown Outputs), passing content to 
                //  unknown output is prohibited if the DigitalVideoOnlyContentRestriction is enabled.
                //
                if (template.PlayRight.DigitalVideoOnlyContentRestriction)
                {
                    if ((template.PlayRight.AllowPassingVideoContentToUnknownOutput == UnknownOutputPassingOption.Allowed) ||
                        (template.PlayRight.AllowPassingVideoContentToUnknownOutput == UnknownOutputPassingOption.AllowedWithVideoConstriction))
                    {
                        throw new ArgumentException(ErrorMessages.DigitalVideoOnlyMutuallyExclusiveWithPassingToUnknownOutputError);
                    }
                }

                //
                //  License template should not have both BeginDate and RelativeBeginDate set.
                //  Only one of these two values should be set.
                if ((template.BeginDate.HasValue) && (template.RelativeBeginDate.HasValue))
                {
                    throw new ArgumentException(ErrorMessages.BeginDateAndRelativeBeginDateCannotbeSetSimultaneouslyError);
                }

                //
                //  License template should not have both ExpirationDate and RelativeExpirationDate set.
                //  Only one of these two values should be set.
                if ((template.ExpirationDate.HasValue) && (template.RelativeExpirationDate.HasValue))
                {
                    throw new ArgumentException(ErrorMessages.ExpirationDateAndRelativeExpirationDateCannotbeSetSimultaneouslyError);
                }

                if (template.LicenseType == PlayReadyLicenseType.Nonpersistent)
                {
                    //
                    //  The PlayReady Rights Manager SDK will return an error if you try to specify a license
                    //  that is non-persistent and has a first play expiration set.  The event log message related
                    //  to the error will say "LicenseGenerationFailure: FirstPlayExpiration can not be set on Non 
                    //  Persistent license PlayRight."
                    //
                    if (template.PlayRight.FirstPlayExpiration.HasValue)
                    {
                        throw new ArgumentException(ErrorMessages.FirstPlayExpirationCannotBeSetOnNonPersistentLicense);
                    }

                    //
                    //  The PlayReady Rights Manager SDK will return an error if you try to specify a license
                    //  that is non-persistent and has a GracePeriod set.
                    //
                    if (template.GracePeriod.HasValue)
                    {
                        throw new ArgumentException(ErrorMessages.GracePeriodCannotBeSetOnNonPersistentLicense);
                    }

                    //
                    //  The PlayReady Rights Manager SDK will return an error if you try to specify a license
                    //  that is non-persistent and has a GracePeriod set.  The event log message related
                    //  to the error will say "LicenseGenerationFailure: BeginDate or ExpirationDate should not be set 
                    //  on Non Persistent licenses"
                    //
                    if (template.BeginDate.HasValue)
                    {
                        throw new ArgumentException(ErrorMessages.BeginDateCannotBeSetOnNonPersistentLicense);
                    }

                    //
                    //  The PlayReady Rights Manager SDK will return an error if you try to specify a license
                    //  that is non-persistent and has a GracePeriod set.  The event log message related
                    //  to the error will say "LicenseGenerationFailure: BeginDate or ExpirationDate should not be set 
                    //  on Non Persistent licenses"
                    //
                    if (template.ExpirationDate.HasValue)
                    {
                        throw new ArgumentException(ErrorMessages.ExpirationCannotBeSetOnNonPersistentLicense);
                    }
                }
            }
        }
    }
}
