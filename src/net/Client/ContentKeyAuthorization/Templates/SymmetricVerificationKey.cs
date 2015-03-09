//-----------------------------------------------------------------------
// <copyright file="SymmetricVerificationKey.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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