//-----------------------------------------------------------------------
// <copyright file="MediaContextBase.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a base media context containing collections to operate on.
    /// </summary>
    public abstract partial class MediaContextBase
    {
        /// <summary>
        /// Gets Microsoft WindowsAzure Media Services credentials used for authenticating requests.
        /// </summary>
        public MediaServicesCredentials Credentials
        {
            get
            {
                return TokenProvider as MediaServicesCredentials;
            }
        }


        /// <summary>
        /// A token provider to get authorization tokens for Azure Media Services.
        /// </summary>
        public ITokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Gets a collection to operate on AccessPolicies.
        /// </summary>
        /// <seealso cref="AccessPolicyBaseCollection" />
        /// <seealso cref="IAccessPolicy" />
        public abstract AccessPolicyBaseCollection AccessPolicies { get; }

        /// <summary>
        ///   Gets a collection to operate on Assets.
        /// </summary>
        /// <seealso cref="AssetBaseCollection" />
        /// <seealso cref="IAsset" />
        public abstract AssetBaseCollection Assets { get; }

        /// <summary>
        ///   Gets a collection to operate on ContentKeys.
        /// </summary>
        /// <seealso cref="ContentKeyBaseCollection" />
        /// <seealso cref="IContentKey" />
        public abstract ContentKeyBaseCollection ContentKeys { get; }

        /// <summary>
        ///   Gets a collection to operate on Files.
        /// </summary>
        /// <seealso cref="AssetFileBaseCollection" />
        /// <seealso cref="IAssetFile" />
        public abstract AssetFileBaseCollection Files { get; }

        /// <summary>
        ///   Gets a collection to operate on Jobs.
        /// </summary>
        /// <seealso cref="JobBaseCollection" />
        /// <seealso cref="IJob" />
        public abstract JobBaseCollection Jobs { get; }

        /// <summary>
        ///   Gets a collection to operate on JobTemplates.
        /// </summary>
        /// <seealso cref="JobTemplateBaseCollection" />
        /// <seealso cref="IJobTemplate" />
        public abstract JobTemplateBaseCollection JobTemplates { get; }

        /// <summary>
        ///   Gets a collection to operate on MediaProcessors.
        /// </summary>
        /// <seealso cref="MediaProcessorBaseCollection" />
        /// <seealso cref="IMediaProcessor" />
        public abstract MediaProcessorBaseCollection MediaProcessors { get; }

        /// <summary>
        ///   Gets a collection to operate on StorageAccounts.
        /// </summary>
        /// <seealso cref="StorageAccountBaseCollection" />
        /// <seealso cref="IStorageAccount" />
        public abstract StorageAccountBaseCollection StorageAccounts { get; }

        /// <summary>
        ///   Gets a collection to operate on EncodingReservedUnits.
        /// </summary>
        /// <seealso cref="EncodingReservedUnitCollection" />
        /// <seealso cref="IEncodingReservedUnit" />
        public abstract EncodingReservedUnitCollection EncodingReservedUnits { get; }

        /// <summary>
        /// Returns default storage account
        /// </summary>
        public abstract IStorageAccount DefaultStorageAccount { get; }

        /// <summary>
        /// Gets the collection of notification endpoints available in the system.
        /// </summary>
        public abstract NotificationEndPointCollection NotificationEndPoints { get; }


        /// <summary>
        /// Gets or sets a factory for creating data service context instances prepared for Windows Azure Media Services.
        /// </summary>
        public virtual MediaServicesClassFactory MediaServicesClassFactory { get; set; }

        public abstract IngestManifestFileCollection IngestManifestFiles { get; }
        public abstract IngestManifestCollection IngestManifests { get; }
        public abstract IngestManifestAssetCollection IngestManifestAssets { get; }
        public abstract LocatorBaseCollection Locators { get; }
        public abstract StreamingFilterBaseCollection Filters { get; }

        /// <summary>
        /// Gets the collection of monitoring configuration available in the system.
        /// </summary>
        public abstract MonitoringConfigurationCollection MonitoringConfigurations { get; }

        /// <summary>
        /// Gets the collection of channel metrics available in the system.
        /// </summary>
        public abstract ChannelMetricsCollection ChannelMetrics { get; }

        /// <summary>
        /// Gets the collection of streaming endpoint metrics available in the system.
        /// </summary>
        public abstract StreamingEndPointRequestLogCollection StreamingEndPointRequestLogs { get; }

        /// <summary>
        /// Gets or sets the number of threads to use to for each blob transfer.
        /// </summary>
        /// <remarks>The default value is 10.</remarks>
        public int ParallelTransferThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the number of concurrent blob transfers allowed.
        /// </summary>
        /// <remarks>The default value is 2.</remarks>
        public int NumberOfConcurrentTransfers { get; set; }
    }
}