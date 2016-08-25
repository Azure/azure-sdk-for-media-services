//-----------------------------------------------------------------------
// <copyright file="CloudMediaContext.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Threading;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the context from which all entities in the Microsoft WindowsAzure Media Services platform can be accessed.
    /// </summary>
    public partial class CloudMediaContext : MediaContextBase
    {
        private static readonly Uri _mediaServicesUri = new Uri("https://media.windows.net/");

        private AssetCollection _assets;
        private AssetFileCollection _files;
        private AccessPolicyBaseCollection _accessPolicies;
        private ContentKeyCollection _contentKeys;
        private JobBaseCollection _jobs;
        private JobTemplateBaseCollection _jobTemplates;
        private NotificationEndPointCollection _notificationEndPoints;
        private MediaProcessorBaseCollection _mediaProcessors;
        private LocatorBaseCollection _locators;
        private IngestManifestCollection _ingestManifests;
        private IngestManifestAssetCollection _ingestManifestAssets;
        private IngestManifestFileCollection _ingestManifestFiles;
        private StorageAccountBaseCollection _storageAccounts;
        private EncodingReservedUnitCollection _encodingReservedUnits;
        private MediaServicesClassFactory _classFactory;
        private Uri _apiServer;
        private StreamingFilterBaseCollection _streamingFilters;
        private MonitoringConfigurationCollection _monitoringConfigurations;
        private ChannelMetricsCollection _channelMetrics;
        private StreamingEndPointRequestLogCollection _streamingEndPointRequestLogs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="accountName">The Microsoft WindowsAzure Media Services account name to authenticate with.</param>
        /// <param name="accountKey">The Microsoft WindowsAzure Media Services account key to authenticate with.</param>
        [Obsolete("Use the constructor that takes an ITokenProvider")]
        public CloudMediaContext(string accountName, string accountKey)
            : this(_mediaServicesUri, new MediaServicesCredentials(accountName, accountKey))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="apiServer">A <see cref="Uri"/> representing a the API endpoint.</param>
        /// <param name="accountName">The Microsoft WindowsAzure Media Services account name to authenticate with.</param>
        /// <param name="accountKey">The Microsoft WindowsAzure Media Services account key to authenticate with.</param>
        [Obsolete("Use the constructor that takes an ITokenProvider")]
        public CloudMediaContext(Uri apiServer, string accountName, string accountKey)
            : this(apiServer, new MediaServicesCredentials(accountName, accountKey))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="apiServer">A <see cref="Uri"/> representing a the API endpoint.</param>
        /// <param name="accountName">The Microsoft WindowsAzure Media Services account name to authenticate with.</param>
        /// <param name="accountKey">The Microsoft WindowsAzure Media Services account key to authenticate with.</param>
        /// <param name="scope">The scope of authorization.</param>
        /// <param name="acsBaseAddress">The access control endpoint to authenticate against.</param>
        [Obsolete("Use the constructor that takes an ITokenProvider")]
        public CloudMediaContext(Uri apiServer, string accountName, string accountKey, string scope, string acsBaseAddress)
            : this(apiServer, new MediaServicesCredentials(accountName, accountKey, scope, acsBaseAddress))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="tokenProvider">A token provider for authorization tokens</param>
        public CloudMediaContext(ITokenProvider tokenProvider):
            this(_mediaServicesUri, tokenProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="apiServer">A <see cref="Uri"/> representing the API endpoint.</param>
        /// <param name="tokenProvider">A token provider for authorization tokens.</param>
        public CloudMediaContext(Uri apiServer, ITokenProvider tokenProvider)
        {
            _apiServer = apiServer;
            TokenProvider = tokenProvider;
            ParallelTransferThreadCount = 10;
            NumberOfConcurrentTransfers = 2;
        }

        public override MediaServicesClassFactory MediaServicesClassFactory
        {
            get
            {
                if (_classFactory == null)
                {
                    Interlocked.CompareExchange(ref _classFactory, new AzureMediaServicesClassFactory(_apiServer, this), null);
                }
                return _classFactory;
            }
            set
            {
                _classFactory = value;
            }
        }
        
        /// <summary>
        /// Gets the collection of assets in the system.
        /// </summary>
        public override AssetBaseCollection Assets
        {
            get
            {
                if (_assets == null)
                {
                    Interlocked.CompareExchange(ref _assets, new AssetCollection(this), null);
                }
                return this._assets;

            }
        }

        /// <summary>
        /// Gets the collection of files in the system.
        /// </summary>
        public override AssetFileBaseCollection Files
        {
            get
            {
                if (_files == null)
                {
                    Interlocked.CompareExchange(ref _files, new AssetFileCollection(this), null);
                }
                return this._files;

            }
        }

        /// <summary>
        /// Gets the collection of access policies in the system.
        /// </summary>
        public override AccessPolicyBaseCollection AccessPolicies
        {
            get
            {
                if (_accessPolicies == null)
                {
                    Interlocked.CompareExchange(ref _accessPolicies, new AccessPolicyBaseCollection(this), null);
                }
                return this._accessPolicies;

            }
        }

        /// <summary>
        /// Gets the collection of content keys in the system.
        /// </summary>
        public override ContentKeyBaseCollection ContentKeys
        {
            get
            {
                if (_contentKeys == null)
                {
                    Interlocked.CompareExchange(ref _contentKeys, new ContentKeyCollection(this), null);
                }
                return this._contentKeys;

            }
        }

        /// <summary>
        /// Gets the collection of jobs available in the system.
        /// </summary>
        public override JobBaseCollection Jobs
        {
            get
            {
                if (_jobs == null)
                {
                    Interlocked.CompareExchange(ref _jobs, new JobBaseCollection(this), null);
                }
                return this._jobs;
            }
        }

        /// <summary>
        /// Gets the collection of job templates available in the system.
        /// </summary>
        public override JobTemplateBaseCollection JobTemplates
        {
            get
            {
                if (_jobTemplates == null)
                {
                    Interlocked.CompareExchange(ref _jobTemplates, new JobTemplateBaseCollection(this), null);
                }
                return this._jobTemplates;
            }
        }

        /// <summary>
        /// Gets the collection of media processors available in the system.
        /// </summary>
        public override MediaProcessorBaseCollection MediaProcessors
        {
            get
            {
                if (_mediaProcessors == null)
                {
                    Interlocked.CompareExchange(ref _mediaProcessors, new MediaProcessorBaseCollection(this), null);
                }
                return this._mediaProcessors;

            }
        }

        /// <summary>
        /// Gets a collection to operate on StorageAccounts.
        /// </summary>
        /// <seealso cref="StorageAccountBaseCollection" />
        ///   <seealso cref="IStorageAccount" />
        public override StorageAccountBaseCollection StorageAccounts
        {
            get
            {
                if (_storageAccounts == null)
                {
                    Interlocked.CompareExchange(ref _storageAccounts, new StorageAccountBaseCollection(this), null);
                }
                return this._storageAccounts;

            }
        }

        /// <summary>
        /// Returns default storage account
        /// </summary>
        public override IStorageAccount DefaultStorageAccount
        {
            get
            {
                return this.StorageAccounts.Where(c => c.IsDefault == true).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the collection of notification endpoints available in the system.
        /// </summary>
        public override NotificationEndPointCollection NotificationEndPoints
        {
            get
            {
                if (_notificationEndPoints == null)
                {
                    Interlocked.CompareExchange(ref _notificationEndPoints, new NotificationEndPointCollection(this), null);
                }
                return this._notificationEndPoints;

            }
        }

        /// <summary>
        /// Gets the collection of locators in the system.
        /// </summary>
        public override LocatorBaseCollection Locators
        {
            get
            {
                if (_locators == null)
                {
                    Interlocked.CompareExchange(ref _locators, new LocatorBaseCollection(this), null);
                }
                return this._locators;

            }
        }

        /// <summary>
        /// Gets the collection of bulk ingest manifests in the system.
        /// </summary>
        public override IngestManifestCollection IngestManifests
        {
            get
            {
                if (_ingestManifests == null)
                {
                    Interlocked.CompareExchange(ref _ingestManifests, new IngestManifestCollection(this), null);
                }
                return this._ingestManifests;

            }
        }

        /// <summary>
        /// Gets the collection of manifest asset files in the system
        /// </summary>
        public override IngestManifestFileCollection IngestManifestFiles
        {
            get
            {
                if (_ingestManifestFiles == null)
                {
                    Interlocked.CompareExchange(ref _ingestManifestFiles, new IngestManifestFileCollection(this, null), null);
                }
                return this._ingestManifestFiles;

            }
        }

        /// <summary>
        /// Gets the collection of manifest assets in the system
        /// </summary>
        public override IngestManifestAssetCollection IngestManifestAssets
        {
            get
            {
                if (_ingestManifestAssets == null)
                {
                    Interlocked.CompareExchange(ref _ingestManifestAssets, new IngestManifestAssetCollection(this, null), null);
                }
                return this._ingestManifestAssets;
            }
        }

        /// <summary>
        /// Gets the collection of EncodingReservedUnits
        /// </summary>
        public override EncodingReservedUnitCollection EncodingReservedUnits
        {
            get
            {
                if (_encodingReservedUnits == null)
                {
                    Interlocked.CompareExchange(ref _encodingReservedUnits, new EncodingReservedUnitCollection(this), null);
                }
                return this._encodingReservedUnits;
            }
        }

        /// <summary>
        /// Gets the collection of Filters (account level Filter)
        /// </summary>
        public override StreamingFilterBaseCollection Filters
        {
            get
            {
                if (_streamingFilters == null)
                {
                    Interlocked.CompareExchange(ref _streamingFilters, new StreamingFilterBaseCollection(this), null);
                }
                return this._streamingFilters;

            }
        }

        /// <summary>
        /// Gets the collection of MonitoringConfiguration
        /// </summary>
        public override MonitoringConfigurationCollection MonitoringConfigurations
        {
            get
            {
                if (_monitoringConfigurations == null)
                {
                    Interlocked.CompareExchange(ref _monitoringConfigurations,
                        new MonitoringConfigurationCollection(this), null);
                }
                return this._monitoringConfigurations;
            }
        }

        /// <summary>
        /// Gets the collection of ChannelMetrics
        /// </summary>
        public override ChannelMetricsCollection ChannelMetrics
        {
            get
            {
                if (_channelMetrics == null)
                {
                    Interlocked.CompareExchange(ref _channelMetrics, new ChannelMetricsCollection(this), null);
                }
                return this._channelMetrics;
            }
        }

        /// <summary>
        /// Gets the collection of Streaming EndPoint Metrics
        /// </summary>
        public override StreamingEndPointRequestLogCollection StreamingEndPointRequestLogs
        {
            get
            {
                if (_streamingEndPointRequestLogs == null)
                {
                    Interlocked.CompareExchange(ref _streamingEndPointRequestLogs, new StreamingEndPointRequestLogCollection(this), null);
                }
                return this._streamingEndPointRequestLogs;
            }
        }
    }
}