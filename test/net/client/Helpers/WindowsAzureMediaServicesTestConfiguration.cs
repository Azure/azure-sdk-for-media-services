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

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers
{
    public class WindowsAzureMediaServicesTestConfiguration
    {
        public const string MediaDir = @"Media";
        public const string ConfigurationDir = @"Configuration";

        public const string SmallWmv = @"Media\SmallWmv.wmv";
        public const string SmallWmv2 = @"Media\SmallWmv2.wmv";
        public const string BadSmallWmv = @"Media\BadSmallWmv.wmv";
        public const string ThumbnailXml = @"Media\Thumbnail.xml";
        public const string ThumbnailWithZeroStepXml = @"Media\ThumbnailWithZeroStep.xml";
        public const string EncodePlusEncryptWithEeXml = @"Media\EncodePlusEncryptWithEE.xml";

        public const string SmallMp41 = @"Media\SmallMP41.mp4";

        public const string SmallIsm = @"Media\Small.ism";
        public const string SmallIsmc = @"Media\Small.ismc";
        public const string SmallIsmv = @"Media\Small.ismv";

        public const string MultiConfig = @"Configuration\multi.xml";
        public const string DefaultMp4ToSmoothConfig = @"Configuration\MP4 to Smooth Streams.xml";
        public const string PlayReadyConfig = @"Configuration\PlayReady Protection.xml";
        public const string SmoothToHlsConfig = @"Configuration\Smooth Streams to Apple HTTP Live Streams.xml";
        public const string SmoothToEncryptHlsConfig = @"Configuration\Smooth Streams to Encrypted Apple HTTP Live Streams.xml";
        
        public static string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        public static string ClientId = ConfigurationManager.AppSettings["ClientId"];
        public static string ClientStorageConnectionString = ConfigurationManager.AppSettings["ClientStorageConnectionString"];

        public static string MpEncoderName = ConfigurationManager.AppSettings["MPEncoderName"];
        public static string MpEncoderVersion = ConfigurationManager.AppSettings["MPEncoderVersion"];
        public static string MpEncryptorName = ConfigurationManager.AppSettings["MPEncryptorName"];
        public static string MpEncryptorVersion = ConfigurationManager.AppSettings["MPEncryptorVersion"];
        public static string MpPackagerName = ConfigurationManager.AppSettings["MPPackagerName"];
        public static string MpPackagerVersion = ConfigurationManager.AppSettings["MPPackagerVersion"];
        public static string MpStorageDecryptorName = ConfigurationManager.AppSettings["MPStorageDecryptorName"];
        public static string MpStorageDecryptorVersion = ConfigurationManager.AppSettings["MPStorageDecryptorVersion"];

        public static CloudMediaContext CreateCloudMediaContext()
        {
            return new CloudMediaContext( ClientId, ClientSecret);
        }

        public static string GetVideoSampleFilePath(TestContext testContext, string filepath)
        {
            return Path.Combine(testContext.TestDeploymentDir, filepath);
        }
    }
}