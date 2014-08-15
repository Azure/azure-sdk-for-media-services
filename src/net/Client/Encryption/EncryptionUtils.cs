//-----------------------------------------------------------------------
// <copyright file="EncryptionUtils.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides helpers for encryption.
    /// </summary>
    public static class EncryptionUtils
    {
        private const string KeyIdentifierPrefix = "nb:kid:UUID:";

        /// <summary>
        /// The key size for AEI 128.
        /// </summary>
        public const int KeySizeInBytesForAes128 = 16;

        /// <summary>
        /// The key size for AEI 256.
        /// </summary>
        public const int KeySizeInBytesForAes256 = 32;

        /// <summary>
        /// The key size for AEI 128 in bits.
        /// </summary>
        public const int KeySizeInBitsForAes128 = 128;

        /// <summary>
        /// The key size for AEI 256 in bits.
        /// </summary>
        public const int KeySizeInBitsForAes256 = 256;

        /// <summary>
        /// The IV size for AEI Cbc.
        /// </summary>
        public const int IVSizeInBytesForAesCbc = 16;

        /// <summary>
        /// Gets the key identifier as string.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns>The key ID.</returns>
        public static string GetKeyIdentifierAsString(Guid keyIdentifier)
        {
            return KeyIdentifierPrefix + keyIdentifier.ToString();
        }

        /// <summary>
        /// Gets the key id as GUID.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns>The key ID.</returns>
        public static Guid GetKeyIdAsGuid(string keyIdentifier)
        {
            if (string.IsNullOrEmpty(keyIdentifier))
            {
                throw new ArgumentException("Key Identifier string cannot be null or empty.", "keyIdentifier");
            }

            if (keyIdentifier.StartsWith(KeyIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new Guid(keyIdentifier.Substring(KeyIdentifierPrefix.Length));
            }
            else
            {
                throw new ArgumentException("Key Identifier string was not in the expected format.", "keyIdentifier");
            }
        }

        /// <summary>
        /// Encrypts the symmetric key.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <param name="aes">The aes.</param>
        /// <returns>The encrypted key.</returns>
        public static byte[] EncryptSymmetricKey(X509Certificate2 cert, SymmetricAlgorithm aes)
        {
            if (aes == null)
            {
                throw new ArgumentNullException("aes");
            }

            return EncryptSymmetricKeyData(cert, aes.Key);
        }

        /// <summary>
        /// Encrypts the symmetric key data.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <param name="keyData">The key data.</param>
        /// <returns>The encrypted data.</returns>
        public static byte[] EncryptSymmetricKeyData(X509Certificate2 cert, byte[] keyData)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }

            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }

            RSACryptoServiceProvider rsaPublicKey = cert.PublicKey.Key as RSACryptoServiceProvider;

            RSAOAEPKeyExchangeFormatter keyFormatter = new RSAOAEPKeyExchangeFormatter(rsaPublicKey);

            return keyFormatter.CreateKeyExchange(keyData);
        }

        /// <summary>
        /// Decrypts the symmetric key.
        /// </summary>
        /// <param name="cert">The cert.</param>
        /// <param name="encryptedData">The encrypted data.</param>
        /// <returns>The decrypted key.</returns>
        public static byte[] DecryptSymmetricKey(X509Certificate2 cert, byte[] encryptedData)
        {
            AsymmetricAlgorithm rsaPrivateKey = null;
            RSAOAEPKeyExchangeDeformatter keyFormatter = null;

            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }

            if (!cert.HasPrivateKey)
            {
                throw new ArgumentException("Certificate does not have a private key which is a requirment for decryption.", "cert");
            }

            try
            {
                rsaPrivateKey = cert.PrivateKey;
            }
            catch (CryptographicException ce)
            {
                if (ce.Message.Contains("Keyset does not exist"))
                {
                    IdentityReference currentUser = WindowsIdentity.GetCurrent().Owner as IdentityReference;
                    string message = string.Format(CultureInfo.CurrentCulture, "Unable to create the RSAOAEPKeyExchangeDeformatter likely due to the access permissions on the private key.  Check to see if the current user has access to the private key for the certificate with thumbprint={0}.  Current User is {1}.", cert.Thumbprint, currentUser.ToString());
                    throw new InvalidOperationException(message, ce);
                }
                else
                {
                    throw;
                }
            }

            keyFormatter = new RSAOAEPKeyExchangeDeformatter(rsaPrivateKey);

            return keyFormatter.DecryptKeyExchange(encryptedData);
        }

        /// <summary>
        /// Gets the certificate from store.
        /// </summary>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <returns>The X509 certificate.</returns>
        public static X509Certificate2 GetCertificateFromStore(string certificateThumbprint)
        {
            return GetCertificateFromStore(certificateThumbprint, StoreLocation.CurrentUser) ??
                   GetCertificateFromStore(certificateThumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        /// Gets the certificate from store.
        /// </summary>
        /// <param name="certificateThumbprint">The certificate thumbprint.</param>
        /// <param name="location">The location.</param>
        /// <returns>The X509 certificate.</returns>
        public static X509Certificate2 GetCertificateFromStore(string certificateThumbprint, StoreLocation location)
        {
            X509Certificate2 returnValue = null;

            if (string.IsNullOrEmpty(certificateThumbprint))
            {
                throw new ArgumentException("Cannot be null or empty", "certificateThumbprint");
            }

            // Get the certificate store for the current user.
            X509Store store = new X509Store(location);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint, false);

                if (certs.Count > 0)
                {
                    returnValue = certs[0];
                }
            }
            finally
            {
                store.Close();
            }

            return returnValue;
        }

        /// <summary>
        /// Saves the certificate to store.
        /// </summary>
        /// <param name="certToStore">The cert to store.</param>
        public static void SaveCertificateToStore(X509Certificate2 certToStore)
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);

            try
            {
                store.Open(OpenFlags.ReadWrite);

                store.Add(certToStore);
            }
            finally
            {
                store.Close();
            }
        }

        /// <summary>
        /// Calculates the checksum.
        /// </summary>
        /// <param name="contentKey">The content key.</param>
        /// <param name="keyId">The key id.</param>
        /// <returns>The checksum.</returns>
        public static string CalculateChecksum(byte[] contentKey, Guid keyId)
        {
            const int ChecksumLength = 8;
            const int KeyIdLength = 16;

            byte[] encryptedKeyId = null;

            // Checksum is computed by AES-ECB encrypting the KID
            // with the content key.
            using (AesCryptoServiceProvider rijndael = new AesCryptoServiceProvider())
            {
                rijndael.Mode = CipherMode.ECB;
                rijndael.Key = contentKey;
                rijndael.Padding = PaddingMode.None;

                ICryptoTransform encryptor = rijndael.CreateEncryptor();
                encryptedKeyId = new byte[KeyIdLength];
                encryptor.TransformBlock(keyId.ToByteArray(), 0, KeyIdLength, encryptedKeyId, 0);
            }

            byte[] retVal = new byte[ChecksumLength];
            Array.Copy(encryptedKeyId, retVal, ChecksumLength);

            return Convert.ToBase64String(retVal);
        }

        /// <summary>
        /// Overwrites the supplied byte array with RNG generated data which destroys the original contents.
        /// </summary>
        /// <param name="keyToErase">The content key to erase.</param>
        public static void EraseKey(byte[] keyToErase)
        {
            if (keyToErase != null)
            {
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(keyToErase);
                }
            }
        }
    }
}
