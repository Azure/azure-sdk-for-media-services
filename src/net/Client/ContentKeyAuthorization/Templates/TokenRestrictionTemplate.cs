using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Represents a token template for validating tokens that are presented by clients to the Key Delivery Service.
    /// The data within the template instructs the Key Delivery Service on whether a token should be considered valid or not.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class TokenRestrictionTemplate
    {
        public TokenRestrictionTemplate()
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
            RequiredClaims = new List<TokenClaim>();
            AlternateVerificationKeys = new List<TokenVerificationKey>();
        }

        /// <summary>
        /// A Uri describing the issuer of the token.  Must match the value in the token for the token to be considered valid.
        /// </summary>
        [DataMember(IsRequired = true)]
        public Uri Issuer
        {
            get { return _issuer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _issuer = value;
            }
        }
        private Uri _issuer;

        /// <summary>
        /// The Audience or Scope of the token.  Must match the value in the token for the token to be considered valid.
        /// </summary>
        [DataMember(IsRequired = true)]
        public Uri Audience
        {
            get { return _audience; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _audience = value;
            }
        }
        private Uri _audience;

        /// <summary>
        /// The first key tried to validate the signature of an incoming token.
        /// </summary>
        [DataMember(IsRequired = true)]
        public TokenVerificationKey PrimaryVerificationKey
        {
            get { return _primaryVerificationKey; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _primaryVerificationKey = value;
            }
        }
        private TokenVerificationKey _primaryVerificationKey;

        /// <summary>
        /// A list of additional token keys that will be tried if the token signature cannot be validted with the PrimaryVerificationKey 
        /// </summary>
        [DataMember]
        public IList<TokenVerificationKey> AlternateVerificationKeys {get; private set;}

        /// <summary>
        /// A list of claims that MUST be present in the token for the token to be considered valid.
        /// </summary>
        [DataMember]
        public IList<TokenClaim> RequiredClaims { get; private set;}

    }
}