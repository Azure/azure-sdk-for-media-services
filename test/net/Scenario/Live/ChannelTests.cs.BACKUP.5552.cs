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

using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
=======
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;
>>>>>>> origin/dev

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ChannelTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        public void ChannelTestReset()
        {
            var channelName = Guid.NewGuid().ToString().Substring(0, 30);

            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = channelName,
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput()
                });
            Assert.AreEqual(ChannelState.Stopped, channel.State);

            channel.Start();
            Assert.AreEqual(ChannelState.Running, channel.State);

            channel.Reset();

            channel.Stop();
            Assert.AreEqual(ChannelState.Stopped, channel.State);

            channel.Delete();
            channel = _mediaContext.Channels.Where(c => c.Name == channelName).SingleOrDefault();
            Assert.IsNull(channel);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        public void ChannelTestCreateTrivial()
        {
            var channelName = Guid.NewGuid().ToString().Substring(0, 30);

            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = channelName,
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput()
                });
            Assert.AreEqual(ChannelState.Stopped, channel.State);

            channel.Delete();
            channel = _mediaContext.Channels.Where(c => c.Name == channelName).SingleOrDefault();
            Assert.IsNull(channel);
        }

        #region Helper/utility methods

        static ChannelInput MakeChannelInput()
        {
            return new ChannelInput
            {
                KeyFrameInterval = TimeSpan.FromSeconds(2),
                StreamingProtocol = StreamingProtocol.FragmentedMP4,
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "testName1",
                            Address = IPAddress.Parse("1.1.1.1"),
                            SubnetPrefixLength = 24
                        }
                    }
                }
            };
        }

        static ChannelPreview MakeChannelPreview()
        {
            return new ChannelPreview
            {
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "testName1",
                            Address = IPAddress.Parse("1.1.1.1"),
                            SubnetPrefixLength = 24
                        }
                    }
                }
            };
        }

        static ChannelOutput MakeChannelOutput()
        {
            return new ChannelOutput
            {
                Hls = new ChannelOutputHls { FragmentsPerSegment = 1 }
            };
        }

        #endregion
    }
}