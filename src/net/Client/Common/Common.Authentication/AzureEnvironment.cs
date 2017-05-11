//-----------------------------------------------------------------------
// <copyright file="AzureEnvironment.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Describes the Azure environment.
    /// </summary>
    public class AzureEnvironment
    {
        /// <summary>
        /// Gets the Active Directory endpoint.
        /// </summary>
        public Uri ActiveDirectoryEndpoint { get; }

        /// <summary>
        /// Gets the Media Services resource.
        /// </summary>
        public string MediaServicesResource { get; }

        /// <summary>
        /// Gets the Media Services SDK client ID.
        /// </summary>
        public string MediaServicesSdkClientId { get; }

        /// <summary>
        /// Gets Media Services SDK application redirect URI.
        /// </summary>
        public Uri MediaServicesSdkRedirectUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureEnvironment"/> class.
        /// </summary>
        /// <param name="activeDirectoryEndpoint">The Active Directory endpoint.</param>
        /// <param name="mediaServicesResource">The Media Services resource.</param>
        /// <param name="mediaServicesSdkClientId">The Media Services SDK client ID.</param>
        /// <param name="mediaServicesSdkRedirectUri">The Media Services SDK redirect URI.</param>
        public AzureEnvironment(
            Uri activeDirectoryEndpoint,
            string mediaServicesResource,
            string mediaServicesSdkClientId,
            Uri mediaServicesSdkRedirectUri)
        {
            if (activeDirectoryEndpoint == null)
            {
                throw new ArgumentNullException("activeDirectoryEndpoint");
            }

            if (string.IsNullOrWhiteSpace(mediaServicesResource))
            {
                throw new ArgumentException("mediaServicesResource");
            }

            if (string.IsNullOrWhiteSpace(mediaServicesSdkClientId))
            {
                throw new ArgumentException("mediaServicesSdkClientId");
            }

            if (mediaServicesSdkRedirectUri == null)
            {
                throw new ArgumentNullException("mediaServicesSdkRedirectUri");
            }

            ActiveDirectoryEndpoint = activeDirectoryEndpoint;
            MediaServicesResource = mediaServicesResource;
            MediaServicesSdkClientId = mediaServicesSdkClientId;
            MediaServicesSdkRedirectUri = mediaServicesSdkRedirectUri;
        }
    }
}