//-----------------------------------------------------------------------
// <copyright file="ConfigurationEncryption.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provide encryption for configuration.
    /// </summary>
    public class ConfigurationEncryption : IDisposable
    {
        /// <summary>
        /// The version of the encryption scheme.
        /// </summary>
        public const string SchemeVersion = "1.0";

        /// <summary>
        /// The name of the encryption scheme.
        /// </summary>
        public const string SchemeName = "ConfigurationEncryption";

        private SymmetricAlgorithm _encryptionAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEncryption"/> class.
        /// </summary>
        public ConfigurationEncryption()
        {
            this.InternalInit(Guid.NewGuid(), null, null); // the SymmetricAlgorithm will randomly generate a key and IV for us
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationEncryption"/> class.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="contentKey">The content key.</param>
        /// <param name="initializationVector">The initialization vector.</param>
        public ConfigurationEncryption(Guid keyIdentifier, byte[] contentKey, byte[] initializationVector)
        {
            if (keyIdentifier == Guid.Empty)
            {
                throw new ArgumentException(CommonStringsFileEncryption.ErrorInvalidKeyIdentifier);
            }

            this.InternalInit(keyIdentifier, contentKey, initializationVector);
        }

        /// <summary>
        /// Gets the key identifier.
        /// </summary>
        public Guid KeyIdentifier { get; private set; }

        /// <summary>
        /// Gets the initialization vector from string.
        /// </summary>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <returns>The initialization vector for the specified string.</returns>
        public static byte[] GetInitializationVectorFromString(string initializationVector)
        {
            return Convert.FromBase64String(initializationVector);
        }

        /// <summary>
        /// Gets the content key.
        /// </summary>
        /// <returns>The content key.</returns>
        public byte[] GetContentKey()
        {
            return this._encryptionAlgorithm.Key;
        }

        /// <summary>
        /// Gets the key identifier as string.
        /// </summary>
        /// <returns>The key ID string.</returns>
        public string GetKeyIdentifierAsString()
        {
            return EncryptionUtils.GetKeyIdentifierAsString(this.KeyIdentifier);
        }

        /// <summary>
        /// Gets the initialization vector.
        /// </summary>
        /// <returns>The initialization vector bytes.</returns>
        public byte[] GetInitializationVector()
        {
            return this._encryptionAlgorithm.IV;
        }

        /// <summary>
        /// Gets the initialization vector as string.
        /// </summary>
        /// <returns>The initialization vector string.</returns>
        public string GetInitializationVectorAsString()
        {
            return Convert.ToBase64String(this._encryptionAlgorithm.IV);
        }

        /// <summary>
        /// Gets the checksum.
        /// </summary>
        /// <returns>The checksum string.</returns>
        public string GetChecksum()
        {
            return EncryptionUtils.CalculateChecksum(this._encryptionAlgorithm.Key, this.KeyIdentifier);
        }

        /// <summary>
        /// Encrypts the configuration.
        /// </summary>
        /// <param name="original">The configuration.</param>
        /// <returns>The encrypted value.</returns>
        public string Encrypt(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                throw new ArgumentException("The string to encrypt cannot be null or empty.", "original");
            }

            byte[] data = Encoding.UTF8.GetBytes(original);

            byte[] encryptedData = null;
            using (ICryptoTransform transform = this._encryptionAlgorithm.CreateEncryptor())
            {
                encryptedData = transform.TransformFinalBlock(data, 0, data.Length);
            }

            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts the specified encrypted value.
        /// </summary>
        /// <param name="encryptedValue">The encrypted value.</param>
        /// <returns>The decrypted configuration.</returns>
        public string Decrypt(string encryptedValue)
        {
            if (string.IsNullOrEmpty(encryptedValue))
            {
                throw new ArgumentException("The string to decrypt cannot be null or empty.", "encryptedValue");
            }

            byte[] data = Convert.FromBase64String(encryptedValue);
            byte[] decryptedData = null;

            using (ICryptoTransform transform = this._encryptionAlgorithm.CreateDecryptor())
            {
                decryptedData = transform.TransformFinalBlock(data, 0, data.Length);
            }

            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Encrypts the content key using the specified certificate.
        /// </summary>
        /// <param name="certToUse">The cert to use.</param>
        /// <returns>The encrypted content key.</returns>
        public byte[] EncryptContentKeyToCertificate(X509Certificate2 certToUse)
        {
            return EncryptionUtils.EncryptSymmetricKey(certToUse, this._encryptionAlgorithm);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // Take this object off the finalization queue and prevent the finalization
            // code from running a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._encryptionAlgorithm != null)
                {
                    this._encryptionAlgorithm.Dispose();
                    this._encryptionAlgorithm = null;
                }
            }
        }

        private void InternalInit(Guid keyIdentifier, byte[] contentKey, byte[] initializationVector)
        {
            if ((contentKey != null) && (contentKey.Length != EncryptionUtils.KeySizeInBytesForAes256))
            {
                throw new ArgumentOutOfRangeException("contentKey", "Configuration Encryption content keys are 256-bits (32 bytes) in length.");
            }

            if ((initializationVector != null) && (initializationVector.Length != EncryptionUtils.IVSizeInBytesForAesCbc))
            {
                throw new ArgumentOutOfRangeException("initializationVector", "Configuration Encryption initialization vectors are 16 bytes in length.");
            }

            this.KeyIdentifier = keyIdentifier;

            this._encryptionAlgorithm = new AesCryptoServiceProvider();
            this._encryptionAlgorithm.Mode = CipherMode.CBC;
            this._encryptionAlgorithm.Padding = PaddingMode.PKCS7;

            if (contentKey != null)
            {
                this._encryptionAlgorithm.Key = contentKey;
                this._encryptionAlgorithm.IV = initializationVector;
            }
            else
            {
                this._encryptionAlgorithm.KeySize = EncryptionUtils.KeySizeInBitsForAes256;
            }
        }
    }
}
