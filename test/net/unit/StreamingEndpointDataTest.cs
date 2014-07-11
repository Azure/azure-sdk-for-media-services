//-----------------------------------------------------------------------
// <copyright file="AssetFilesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{    
    /// <summary>
    ///This is a test class for StreamingEndpointDataTest and is intended
    ///to contain all StreamingEndpointDataTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] //TODO: enable when the streaming endpoint is deployed in the test environment
    public class StreamingEndpointDataTest
    {
        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod()]
        public void SettingsTestSubProperties()
        {
            IStreamingEndpoint target = new StreamingEndpointData();

            var settings = new StreamingEndpointCacheControl
            {
                MaxAge = TimeSpan.FromMinutes(1)
            };

            target.CacheControl = settings;

            Assert.AreEqual(60, target.CacheControl.MaxAge.Value.TotalSeconds);
        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void SettingsTestChannelSubProperties()
        {
            IChannel target = new ChannelData();

            var input = new ChannelInput()
            {
                 AccessControl = new ChannelServiceAccessControl
                 {
                     IPAllowList = new List<ServiceIPAddress> {new ServiceIPAddress {Address = "192.168.0.1/24", SubnetPrefixLength = 24} }
                 }
            };

            target.Input = input;

            Assert.IsNotNull(target.Input.AccessControl.IPAllowList.FirstOrDefault());
        }
    }
}
