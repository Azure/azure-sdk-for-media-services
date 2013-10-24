//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ChannelTests
    {
        private CloudMediaContext _dataContext;

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Priority(1)]
        [Ignore] // enable when environment is ready
        public void ChannelTestReset()
        {
            IChannel channel = _dataContext.Channels.Create("unittestreset-830", ChannelSize.Large, MakeChannelSettings());
            channel.Reset();
        }

        #region Helper/utility methods

        static ChannelSettings MakeChannelSettings()
        {
            var settings = new ChannelSettings
            {
                Ingest = new IngestEndpointSettings
                {
                    Security = new IngestEndpointSecuritySettings
                    {
                        IPv4AllowList = new List<Ipv4>
                        {
                            new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                        }
                    }
                },
            };

            return settings;
        }

        #endregion
    }
}