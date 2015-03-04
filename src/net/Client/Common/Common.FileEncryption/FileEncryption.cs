//-----------------------------------------------------------------------
// <copyright file="FileEncryption.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides file encryption.
    /// </summary>
    public class FileEncryption : IDisposable
    {
        /// <summary>
        /// The version of the encryption scheme.
        /// </summary>
        public static readonly string SchemeVersion = "1.0";

        /// <summary>
        /// The name of the encryption scheme.
        /// </summary>
        public static readonly string SchemeName = "StorageEncryption";

        private object _lockObject = new object();
        private RNGCryptoServiceProvider _rng;
        private SymmetricAlgorithm _key;
        private Dictionary<string, ulong> _initializationVectorListByFileName = new Dictionary<string, ulong>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEncryption"/> class.
        /// </summary>
        public FileEncryption()
        {
            // The SymmetricAlgorithm will randomly generate a key for us.
            this.InternalInit(null, Guid.NewGuid());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEncryption"/> class.
        /// </summary>
        /// <param name="contentKey">The content key.</param>
        /// <param name="keyIdentifier">The key identifier.</param>
        public FileEncryption(byte[] contentKey, Guid keyIdentifier)
        {
            if (keyIdentifier == Guid.Empty)
            {
                throw new ArgumentException("Guid.Empty is not a valid keyIdentifier");
            }

            this.InternalInit(contentKey, keyIdentifier);
        }

        /// <summary>
        /// Gets the key identifier.
        /// </summary>
        public Guid KeyIdentifier { get; private set; }

        /// <summary>
        /// Gets the key identifier as string.
        /// </summary>
        /// <returns>The key ID.</returns>
        public string GetKeyIdentifierAsString()
        {
            return EncryptionUtils.GetKeyIdentifierAsString(this.KeyIdentifier);
        }

        /// <summary>
        /// Gets the content key.
        /// </summary>
        /// <returns>The content key.</returns>
        public byte[] GetContentKey()
        {
            return this._key.Key;
        }

        /// <summary>
        /// Gets the checksum.
        /// </summary>
        /// <returns>The checksum.</returns>
        public string GetChecksum()
        {
            return EncryptionUtils.CalculateChecksum(this._key.Key, this.KeyIdentifier);
        }

        /// <summary>
        /// Determines whether [is initialization vector present] [the specified file name].
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if [is initialization vector present] [the specified file name]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsInitializationVectorPresent(string fileName)
        {
            bool returnValue = false;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot be null or empty", "fileName");
            }

            lock(this._lockObject)
            {
                returnValue = this._initializationVectorListByFileName.ContainsKey(fileName);
            }

            return returnValue;
        }

        /// <summary>
        /// Creates the initialization vector for file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The initialization vector.</returns>
        public ulong CreateInitializationVectorForFile(string fileName)
        {
            ulong iv = 0;
            bool duplicateIv = false;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot be null or empty", "fileName");
            }

            lock(this._lockObject)
            {
                if (this._rng == null)
                {
                    this._rng = new RNGCryptoServiceProvider();
                }

                byte[] initializationVectorAsBytes = new byte[sizeof(ulong)];

                do
                {
                    this._rng.GetBytes(initializationVectorAsBytes);
                    iv = BitConverter.ToUInt64(initializationVectorAsBytes, 0);

                    // Each file protected by a given key must have a unique iv value.
                    // One of the issues with using CTR mode is that reusing counter
                    // values can lead to an attack on the encryption. To prevent that, 
                    // we use a unique 64-bit IV value per file.  The remaining 64 bits 
                    // of the counter value are a block counter ensuring that each block
                    // in the file has a unique counter value IF each file has a unique 
                    // 64-bit IV value.
                    duplicateIv = this._initializationVectorListByFileName.ContainsValue(iv);
                }
                while (duplicateIv);

                this._initializationVectorListByFileName.Add(fileName, iv);
            }

            return iv;
        }

        /// <summary>
        /// Gets the initialization vector for file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The initialization vector.</returns>
        public ulong GetInitializationVectorForFile(string fileName)
        {
            ulong returnValue = 0;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot be null or empty", "fileName");
            }

            lock(this._lockObject)
            {
                returnValue = this._initializationVectorListByFileName[fileName];
            }

            return returnValue;
        }

        /// <summary>
        /// Sets the initialization vector for file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="initializationVectorToSet">The initialization vector to set.</param>
        public void SetInitializationVectorForFile(string fileName, ulong initializationVectorToSet)
        {
            ulong temp = 0;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot be null or empty", "fileName");
            }

            lock(this._lockObject)
            {
                if (!this._initializationVectorListByFileName.TryGetValue(fileName, out temp))
                {
                    this._initializationVectorListByFileName.Add(fileName, initializationVectorToSet);
                }
                else if (this._initializationVectorListByFileName[fileName] != initializationVectorToSet)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, "An initialization vector is already set for {0}.", fileName);
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Encrypts the content key to certificate.
        /// </summary>
        /// <param name="certToUse">The cert to use.</param>
        /// <returns>The encrypted content key.</returns>
        public byte[] EncryptContentKeyToCertificate(X509Certificate2 certToUse)
        {
            return EncryptionUtils.EncryptSymmetricKey(certToUse, this._key);
        }

        /// <summary>
        /// Gets the file encryption transform.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The transform.</returns>
        public FileEncryptionTransform GetTransform(string fileName)
        {
            return this.GetTransform(fileName, 0);
        }

        /// <summary>
        /// Gets the file encryption transform.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileOffset">The file offset.</param>
        /// <returns>The transform.</returns>
        public FileEncryptionTransform GetTransform(string fileName, long fileOffset)
        {
            ulong iv = 0;

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Cannot be null or empty", "fileName");
            }

            if (this.IsInitializationVectorPresent(fileName))
            {
                iv = this.GetInitializationVectorForFile(fileName);
            }
            else
            {
                iv = this.CreateInitializationVectorForFile(fileName);
            }

            return this.GetTransform(iv, fileOffset);
        }

        /// <summary>
        /// Gets the file encryption transform.
        /// </summary>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <returns>The transform.</returns>
        public FileEncryptionTransform GetTransform(ulong initializationVector)
        {
            return this.GetTransform(initializationVector, 0);
        }

        /// <summary>
        /// Gets the file encryption transform.
        /// </summary>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <param name="fileOffset">The file offset.</param>
        /// <returns>The transform.</returns>
        public FileEncryptionTransform GetTransform(ulong initializationVector, long fileOffset)
        {
            ICryptoTransform transform = null;

            lock(this._lockObject)
            {
                // Note that ECB encrypt is always used for AES-CTR whether doing encryption or decryption.
                transform = this._key.CreateEncryptor();
            }

            return new FileEncryptionTransform(transform, initializationVector, fileOffset);
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
                if (this._key != null)
                {
                    this._key.Dispose();
                    this._key = null;
                }

                if (this._rng != null)
                {
                    this._rng.Dispose();
                    this._rng = null;
                }
            }
        }

        /// <summary>
        /// Intializes this instance.
        /// </summary>
        /// <param name="contentKey">The content key.</param>
        /// <param name="keyIdentifier">The key identifier.</param>
        private void InternalInit(byte[] contentKey, Guid keyIdentifier)
        {
            if ((contentKey != null) && (contentKey.Length != EncryptionUtils.KeySizeInBytesForAes256))
            {
                throw new ArgumentOutOfRangeException("contentKey", "StorageEncryption content keys are 256-bits in length.");
            }

            this.KeyIdentifier = keyIdentifier;

            this._key = new AesCryptoServiceProvider();
            this._key.Mode = CipherMode.ECB;
            this._key.Padding = PaddingMode.None;
            if (contentKey != null)
            {
                this._key.Key = contentKey;
            }
            else
            {
                this._key.KeySize = EncryptionUtils.KeySizeInBitsForAes256;
            }
        }
    }
}
