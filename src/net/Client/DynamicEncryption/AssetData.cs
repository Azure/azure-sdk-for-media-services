//-----------------------------------------------------------------------
// <copyright file="AssetData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.ObjectModel;
using System.Data.Services.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset that can be an input to jobs or tasks.
    /// </summary>
    internal partial class AssetData : BaseEntity<IAsset>, IAsset
    {
        private IList<IAssetDeliveryPolicy> _deliveryPolicyCollection;

        /// <summary>
        /// Gets the delivery policies associated with the asset.
        /// </summary>
        /// <value>A collection of <see cref="IAssetDeliveryPolicy"/> associated with the Asset.</value>
        public List<AssetDeliveryPolicyData> DeliveryPolicies { get; set; }

        IList<IAssetDeliveryPolicy> IAsset.DeliveryPolicies
        {
            get
            {
                lock (_deliveryPolicyLocker)
                {
                    if ((this._deliveryPolicyCollection == null) && !string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this._mediaContextBase.MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(AssetCollection.AssetSet, this);
                        LoadProperty(dataContext, DeliveryPoliciesPropertyName);

                        this._deliveryPolicyCollection = new LinkCollection<IAssetDeliveryPolicy, AssetDeliveryPolicyData>(dataContext, this, DeliveryPoliciesPropertyName, this.DeliveryPolicies);
                    }

                    return this._deliveryPolicyCollection;
                }
            }
        }

        /// <summary>
        /// Invalidates the content key collection.
        /// </summary>
        internal void InvalidateDeliveryPoliciesCollection()
        {
            this.DeliveryPolicies.Clear();
            this._deliveryPolicyCollection = null;
        }

        AssetEncryptionState IAsset.GetEncryptionState(AssetDeliveryProtocol protocolsToCheck)
        {
            IAsset asset = (IAsset)this;

            AssetEncryptionState returnValue = AssetEncryptionState.Unsupported;

            if (asset.IsStreamable)
            {
                if (asset.DeliveryPolicies.Count == 0)
                {
                    if ((asset.Options == AssetCreationOptions.EnvelopeEncryptionProtected) &&
                        (asset.AssetType == AssetType.MediaServicesHLS))
                    {
                        returnValue = AssetEncryptionState.StaticEnvelopeEncryption;
                    }
                    else if (asset.Options == AssetCreationOptions.CommonEncryptionProtected &&
                             (asset.AssetType == AssetType.MediaServicesHLS || asset.AssetType == AssetType.SmoothStreaming))
                    {
                        returnValue = AssetEncryptionState.StaticCommonEncryption;
                    }
                    else if (asset.Options == AssetCreationOptions.None)
                    {
                        returnValue = AssetEncryptionState.ClearOutput;
                    }
                    else if (asset.Options == AssetCreationOptions.StorageEncrypted)
                    {
                        returnValue = AssetEncryptionState.StorageEncryptedWithNoDeliveryPolicy;
                    }
                }
                else
                {
                    IAssetDeliveryPolicy policy = asset.DeliveryPolicies.Where(p => p.AssetDeliveryProtocol.HasFlag(protocolsToCheck)).FirstOrDefault();

                    if (policy == null)
                    {
                        List<AssetDeliveryProtocol> individualProtocols = GetIndividualProtocols(protocolsToCheck);

                        bool partialMatch = false;

                        foreach (AssetDeliveryProtocol protocol in individualProtocols)
                        {
                            if (asset.DeliveryPolicies.Any(p => p.AssetDeliveryProtocol.HasFlag(protocol)))
                            {
                                partialMatch = true;
                                break;
                            }
                        }

                        if (partialMatch)
                        {
                            returnValue = AssetEncryptionState.NoSinglePolicyApplies;
                        }
                        else
                        {
                            returnValue = AssetEncryptionState.BlockedByPolicy;
                        }
                    }
                    else
                    {
                        if (policy.AssetDeliveryPolicyType == AssetDeliveryPolicyType.Blocked)
                        {
                            returnValue = AssetEncryptionState.BlockedByPolicy;
                        }
                        else if (policy.AssetDeliveryPolicyType == AssetDeliveryPolicyType.NoDynamicEncryption)
                        {
                            returnValue = AssetEncryptionState.NoDynamicEncryption;
                        }
                        else if (policy.AssetDeliveryPolicyType == AssetDeliveryPolicyType.DynamicCommonEncryption)
                        {
                            if (((asset.AssetType == AssetType.SmoothStreaming) || (asset.AssetType == AssetType.MultiBitrateMP4)) &&
                                ((asset.Options == AssetCreationOptions.StorageEncrypted) || (asset.Options == AssetCreationOptions.None)))
                            {
                                returnValue = AssetEncryptionState.DynamicCommonEncryption;
                            }
                        }
                        else if (policy.AssetDeliveryPolicyType == AssetDeliveryPolicyType.DynamicEnvelopeEncryption)
                        {
                            if (((asset.AssetType == AssetType.SmoothStreaming) || (asset.AssetType == AssetType.MultiBitrateMP4)) &&
                                ((asset.Options == AssetCreationOptions.StorageEncrypted) || (asset.Options == AssetCreationOptions.None)))
                            {
                                returnValue = AssetEncryptionState.DynamicEnvelopeEncryption;
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        bool IAsset.IsStreamable
        {
            get
            {
                return IsStreamable(((IAsset)this).AssetType);
            }
        }

        bool IAsset.SupportsDynamicEncryption
        {
            get
            {
                return SupportsDynamicEncryption(((IAsset)this).AssetType);
            }
        }

        private static bool SupportsDynamicEncryption(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.MP4:
                case AssetType.Unknown:
                case AssetType.MediaServicesHLS:
                    return false;

                default:
                    return true;
            }
        }

        private static bool IsStreamable(AssetType assetType)
        { 
            switch (assetType)
            {
                case AssetType.MP4:
                case AssetType.Unknown:
                    return false;

                default:
                    return true;
            }
        }

        AssetType IAsset.AssetType
        {
            get
            {
                return GetAssetType((IAsset)this);
            }
        }

        private static IAssetFile GetPrimaryFile(IAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException("asset");
            }

            return asset.AssetFiles.Where(af => af.IsPrimary == true).SingleOrDefault();
        }

        private static bool IsExtension(string filepath, string extensionToCheck)
        {
            string extension = Path.GetExtension(filepath);

            return (0 == String.Compare(extension, extensionToCheck, StringComparison.OrdinalIgnoreCase));
        }

        private static AssetType GetAssetType(IAsset asset)
        {
            AssetType assetType = AssetType.Unknown;

            IAssetFile primaryFile = GetPrimaryFile(asset);

            if (IsExtension(primaryFile.Name, ".mp4"))
            {
                if (asset.Options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected) ||
                    asset.Options.HasFlag(AssetCreationOptions.CommonEncryptionProtected))
                {
                    // We have no supported cases where Encryption is statically applied to an MBR MP4 fileset.
                    assetType = AssetType.Unknown;
                }
                else
                {
                    assetType = AssetType.MP4;
                }
            }
            else if (IsExtension(primaryFile.Name, ".ism"))
            {
                IAssetFile[] assetFiles = asset.AssetFiles.ToArray();

                if (assetFiles.Where(af => IsExtension(af.Name, ".m3u8")).Any())
                {
                    assetType = AssetType.MediaServicesHLS;
                }
                else if (assetFiles.Where(af => IsExtension(af.Name, ".ismc")).Any())
                {
                    if (asset.Options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected))
                    {
                        // We have no supported cases where Envelope Encryption is statically applied to a smooth streaming file.
                        assetType = AssetType.Unknown;
                    }
                    else
                    {
                        assetType = AssetType.SmoothStreaming;
                    }
                }
                else if (assetFiles.Where(af => IsExtension(af.Name, ".mp4")).Any())
                {
                    if (asset.Options.HasFlag(AssetCreationOptions.EnvelopeEncryptionProtected) ||
                        asset.Options.HasFlag(AssetCreationOptions.CommonEncryptionProtected))
                    {
                        // We have no supported cases where Encryption is statically applied to an MBR MP4 fileset.
                        assetType = AssetType.Unknown;
                    }
                    else
                    {
                        assetType = AssetType.MultiBitrateMP4;
                    }
                }
            }

            return assetType;
        }

        static AssetDeliveryProtocol[] _allValues = null;
        internal static List<AssetDeliveryProtocol> GetIndividualProtocols(AssetDeliveryProtocol protocolsToSplit)
        {
            List<AssetDeliveryProtocol> protocolList = new List<AssetDeliveryProtocol>();

            if (_allValues == null)
            {
                _allValues = (AssetDeliveryProtocol[])Enum.GetValues(typeof(AssetDeliveryProtocol));
            }

            foreach (AssetDeliveryProtocol protocol in _allValues)
            {
                if ((protocol == AssetDeliveryProtocol.None) || (protocol == AssetDeliveryProtocol.All))
                {
                    continue;
                }

                if (protocolsToSplit.HasFlag(protocol))
                {
                    protocolList.Add(protocol);
                }
            }

            return protocolList;
        }
    }
}
