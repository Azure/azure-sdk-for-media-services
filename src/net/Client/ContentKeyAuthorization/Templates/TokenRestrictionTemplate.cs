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

        [DataMember]
        public IList<TokenVerificationKey> AlternateVerificationKeys {get; private set;}


        [DataMember]
        public IList<TokenClaim> RequiredClaims { get; private set;}

    }
}