//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyRestriction.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Authorization restrictions that must be met in order for the key to be delivered to the client using this authorization policy option.
    /// The requirements of each  restriction MUST be met in order to deliver the key using the key delivery data.
    /// </summary>
    public class ContentKeyAuthorizationPolicyRestriction
    {
        /// <summary>
        /// Friendly name of the restriction.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the restriction.
        /// </summary>
        public int KeyRestrictionType { get; set; }

        /// <summary>
        /// Restriction requirements.
        /// </summary>
        public string Requirements { get; set; }

        public static ContentKeyRestrictionType GetKeyRestrictionTypeValue(ContentKeyAuthorizationPolicyRestriction restriction)
        {
            return (ContentKeyRestrictionType)restriction.KeyRestrictionType;
        }

        public void SetKeyRestrictionTypeValue(ContentKeyRestrictionType value)
        {
            KeyRestrictionType = (int)value;
        }
    }
}
