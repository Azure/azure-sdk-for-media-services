//-----------------------------------------------------------------------
// <copyright file="ConfigurationEncryptionHelper.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
     /// <summary>
    /// A helper class to encrypt and decrypt configuration strings.
    /// </summary>
    internal static class ConfigurationEncryptionHelper
    {
        /// <summary>
        /// Decrypts the configuration string.
        /// </summary>
        /// <param name="cloudMediaContext">The cloud media context.</param>
        /// <param name="encryptionKeyId">The encryption key id.</param>
        /// <param name="initializationVector">The initialization vector.</param>
        /// <param name="encryptedConfiguration">The encrypted configuration.</param>
        /// <returns>The decrypted configuration.</returns>
        internal static string DecryptConfigurationString(MediaContextBase cloudMediaContext, string encryptionKeyId, string initializationVector, string encryptedConfiguration)
        {
            if (cloudMediaContext == null)
            {
                throw new ArgumentNullException("cloudMediaContext");
            }

            if (string.IsNullOrEmpty(encryptionKeyId))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "encryption key identifier"), "encryptionKeyId");
            }

            if (string.IsNullOrEmpty(initializationVector))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "initialization vector"), "initializationVector");
            }

            if (string.IsNullOrEmpty(encryptedConfiguration))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "encrypted configuration"), "encryptedConfiguration");
            }

            string returnValue;
            Guid keyId = EncryptionUtils.GetKeyIdAsGuid(encryptionKeyId);

            byte[] iv = Convert.FromBase64String(initializationVector);

            IContentKey configKey = cloudMediaContext.ContentKeys.Where(c => c.Id == encryptionKeyId).Single();
            byte[] contentKey = configKey.GetClearKeyValue();

            using (ConfigurationEncryption configEnc = new ConfigurationEncryption(keyId, contentKey, iv))
            {
                returnValue = configEnc.Decrypt(encryptedConfiguration);
            }

            return returnValue;
        }
    }
}