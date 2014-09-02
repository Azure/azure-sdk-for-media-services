using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public enum UnknownOutputPassingOption
    { 
        /// <summary>
        /// Passing the video portion of protected content to an Unknown Output is not allowed.
        /// </summary>
        [EnumMember]
        NotAllowed,

        /// <summary>
        /// Passing the video portion of protected content to an Unknown Output is allowed.
        /// </summary>
        [EnumMember]
        Allowed,

        /// <summary>
        /// Passing the video portion of protected content to an Unknown Output is allowed but
        /// the client must constrain the resolution of the video content.
        /// </summary>
        [EnumMember]
        AllowedWithVideoConstriction
    }
}
