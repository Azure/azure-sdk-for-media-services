//-----------------------------------------------------------------------
// <copyright file="AssetBaseCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

    /// <summary>
    /// Represents the base of all asset collections.
    /// </summary>
    public abstract class AssetBaseCollection : BaseCollection<IAsset>
    {
        protected AssetBaseCollection(MediaContextBase context) : base(context)
        {
        }

        /// <summary>
        /// Asynchronously creates an asset that does not contain any files and <see cref="AssetState"/> is Initialized.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="options">A <see cref="AssetCreationOptions"/> which will be associated with created asset.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="Task"/> of type <see cref="IAsset"/>, where IAsset created according to the specified creation <paramref name="options"/>.
        /// </returns>
        public abstract Task<IAsset> CreateAsync(string assetName, AssetCreationOptions options,CancellationToken cancellationToken);

        /// <summary>
        /// Creates an asset that does not contain any files and <see cref="AssetState"/> is Initialized. 
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="options">A <see cref="AssetCreationOptions"/> which will be associated with created asset.</param>
        /// <returns>The created asset.</returns>
        public abstract IAsset Create(string assetName, AssetCreationOptions options);


        /// <summary>
        /// Asynchronously creates an asset for specified storage account. Asset  does not contain any files and <see cref="AssetState"/> is Initialized.
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="storageAccountName">The storage account name</param>
        /// <param name="options">A <see cref="AssetCreationOptions"/> which will be associated with created asset.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// An <see cref="Task"/> of type <see cref="IAsset"/>, where IAsset created according to the specified creation <paramref name="options"/>.
        /// </returns>
        public abstract Task<IAsset> CreateAsync(string assetName,string storageAccountName, AssetCreationOptions options, CancellationToken cancellationToken);

        /// <summary>
        /// Creates an asset for specified storage account. Asset does not contain any files and <see cref="AssetState"/> is Initialized. 
        /// </summary>
        /// <param name="assetName">The asset name.</param>
        /// <param name="storageAccountName"></param>
        /// <param name="options">A <see cref="AssetCreationOptions"/> which will be associated with created asset.</param>
        /// <returns>The created asset.</returns>
        public abstract IAsset Create(string assetName, string storageAccountName, AssetCreationOptions options);
        
        
   

        /// <summary>
        /// Verifies the asset.
        /// </summary>
        /// <param name="asset">The asset to verify.</param>
        internal static void VerifyAsset(IAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset");
            }

            if (!(asset is AssetData))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidAssetType);
            }
        }

        /// <summary>
        /// Sets the file info for envelope encryption.
        /// </summary>
        /// <param name="file">The file info to update.</param>
        internal static void SetAssetFileForEnvelopeEncryption(AssetFileData file)
        {
            file.IsEncrypted = true;
            file.EncryptionScheme = EnvelopeEncryption.SchemeName;
            file.EncryptionVersion = EnvelopeEncryption.SchemeVersion;
        }        

        /// <summary>
        /// Sets the file info for common encryption.
        /// </summary>
        /// <param name="file">The file info to update.</param>
        internal static void SetAssetFileForCommonEncryption(AssetFileData file)
        {
            file.IsEncrypted = true;
            file.EncryptionScheme = CommonEncryption.SchemeName;
            file.EncryptionVersion = CommonEncryption.SchemeVersion;
        }



        /// <summary>
        /// Adds the encryption metadata to the file info.
        /// </summary>
        /// <param name="file">The file information to update.</param>
        /// <param name="fileEncryption">The file encryption to use.</param>
        internal static void AddEncryptionMetadataToAssetFile(AssetFileData file, FileEncryption fileEncryption)
        {
            ulong iv = fileEncryption.GetInitializationVectorForFile(file.Name);

            file.IsEncrypted = true;
            file.EncryptionKeyId = fileEncryption.GetKeyIdentifierAsString();
            file.EncryptionScheme = FileEncryption.SchemeName;
            file.EncryptionVersion = FileEncryption.SchemeVersion;
            file.InitializationVector = iv.ToString(CultureInfo.InvariantCulture);
        }

    }
}
