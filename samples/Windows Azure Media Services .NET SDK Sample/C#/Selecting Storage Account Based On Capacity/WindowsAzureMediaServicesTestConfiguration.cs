//-----------------------------------------------------------------------
// <copyright file="WindowsAzureMediaServicesTestConfiguration.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Configuration;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client;

namespace SDK.Client.Samples.LoadBalancing
{
    public class WindowsAzureMediaServicesTestConfiguration
    {
      
        public static string MediaServiceAccountName = ConfigurationManager.AppSettings["MediaServiceAccountName"];
        public static string MediaServiceAccountKey = ConfigurationManager.AppSettings["MediaServiceAccountKey"];
        public static string ClientStorageConnectionString = ConfigurationManager.AppSettings["ClientStorageConnectionString"];
        public static string MediaServicesUri = ConfigurationManager.AppSettings["MediaServicesUri"];
        public static string MediaServicesAcsBaseAddress = ConfigurationManager.AppSettings["MediaServicesAcsBaseAddress"];
        public static string MediaServicesAccessScope = ConfigurationManager.AppSettings["MediaServicesAccessScope"];

        public static CloudMediaContext CreateCloudMediaContext()
        {
            // This overload is used for testing purposes
            // It is recommended to use public CloudMediaContext(string accountName, string accountKey) in your code to avoid code changes if default values will be changed later
            return new CloudMediaContext(new Uri(MediaServicesUri), MediaServiceAccountName, MediaServiceAccountKey, MediaServicesAccessScope, MediaServicesAcsBaseAddress);
        }
    }
}