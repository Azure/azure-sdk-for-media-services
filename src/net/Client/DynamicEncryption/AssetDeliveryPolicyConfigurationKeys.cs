//-----------------------------------------------------------------------
// <copyright file="AssetDeliveryPolicyConfigurationKeys.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption
{
    /// <summary>
    /// Keys used to get specific configuration for an asset delivery policy.
    /// </summary>
    public enum AssetDeliveryPolicyConfigurationKey
    {
        /// <summary>
        /// No policies.
        /// </summary>
        None,

        /// <summary>
        /// Exact Envelope key URL.
        /// </summary>
        EnvelopeKeyAcquisitionUrl,

        /// <summary>
        /// Base key URL that will have KID=<Guid> appended for Envelope.
        /// </summary>
        EnvelopeBaseKeyAcquisitionUrl,
        
        /// <summary>
        /// The initialization vector to use for envelope encryption in Base64 format.
        /// </summary>
        EnvelopeEncryptionIVAsBase64,

        /// <summary>
        /// The PlayReady License Acquisition URL to use for common encryption.
        /// </summary>
        PlayReadyLicenseAcquisitionUrl,

        /// <summary>
        /// The PlayReady Custom Attributes to add to the PlayReady Content Header
        /// </summary>
        PlayReadyCustomAttributes,

        /// <summary>
        /// The initialization vector to use for envelope encryption.
        /// </summary>
        EnvelopeEncryptionIV,

        /// <summary>
        /// Widevine DRM acquisition URL
        /// </summary>
        WidevineLicenseAcquisitionUrl,

        /// <summary>
        /// Base Widevine URL that will have KID=<Guid> appended.
        /// </summary>
        WidevineBaseLicenseAcquisitionUrl,

        /// <summary>
        /// FairPlay license acquisition URL.
        /// </summary>
        FairPlayLicenseAcquisitionUrl,

        /// <summary>
        /// Base FairPlay license acquisition URL that will have KID=<Guid> appended.
        /// </summary>
        FairPlayBaseLicenseAcquisitionUrl,

        /// <summary>
        /// Initialization Vector that will be used for encrypting the content. Must match
        /// IV in the AssetDeliveryPolicy.
        /// </summary>
        CommonEncryptionIVForCbcs,

        /// <summary>
        /// FourCCs that will be streamed unecrypted.
        /// </summary>
        UnencryptedTracksByFourCC
    }
}
