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
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

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
                        if (asset.DeliveryPolicies.Any(p => ((p.AssetDeliveryProtocol & protocolsToCheck) != 0)))
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
                        switch (policy.AssetDeliveryPolicyType)
                        {
                            case AssetDeliveryPolicyType.Blocked:
                                returnValue = AssetEncryptionState.BlockedByPolicy;
                                break;
                            case AssetDeliveryPolicyType.NoDynamicEncryption:
                                returnValue = AssetEncryptionState.NoDynamicEncryption;
                                break;
                            case AssetDeliveryPolicyType.DynamicCommonEncryption:
                                if (((asset.AssetType == AssetType.SmoothStreaming) || (asset.AssetType == AssetType.MultiBitrateMP4)) &&
                                    ((asset.Options == AssetCreationOptions.StorageEncrypted) || (asset.Options == AssetCreationOptions.None)))
                                {
                                    returnValue = AssetEncryptionState.DynamicCommonEncryption;
                                }
                                break;
                            case AssetDeliveryPolicyType.DynamicCommonEncryptionCbcs:
                                if (((asset.AssetType == AssetType.SmoothStreaming) || (asset.AssetType == AssetType.MultiBitrateMP4)) &&
                                    ((asset.Options == AssetCreationOptions.StorageEncrypted) || (asset.Options == AssetCreationOptions.None)))
                                {
                                    returnValue = AssetEncryptionState.DynamicCommonEncryptionCbcs;
                                }
                                break;
                            case AssetDeliveryPolicyType.DynamicEnvelopeEncryption:
                                if (((asset.AssetType == AssetType.SmoothStreaming) || (asset.AssetType == AssetType.MultiBitrateMP4)) &&
                                    ((asset.Options == AssetCreationOptions.StorageEncrypted) || (asset.Options == AssetCreationOptions.None)))
                                {
                                    returnValue = AssetEncryptionState.DynamicEnvelopeEncryption;
                                }
                                break;
                            case AssetDeliveryPolicyType.None:
                            default:
                                break;
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

                case AssetType.SmoothStreaming:
                case AssetType.MultiBitrateMP4:
                    return true;

                default:
                    throw new ArgumentException("Unsupported AssetType");
            }
        }

        private static bool IsStreamable(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.MP4:
                case AssetType.Unknown:
                    return false;

                case AssetType.SmoothStreaming:
                case AssetType.MultiBitrateMP4:
                case AssetType.MediaServicesHLS:
                    return true;

                default:
                    throw new ArgumentException("Unsupported AssetType");
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

            // Take the first asset file marked as primary 
            IAssetFile primaryFile =  asset.AssetFiles.Where(af => af.IsPrimary).FirstOrDefault();

            //If the primary file is not set and only 1 file is present in asset, return that file as primary
            //Fyi.. We are not doing update of assetFile via rest here.
            if ((primaryFile == null) && ((asset.AssetFiles.Count() == 1)))
            {
                primaryFile = asset.AssetFiles.First();
            }
            return primaryFile;
        }

        private static bool IsExtension(string filepath, string extensionToCheck)
        {
            string extension = Path.GetExtension(filepath);

            return (0 == String.Compare(extension, extensionToCheck, StringComparison.OrdinalIgnoreCase));
        }

        private static AssetType GetAssetType(IAsset asset)
        {
            // If there is no primary asset file then default to AssetType.Unknown. 
            AssetType assetType = AssetType.Unknown;

            IAssetFile primaryFile = GetPrimaryFile(asset);

            // Adding null check to prevent NullReferenceException.
            if (primaryFile != null)
            {
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

                    if (assetFiles.Length == 1)
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
                    else if (assetFiles.Where(af => IsExtension(af.Name, ".m3u8")).Any())
                    {
                        assetType = AssetType.MediaServicesHLS;
                    }
                    else if (assetFiles.Any(af => af.AssetFileOptions.HasFlag(AssetFileOptions.Fragmented))
                             || assetFiles.Any(af => IsExtension(af.Name, ".ismv"))
                             || assetFiles.Any(af => IsExtension(af.Name, ".isma")))
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
            }

            return assetType;
        }
    }
}
