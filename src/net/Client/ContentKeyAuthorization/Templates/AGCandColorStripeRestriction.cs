using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures Automatic Gain Control (AGC) and Color Stripe in the license.  These are a form of video output protection.
    /// For further details see the PlayReady Compliance Rules.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class AgcAndColorStripeRestriction
    {
        /// <summary>
        /// Configures the Automatic Gain Control (AGC) and Color Stripe control bits. For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember]
        public byte ConfigurationData { get; private set; }

        public AgcAndColorStripeRestriction(byte configurationData)
        {
            ScmsRestriction.VerifyTwoBitConfigurationData(configurationData);
            ConfigurationData = configurationData;
        }
    }
}
