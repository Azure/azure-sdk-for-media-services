//-----------------------------------------------------------------------
// <copyright file="PlayReadyLicenseTemplate.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Represents a license template for creating PlayReady licenses to return to clients.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class PlayReadyLicenseTemplate : IExtensibleDataObject
    {
        public PlayReadyLicenseTemplate()
        {
            ContentKey = new ContentEncryptionKeyFromHeader();
            PlayRight = new PlayReadyPlayRight();
        }

        /// <summary>
        /// Controls whether test devices can use the license or not.  If true, the MinimumSecurityLevel property of the license
        /// is set to 150.  If false (the default), the MinimumSecurityLevel property of the license is set to 2000.
        /// </summary>
        [DataMember]
        public bool AllowTestDevices { get; set; }

        /// <summary>
        /// Configures the starting DateTime that the license is valid.  Attempts to use the license before this date and time will
        /// result in an error on the client.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? BeginDate { get; set; }

        /// <summary>
        /// Configures the DateTime value when the the license expires.  Attempts to use the license after this date and time will
        /// result in an error on the client.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Configures starting DateTime value when the license is valid.  Attempts to use the license before this date and time 
        /// will result in an error on the client.  The DateTime value is calculated as DateTime.UtcNow + RelativeBeginDate when 
        /// the license is issued
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan? RelativeBeginDate { get; set; }

        /// <summary>
        /// Configures the DateTime value when the license expires.  Attempts to use the license after this date and time will result 
        /// in an error on the client.  The DateTime value is calculated as DateTime.UtcNow + RelativeExpirationDate when the license 
        /// is issued
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan? RelativeExpirationDate { get; set; }
        /// <summary>
        /// Configures the Grace Period setting of the PlayReady license.  This setting affects how DateTime based restrictions are
        /// evaluated on certain devices in the situation that the devices secure clock becomes unset.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public TimeSpan? GracePeriod  { get; set; }

        /// <summary>
        /// Configures the PlayRight of the PlayReady license.  This Right gives the client the ability to play back the content.
        /// The PlayRight also allows configuring restrictions specific to playback.  This Right is required.
        /// </summary>
        [DataMember]
        public PlayReadyPlayRight PlayRight { get; set; }

        /// <summary>
        /// Configures whether the license is persistent (saved in persistent storage on the client) or non-persistent (only held in
        /// memory while the player is using the license).  Persistent licenses are typically used to allow offline playback of the
        /// content.
        /// </summary>
        [DataMember]
        public PlayReadyLicenseType LicenseType { get; set; }

        /// <summary>
        /// Specifies the content key in the license.  This is typically set to an instance of the ContentEncryptionKeyFromHeader
        /// object to allow the template to be applied to multiple content keys and have the content header tell the license
        /// server the exact key to embed in the license issued to the client.
        /// </summary>
        [DataMember(IsRequired=true)]
        public PlayReadyContentKey ContentKey { get; set; }

        //Implementing IExtensibleDataObject member ExtensionData
        
        #region IExtensibleDataObject Members
        private ExtensionDataObject _extensionData;
        public virtual ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion
    }
}
