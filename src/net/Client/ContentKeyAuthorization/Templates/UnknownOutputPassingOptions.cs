//-----------------------------------------------------------------------
// <copyright file="UnknownOutputPassingOption.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
