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

using System.Threading;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the context from which all entities in the Microsoft WindowsAzure Media Services platform can be accessed.
    /// </summary>
    public partial class CloudMediaContext : MediaContextBase
    {
        private ContentKeyAuthorizationPolicyOptionCollection _contentKeyAuthorizationPolicyOptions;
        private ContentKeyAuthorizationPolicyCollection _contentKeyAuthorizationPolicies;
        private AssetDeliveryPolicyCollection _assetDeliveryPolicies;

        /// <summary>
        /// Gets the collection of content key authorization policy options.
        /// </summary>
        public override ContentKeyAuthorizationPolicyOptionCollection ContentKeyAuthorizationPolicyOptions
        {

            get
            {
                if (_contentKeyAuthorizationPolicyOptions == null)
                {
                    Interlocked.CompareExchange(ref _contentKeyAuthorizationPolicyOptions, new ContentKeyAuthorizationPolicyOptionCollection(this), null);
                }
                return _contentKeyAuthorizationPolicyOptions;

            }
        }

        /// <summary>
        /// Gets the content key authorization policies.
        /// </summary>
        /// <value>
        /// The content key authorization policies.
        /// </value>
        public override ContentKeyAuthorizationPolicyCollection ContentKeyAuthorizationPolicies
        {

             get
            {
                if (_contentKeyAuthorizationPolicies == null)
                {
                    Interlocked.CompareExchange(ref _contentKeyAuthorizationPolicies, new ContentKeyAuthorizationPolicyCollection(this), null);
                }
                return _contentKeyAuthorizationPolicies;

            }
        }

        /// <summary>
        /// Gets the asset delivery policies.
        /// </summary>
        /// <value>
        /// The asset delivery policies.
        /// </value>
        public override AssetDeliveryPolicyCollection AssetDeliveryPolicies
        {
            get
            {
                if (_assetDeliveryPolicies == null)
                {
                    Interlocked.CompareExchange(ref _assetDeliveryPolicies, new AssetDeliveryPolicyCollection(this), null);
                }
                return _assetDeliveryPolicies;

            }
        }
    }
}
