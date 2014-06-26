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
    public abstract class TokenVerificationKey
    {

    }

    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class SymmetricVerificationKey : TokenVerificationKey
    {
        public SymmetricVerificationKey(byte[] keyValue)
        {
            KeyValue = keyValue;
        }

        public SymmetricVerificationKey()
        {
            _keyValue = new byte[64];

            // Erase fills the buffer with cryptographically random data
            EncryptionUtils.EraseKey(_keyValue);
        }

        [DataMember(IsRequired = true)]
        public byte[] KeyValue
        {
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _keyValue = value;
            }
            get
            {
                return _keyValue;
            }
        }

        private byte[] _keyValue;
    }
}