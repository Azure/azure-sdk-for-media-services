using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// Represents a Symmetric Verification Key used to sign or verify the signature on a Token.
    /// The Key is typically used with HMACSHA256 and thus uses a 64 byte key value.
    /// </summary>
    [DataContract(Namespace = "http://schemas.microsoft.com/Azure/MediaServices/KeyDelivery/TokenRestrictionTemplate/v1")]
    public class SymmetricVerificationKey : TokenVerificationKey
    {
        /// <summary>
        /// Constructs a SymmetricVerificationKey using the provided key value.
        /// </summary>
        /// <param name="keyValue">Value of the key</param>
        public SymmetricVerificationKey(byte[] keyValue)
        {
            KeyValue = keyValue;
        }

        /// <summary>
        /// Constructs a SymmetricVerificationKey using a randomly generated key value.
        /// The key value generated is 64 bytes long.
        /// </summary>
        public SymmetricVerificationKey()
        {
            _keyValue = new byte[64];

            // Erase fills the buffer with cryptographically random data
            EncryptionUtils.EraseKey(_keyValue);
        }

        /// <summary>
        /// Value of the Key used for Token signing or verification
        /// </summary>
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