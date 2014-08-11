using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures the Serial Copy Management System (SCMS) in the license.  SCMS is a form of audio output protection.
    /// For further details see the PlayReady Compliance Rules.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class ScmsRestriction
    {
        /// <summary>
        /// Configures the Serial Copy Management System (SCMS) control bits. For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember]
        public byte ConfigurationData { get; private set; }

        public ScmsRestriction(byte configurationData)
        {
            VerifyTwoBitConfigurationData(configurationData);

            ConfigurationData = configurationData;
        }

        internal static void VerifyTwoBitConfigurationData(byte configurationData)
        {
            bool valid = new byte[] { 0, 1, 2, 3 }.Contains(configurationData);

            if (!valid)
            {
                throw new ArgumentException(ErrorMessages.InvalidTwoBitConfigurationData);
            }
        }
    }
}
