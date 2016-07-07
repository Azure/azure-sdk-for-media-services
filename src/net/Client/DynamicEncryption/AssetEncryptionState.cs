//-----------------------------------------------------------------------
// <copyright file="AssetEncryptionState.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    public enum AssetEncryptionState
    {
        /// <summary>
        /// The Asset is not in a supported state for static or dynamic encryption.  This could be due to the AssetType or the AssetType and Options being incompatible.
        /// </summary>
        Unsupported,

        /// <summary>
        /// The Asset is blocked for the requested AssetDeliveryProtocol values.  This could be due to an AssetDeliveryPolicy with an AssetDeliveryPolicyType of blocked
        /// for the AssetDeliveryProtocol values or no policy being configured for the AssetDeliveryProtocol (unconfigured protocols default to blocked).
        /// </summary>
        BlockedByPolicy,

        /// <summary>
        /// No Single AssetDeliveryPolicy applies to all of the requested AssetDeliveryProtocol values.  This could be because multiple policies are configured or
        /// that only some of the AssetDeliveryProtocol values configured are covered by explicit policies.  Examine the IAsset.AssetDeliveryPolicies collection
        /// directly for further details.
        /// </summary>
        NoSinglePolicyApplies,

        /// <summary>
        /// The Asset has no delivery policy configured but it will stream in the clear using the default policy.
        /// </summary>
        ClearOutput,

        /// <summary>
        /// The Asset has the AssetCreationOption.StorageEncrypted option but does not have an AssetDeliveryPolicy configured.  An AssetDeliveryPolicy must be configured to stream content.
        /// </summary>
        StorageEncryptedWithNoDeliveryPolicy,

        /// <summary>
        /// The Asset has the AssetCreationOption.EnvelopeEncryptionProtected option but does not have an AssetDeliveryPolicy configured.
        /// </summary>
        StaticEnvelopeEncryption,

        /// <summary>
        /// The Asset has the AssetCreationOption.CommonEncryptionProtected option but does not have an AssetDeliveryPolicy configured.
        /// </summary>
        StaticCommonEncryption,

        /// <summary>
        /// The Asset has an AssetDeliveryPolicy with an AssetDeliveryPolicyType of NoDynamicEncryption configured for the requested AssetDeliveryProtocol value.
        /// </summary>
        NoDynamicEncryption,

        /// <summary>
        /// The Asset has an AssetDeliveryPolicy with an AssetDeliveryPolicyType of DynamicEnvelopeEncryption configured for the requested AssetDeliveryProtocol value.
        /// </summary>
        DynamicEnvelopeEncryption,

        /// <summary>
        /// The Asset has an AssetDeliveryPolicy with an AssetDeliveryPolicyType of DynamicCommonEncryption configured for the requested AssetDeliveryProtocol value.
        /// </summary>
        DynamicCommonEncryption,

        /// <summary>
        /// The Asset has an AssetDeliveryPolicy with an AssetDeliveryPolicyType of DynamicCommonEncryptionCbcs configured for the requested AssetDeliveryProtocol value.
        /// </summary>
        DynamicCommonEncryptionCbcs
    }
}
