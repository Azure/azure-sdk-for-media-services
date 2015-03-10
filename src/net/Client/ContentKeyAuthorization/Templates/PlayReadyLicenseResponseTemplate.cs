//-----------------------------------------------------------------------
// <copyright file="PlayReadyLicenseResponseTemplate.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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


using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Configures the PlayReady License Response Template.  
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class PlayReadyLicenseResponseTemplate : IExtensibleDataObject
    {
        //Implementing IExtensibleDataObject member ExtensionData

        #region IExtensibleDataObject Members
        private ExtensionDataObject _extensionData;
        public virtual ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

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