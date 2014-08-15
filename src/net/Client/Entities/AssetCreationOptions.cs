//-----------------------------------------------------------------------
// <copyright file="AssetCreationOptions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies the the different options that an <see cref="IAsset"/> can be created with.
    /// </summary>
    /// <remarks>This is a flags enum.</remarks>
    [Flags]
    public enum AssetCreationOptions
    {
        /// <summary>
        /// Specifies no asset creation options.
        /// </summary>
        /// <remarks>This is the default value.</remarks>
        None = 0x0,

        /// <summary>
        /// Specifies that an asset's files should be encrypted on for upload and storage when creating the asset.
        /// </summary>
        StorageEncrypted = 0x1,

        /// <summary>
        /// Specifies that an assets files are protected using a common encryption method (such as PlayReady).
        /// </summary>
        CommonEncryptionProtected = 0x2,

        /// <summary>
        /// Specifies that an assets files are protected using a Envelope encryption method (such as AES-CBC).
        /// </summary>
        EnvelopeEncryptionProtected = 0x4
    }
}
