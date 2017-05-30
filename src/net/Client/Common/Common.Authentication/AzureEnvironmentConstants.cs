//-----------------------------------------------------------------------
// <copyright file="AzureEnvironmentConstants.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Describes the constants used by <see cref="AzureEnvironment"/>.
    /// </summary>
    internal static class AzureEnvironmentConstants
    {
        /// <summary>
        /// The Active Directory endpoint for Azure Cloud environment.
        /// </summary>
        public static readonly Uri AzureCloudActiveDirectoryEndpoint = new Uri("https://login.microsoftonline.com/");

        /// <summary>
        /// The Media Services resource for Azure Cloud environment.
        /// </summary>
        public const string AzureCloudMediaServicesResource = "https://rest.media.azure.net";

        /// <summary>
        /// The Active Directory endpoint for Azure China Cloud environment.
        /// </summary>
        public static readonly Uri AzureChinaCloudActiveDirectoryEndpoint = new Uri("https://login.chinacloudapi.cn/");

        /// <summary>
        /// The Media Services resource for Azure China Cloud environment.
        /// </summary>
        public const string AzureChinaCloudMediaServicesResource = "https://rest.media.chinacloudapi.cn";

        /// <summary>
        /// The Active Directory endpoint for Azure US Government environment.
        /// </summary>
        public static readonly Uri AzureUsGovernmentActiveDirectoryEndpoint = new Uri("https://login-us.microsoftonline.com/");

        /// <summary>
        /// The Media Services resource for Azure US Government environment.
        /// </summary>
        public const string AzureUsGovernmentMediaServicesResource = "https://rest.media.usgovcloudapi.net";

        /// <summary>
        /// The native SDK AAD application ID for Azure US Government environment.
        /// </summary>
        public const string AzureUsGovernmentSdkAadAppliationId = "68dac91e-cab5-461b-ab4a-ec7dcff0bd67";

        /// <summary>
        /// The Active Directory endpoint for Azure German cloud environment.
        /// </summary>
        public static readonly Uri AzureGermanCloudActiveDirectoryEndpoint = new Uri("https://login.microsoftonline.de/");

        /// <summary>
        /// The Media Services resource for Azure German Cloud environment.
        /// </summary>
        public const string AzureGermanCloudMediaServicesResource = "https://rest.media.cloudapi.de";

        /// <summary>
        /// The native SDK AAD application ID for Azure Cloud, Azure China Cloud and Azure German Cloud environment.
        /// </summary>
        public const string SdkAadApplicationId = "d476653d-842c-4f52-862d-397463ada5e7";

        /// <summary>
        /// The native SDK AAD application's redirect URL for all environments.
        /// </summary>
        public static readonly Uri SdkAadApplicationRedirectUri = new Uri("https://AzureMediaServicesNativeSDK");
    }
}