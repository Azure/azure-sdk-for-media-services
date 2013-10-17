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

using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a base media context containing collections to operate on.
    /// </summary>
    public abstract class MediaContextBase
    {
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
        /// Returns default storage account
        /// </summary>
        public abstract IStorageAccount DefaultStorageAccount { get; }

        /// <summary>
        /// Gets the collection of notification endpoints avaiable in the system.
        /// </summary>
        public abstract NotificationEndPointCollection NotificationEndPoints { get; }

        /// <summary>
        /// Gets the content key authorization policy options.
        /// </summary>
        /// <value>
        /// The content key authorization policy options.
        /// </value>
        public abstract ContentKeyAuthorizationPolicyOptionCollection ContentKeyAuthorizationPolicyOptions { get; }

        /// <summary>
        /// Gets the content key authorization policies.
        /// </summary>
        /// <value>
        /// The content key authorization policies.
        /// </value>
        public abstract ContentKeyAuthorizationPolicyCollection ContentKeyAuthorizationPolicies { get; }

        /// <summary>
        /// Gets the asset delivery policies.
        /// </summary>
        /// <value>
        /// The asset delivery policies.
        /// </value>
        public abstract AssetDeliveryPolicyCollection AssetDeliveryPolicies { get; }
    }
}
