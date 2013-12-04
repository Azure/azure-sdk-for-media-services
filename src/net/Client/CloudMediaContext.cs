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
using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.WindowsAzure.MediaServices.Client.Versioning;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the context from which all entities in the Microsoft WindowsAzure Media Services platform can be accessed.
    /// </summary>
    public partial class CloudMediaContext : MediaContextBase
    {
        /// <summary>
        /// The certificate thumbprint for Nimbus services.
        /// </summary>
        internal const string NimbusRestApiCertificateThumbprint = "AC24B49ADEF9D6AA17195E041D3F8D07C88EC145";

        /// <summary>
        /// The certificate subject for Nimbus services.
        /// </summary>
        internal const string NimbusRestApiCertificateSubject = "CN=NimbusRestApi";

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

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="accountName">The Microsoft WindowsAzure Media Services account name to authenticate with.</param>
        /// <param name="accountKey">The Microsoft WindowsAzure Media Services account key to authenticate with.</param>
        [Obsolete]
        public CloudMediaContext(string accountName, string accountKey)
            : this(CloudMediaContext._mediaServicesUri, new MediaServicesCredentials(accountName, accountKey))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="apiServer">A <see cref="Uri"/> representing a the API endpoint.</param>
        /// <param name="accountName">The Microsoft WindowsAzure Media Services account name to authenticate with.</param>
        /// <param name="accountKey">The Microsoft WindowsAzure Media Services account key to authenticate with.</param>
        [Obsolete]
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
        [Obsolete]
        public CloudMediaContext(Uri apiServer, string accountName, string accountKey, string scope, string acsBaseAddress)
            : this(apiServer, new MediaServicesCredentials(accountName, accountKey, scope, acsBaseAddress))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="credentials">Microsoft WindowsAzure Media Services credentials.</param>
        public CloudMediaContext(MediaServicesCredentials credentials)
            : this(_mediaServicesUri, credentials)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudMediaContext"/> class.
        /// </summary>
        /// <param name="apiServer">A <see cref="Uri"/> representing the API endpoint.</param>
        /// <param name="credentials">Microsoft WindowsAzure Media Services credentials.</param>
        public CloudMediaContext(Uri apiServer, MediaServicesCredentials credentials)
        {
            this.ParallelTransferThreadCount = 10;
            this.NumberOfConcurrentTransfers = 2;

            this.Credentials = credentials;

            OAuthDataServiceAdapter dataServiceAdapter =
                new OAuthDataServiceAdapter(credentials, NimbusRestApiCertificateThumbprint, NimbusRestApiCertificateSubject);
            ServiceVersionAdapter versionAdapter = new ServiceVersionAdapter(KnownApiVersions.Current);

            this.MediaServicesClassFactory = new AzureMediaServicesClassFactory(apiServer, dataServiceAdapter, versionAdapter, this);

            InitializeCollections();
            InitializeLiveCollections();
            InitializeDynamicEncryptionCollections();
        }

        private void InitializeCollections()
        {
            this._jobs = new JobBaseCollection(this);
            this._jobTemplates = new JobTemplateBaseCollection(this);
            this._assets = new AssetCollection(this);
            this._files = new AssetFileCollection(this);
            this._accessPolicies = new AccessPolicyBaseCollection(this);
            this._contentKeys = new ContentKeyCollection(this);
            this._notificationEndPoints = new NotificationEndPointCollection(this);
            this._mediaProcessors = new MediaProcessorBaseCollection(this);
            this._locators = new LocatorBaseCollection(this);
            this._ingestManifests = new IngestManifestCollection(this);
            this._ingestManifestAssets = new IngestManifestAssetCollection(this, null);
            this._ingestManifestFiles = new IngestManifestFileCollection(this, null);
            this._storageAccounts = new StorageAccountBaseCollection(this);
        }

        /// <summary>
        /// Gets Microsoft WindowsAzure Media Services credentials used for authenticating requests.
        /// </summary>
        public MediaServicesCredentials Credentials { get; private set; }

        /// <summary>
        /// Gets the collection of assets in the system.
        /// </summary>
        public override AssetBaseCollection Assets
        {
            get { return this._assets; }
        }

        /// <summary>
        /// Gets the collection of files in the system.
        /// </summary>
        public override AssetFileBaseCollection Files
        {
            get { return this._files; }
        }

        /// <summary>
        /// Gets the collection of access policies in the system.
        /// </summary>
        public override AccessPolicyBaseCollection AccessPolicies
        {
            get { return this._accessPolicies; }
        }

        /// <summary>
        /// Gets the collection of content keys in the system.
        /// </summary>
        public override ContentKeyBaseCollection ContentKeys
        {
            get { return this._contentKeys; }
        }

        /// <summary>
        /// Gets the collection of jobs available in the system.
        /// </summary>
        public override JobBaseCollection Jobs
        {
            get { return this._jobs; }
        }

        /// <summary>
        /// Gets the collection of job templates available in the system.
        /// </summary>
        public override JobTemplateBaseCollection JobTemplates
        {
            get { return this._jobTemplates; }
        }

        /// <summary>
        /// Gets the collection of media processors available in the system.
        /// </summary>
        public override MediaProcessorBaseCollection MediaProcessors
        {
            get { return this._mediaProcessors; }
        }

        /// <summary>
        /// Gets a collection to operate on StorageAccounts.
        /// </summary>
        /// <seealso cref="StorageAccountBaseCollection" />
        ///   <seealso cref="IStorageAccount" />
        public override StorageAccountBaseCollection StorageAccounts
        {
            get { return this._storageAccounts; }
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
            get { return this._notificationEndPoints; }
        }

        /// <summary>
        /// Gets the collection of locators in the system.
        /// </summary>
        public override LocatorBaseCollection Locators
        {
            get { return this._locators; }
        }

        /// <summary>
        /// Gets the collection of bulk ingest manifests in the system.
        /// </summary>
        public override IngestManifestCollection IngestManifests
        {
            get { return this._ingestManifests; }
        }

        /// <summary>
        /// Gets the collection of manifest asset files in the system
        /// </summary>
        public  override IngestManifestFileCollection IngestManifestFiles
        {
            get { return this._ingestManifestFiles; }
        }

        /// <summary>
        /// Gets the collection of manifest assets in the system
        /// </summary>
        public override IngestManifestAssetCollection IngestManifestAssets
        {
            get { return this._ingestManifestAssets; }
        }
    }
}
