//-----------------------------------------------------------------------
// <copyright file="AzureEnvironments.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Describes the well-known Azure environments.
    /// </summary>
    public static class AzureEnvironments
    {
        /// <summary>
        /// Azure Cloud environment.
        /// </summary>
        public static readonly AzureEnvironment AzureCloudEnvironment = new AzureEnvironment(
            AzureEnvironmentConstants.AzureCloudActiveDirectoryEndpoint,
            AzureEnvironmentConstants.AzureCloudMediaServicesResource,
            AzureEnvironmentConstants.SdkAadApplicationId,
            AzureEnvironmentConstants.SdkAadApplicationRedirectUri);

        /// <summary>
        /// Azure China Cloud environment.
        /// </summary>
        public static readonly AzureEnvironment AzureChinaCloudEnvironment = new AzureEnvironment(
            AzureEnvironmentConstants.AzureChinaCloudActiveDirectoryEndpoint,
            AzureEnvironmentConstants.AzureChinaCloudMediaServicesResource,
            AzureEnvironmentConstants.SdkAadApplicationId,
            AzureEnvironmentConstants.SdkAadApplicationRedirectUri);

        /// <summary>
        /// Azure US Government environment.
        /// </summary>
        public static readonly AzureEnvironment AzureUsGovernmentEnvironment = new AzureEnvironment(
            AzureEnvironmentConstants.AzureUsGovernmentActiveDirectoryEndpoint,
            AzureEnvironmentConstants.AzureUsGovernmentMediaServicesResource,
            AzureEnvironmentConstants.AzureUsGovernmentSdkAadAppliationId,
            AzureEnvironmentConstants.SdkAadApplicationRedirectUri);

        /// <summary>
        /// Azure German Cloud environment.
        /// </summary>
        public static readonly AzureEnvironment AzureGermanCloudEnvironment = new AzureEnvironment(
            AzureEnvironmentConstants.AzureGermanCloudActiveDirectoryEndpoint,
            AzureEnvironmentConstants.AzureGermanCloudMediaServicesResource,
            AzureEnvironmentConstants.SdkAadApplicationId,
            AzureEnvironmentConstants.SdkAadApplicationRedirectUri);
    }
}