//-----------------------------------------------------------------------
// <copyright file="PlayReadyContentKey.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public abstract class PlayReadyContentKey
    {
    }

    /// <summary>
    /// Configures the license server to embed the content key identified in the content header sent with the license
    /// request in the returned license.  This is the typical content key configuration.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class ContentEncryptionKeyFromHeader : PlayReadyContentKey
    {
    }

    /// <summary>
    /// Configures the license server to embed the content key identified by the KeyIdentifier property of the ContentEncryptionKeyFromKeyIdentifier
    /// in the returned license.  This is not typcially used but does allow a specific content key identifier to be put in the license template.
    /// Note that if the content key returned in the license does not match the content key needed to play the content (which is configured in the
    /// header) the player will be unable to play the content.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/PlayReadyTemplate/v1")]
    public class ContentEncryptionKeyFromKeyIdentifier : PlayReadyContentKey
    {
        /// <summary>
        /// Identifier of the content key to embed in the license.
        /// </summary>
        [DataMember]
        public Guid KeyIdentifier { get; private set; }

        public ContentEncryptionKeyFromKeyIdentifier(Guid keyIdentifier)
        {
            if (keyIdentifier == Guid.Empty)
            {
                throw new ArgumentException("Cannot be Guid.Empty", "keyIdentifier");
            }

            KeyIdentifier = keyIdentifier;
        }
    }

}
