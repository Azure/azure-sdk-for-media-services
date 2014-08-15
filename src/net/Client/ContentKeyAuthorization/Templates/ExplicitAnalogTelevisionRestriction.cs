using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures the Explicit Analog Television Output Restriction in the license.  This is a form of video output protection.
    /// For further details see the PlayReady Compliance Rules.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class ExplicitAnalogTelevisionRestriction
    {
        /// <summary>
        /// Controls whether the Explicit Analog Television Output Restriction is enforced on a Best Effort basis or not.
        /// If true, then the PlayReady client must make its best effort to enforce the restriction but can allow video content
        /// to flow to Analog Television Outputs if it cannot support the restriction.  If false, the PlayReady client must
        /// enforce the restriction.  For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember]
        public bool BestEffort { get; private set; }

        /// <summary>
        /// Configures the Explicit Analog Television Output Restriction control bits. For further details see the PlayReady Compliance Rules.
        /// </summary>
        [DataMember]
        public byte ConfigurationData { get; private set; }

        public ExplicitAnalogTelevisionRestriction(byte configurationData, bool bestEffort = false)
        {
            ScmsRestriction.VerifyTwoBitConfigurationData(configurationData);

            BestEffort = bestEffort;
            ConfigurationData = configurationData;
        }
    }
}
