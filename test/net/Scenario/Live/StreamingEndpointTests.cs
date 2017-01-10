//-----------------------------------------------------------------------
// <copyright file="StreamingEndpointTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Net;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Live;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class StreamingEndpointTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void StreamingEndpointCreate()
        {
            string testStreamingEndpointName = Guid.NewGuid().ToString().Substring(0, 30);
            var actual = _mediaContext.StreamingEndpoints.Create(testStreamingEndpointName, 0);
            Assert.AreEqual(testStreamingEndpointName, actual.Name);
            actual.Delete();
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void StreamingEndpointVerifyCdnOptions()
        {
            var name = "StreamingEndpoint" + DateTime.UtcNow.ToString("hhmmss");
            var option = new StreamingEndpointCreationOptions(name, 1)
            {
                CdnEnabled = true,
                CdnProfile = "testCdnProfile",
                CdnProvider = CdnProviderType.PremiumVerizon,
                StreamingEndpointVersion = new Version("2.0")
            };
            var streamingEndpoint = _mediaContext.StreamingEndpoints.Create(option);
            var createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();

            Assert.IsNotNull(createdToValidate);
            Assert.AreEqual(true, createdToValidate.CdnEnabled);
            Assert.AreEqual(option.CdnProfile, createdToValidate.CdnProfile);
            Assert.AreEqual(option.CdnProvider.ToString(), createdToValidate.CdnProvider);
            Assert.AreEqual(new Version("2.0").ToString(), createdToValidate.StreamingEndpointVersion);
            Assert.IsNotNull(createdToValidate.FreeTrialEndTime);

            var updateProfile = "UpdatedProfile";
            streamingEndpoint.CdnEnabled = false;
            streamingEndpoint.CdnProfile = updateProfile;
            streamingEndpoint.Update();

            createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();
            Assert.AreEqual(false, createdToValidate.CdnEnabled);
            Assert.IsTrue(string.Equals(updateProfile, createdToValidate.CdnProfile));

            streamingEndpoint.Delete();
            name = "CDNDisabled" + DateTime.UtcNow.ToString("hhmmss");
            var disabledOption = new StreamingEndpointCreationOptions(name, 1)
            {
                CdnEnabled = false
            };
            streamingEndpoint = _mediaContext.StreamingEndpoints.Create(disabledOption);
            createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();
            Assert.IsNotNull(createdToValidate);
            Assert.AreEqual(false, createdToValidate.CdnEnabled);
            streamingEndpoint.Delete();
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void StreamingEndpointVerifyDefaultCdnOptions()
        {
            var name = "StreamingEndpoint" + DateTime.UtcNow.ToString("hhmmss");
            var option = new StreamingEndpointCreationOptions(name, 1)
            {
                CdnEnabled = true
            };

            var streamingEndpoint = _mediaContext.StreamingEndpoints.Create(option);
            var createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();

            Assert.IsNotNull(createdToValidate);
            Assert.AreEqual(createdToValidate.CdnProvider, CdnProviderType.StandardVerizon.ToString());
            Assert.AreEqual(createdToValidate.CdnProfile, StreamingEndpointCreationOptions.DefaultCdnProfile);
            Assert.AreEqual(new Version("2.0").ToString(), createdToValidate.StreamingEndpointVersion);
            Assert.IsNotNull(createdToValidate.FreeTrialEndTime);

            createdToValidate.CdnProfile = "newTestcdnProfile";
            createdToValidate.CdnProvider = CdnProviderType.PremiumVerizon.ToString();
            createdToValidate.Update();

            createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();
            Assert.AreEqual(createdToValidate.CdnProvider, CdnProviderType.PremiumVerizon.ToString());
            Assert.AreEqual(createdToValidate.CdnProfile, "newTestcdnProfile");

            streamingEndpoint.Delete();
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void StreamingEndpointCreateStartStopDelete()
        {
            var name = "StreamingEndpoint" + DateTime.UtcNow.ToString("hhmmss");
            var option = new StreamingEndpointCreationOptions(name, 1);
            var streamingEndpoint = _mediaContext.StreamingEndpoints.Create(option);
            Assert.IsNotNull(streamingEndpoint);
            streamingEndpoint.Start();
            Assert.AreEqual(StreamingEndpointState.Running,streamingEndpoint.State);
            streamingEndpoint.Stop();
            Assert.AreEqual(StreamingEndpointState.Stopped, streamingEndpoint.State);
            streamingEndpoint.Delete();
            var deleted = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();
            Assert.IsNull(deleted);
        }
    }
}