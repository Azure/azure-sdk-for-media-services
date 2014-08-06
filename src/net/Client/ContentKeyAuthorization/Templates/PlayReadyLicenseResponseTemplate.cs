using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures the PlayReady License Response Template.  
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class PlayReadyLicenseResponseTemplate
    {
        public PlayReadyLicenseResponseTemplate()
        {
            InternalConstruct();
        }

        [OnDeserializing]
        void OnDeserializing(StreamingContext c)
        {
            // The DataContractSerializer doesn't instantiate objects in the
            // normal fashion but instead calls FormatterServices.GetUninitializedObject.
            // This means that the constructor isn't called.  Thus use this function
            // to make sure our List instances are not null.
            InternalConstruct();
        }

        private void InternalConstruct()
        {
            LicenseTemplates = new List<PlayReadyLicenseTemplate>();
        }

        /// <summary>
        /// List of licenses to be returned to the client upon a license request.  Typcially this just
        /// has one license template configured.
        /// </summary>
        [DataMember(IsRequired=true)]
        public IList<PlayReadyLicenseTemplate> LicenseTemplates { get; private set; }

        /// <summary>
        /// A string returned to the client in the license response.  The client may or may not examine
        /// this data but it can be used for sending data to custom client implementations or adding
        /// data for diagnostic purposes.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ResponseCustomData { get; set; }
    }
}