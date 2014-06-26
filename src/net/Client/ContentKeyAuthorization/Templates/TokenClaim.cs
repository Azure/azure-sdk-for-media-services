using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.MediaServices.Client;

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

        [DataMember(IsRequired = true)]
        public string ClaimType { get; private set; }

        [DataMember]
        public string ClaimValue { get; private set; }
    }
}