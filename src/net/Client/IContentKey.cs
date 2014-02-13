//-----------------------------------------------------------------------
// <copyright file="IContentKey.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a content key that can be used for encryption and decryption.
    /// </summary>
    public partial interface IContentKey
    {
        /// <summary>
        /// Asynchronously gets the decrypted content key value.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;byte[]&gt;.</returns>
        Task<byte[]> GetClearKeyValueAsync();

        /// <summary>
        /// Gets the decrypted content key value.
        /// </summary>
        /// <returns>The decrypted key value used for encryption.</returns>
        byte[] GetClearKeyValue();

        /// <summary>
        /// Asynchronously gets the encrypted content key value.
        /// </summary>
        /// <param name="certToEncryptTo">The <see cref="X509Certificate2"/> to protect the key with.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;byte[]&gt;.</returns>
        Task<byte[]> GetEncryptedKeyValueAsync(X509Certificate2 certToEncryptTo);

        /// <summary>
        /// Gets the encrypted content key value.
        /// </summary>
        /// <param name="certToEncryptTo">The <see cref="X509Certificate2"/> to protect the key with.</param>
        /// <returns>The encrypted content key value.</returns>
        byte[] GetEncryptedKeyValue(X509Certificate2 certToEncryptTo);

        /// <summary>
        /// Asynchronously deletes the content key.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes the content key.
        /// </summary>
        void Delete();

        /// <summary>
        /// Asynchronously updates the content key.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task<IContentKey> UpdateAsync();

        /// <summary>
        /// Updates the content key.
        /// </summary>
        void Update();

        /// <summary>
        /// Asynchronously gets the Key Delivery Uri to give to clients to request the content key for content playback.
        /// </summary>
        /// <param name="contentKeyDeliveryType">The type of key delivery to get a Uri for</param>
        /// <returns>Uri for clients to request the key from</returns>
        Task<Uri> GetKeyDeliveryUrlAsync(ContentKeyDeliveryType contentKeyDeliveryType);

        /// <summary>
        /// Gets the Key Delivery Uri to give to clients to request the content key for content playback.
        /// </summary>
        /// <param name="contentKeyDeliveryType">The type of key delivery to get a Uri for</param>
        /// <returns>Uri for clients to request the key from</returns>
        Uri GetKeyDeliveryUrl(ContentKeyDeliveryType contentKeyDeliveryType);
    }
}
