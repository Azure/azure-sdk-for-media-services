//-----------------------------------------------------------------------
// <copyright file="TokenClaim.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class TokenClaim
    {
        /// <summary>
        /// The claim type for the ContentKeyIdentifierClaim.
        /// </summary>
        public static readonly string ContentKeyIdentifierClaimType = "urn:microsoft:azure:mediaservices:contentkeyidentifier";

        /// <summary>
        /// This claim requires that the value of the claim in the token must match the key identifier of the key being requested by the client.
        /// Adding this claim means that the token issued to the client authorizes access to the content key identifier listed in the token.
        /// </summary>        
        public static readonly TokenClaim ContentKeyIdentifierClaim = new TokenClaim(ContentKeyIdentifierClaimType, null);
        
        public TokenClaim(string claimType, string claimValue)
        {
            if (String.IsNullOrWhiteSpace(claimType))
            {
                throw new ArgumentException("The ClaimType cannot be null, empty, or only whitespace.", "claimType");
            }

            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        /// <summary>
        /// Type of the Claim in the Token
        /// </summary>
        [DataMember(IsRequired = true)]
        public string ClaimType { get; private set; }

        /// <summary>
        /// Value of the Claim in Token
        /// </summary>
        [DataMember]
        public string ClaimValue { get; private set; }
    }
}