//-----------------------------------------------------------------------
// <copyright file="LiveEncodingTest.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.Tests
{
    [TestClass]
    public class LiveEncodingTest
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Ignore]    // RTPMPEG2TS deprecated.
        [Priority(1)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void LiveEncodingChannelShowSlateTest()
        {
            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = Guid.NewGuid().ToString().Substring(0, 30),
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput(),
                    EncodingType = ChannelEncodingType.Standard,
                    Encoding = MakeChannelEncoding(ChannelEncodingType.Standard)
                });

            try
            {
                channel.Start();
                try
                {
                    channel.ShowSlate(TimeSpan.FromMinutes(5), null);
                    channel.HideSlate();

                    channel.StartAdvertisement(TimeSpan.FromMinutes(10), 1000);
                    channel.EndAdvertisement(1000);

                    channel.StartAdvertisement(TimeSpan.FromMinutes(10), 1001, false);
                    channel.EndAdvertisement(1001);
                }
                finally 
                {
                    channel.Stop();
                }
            }
            finally 
            {
                channel.Delete();
            }
        }


        [TestMethod]
        [Ignore]    // RTPMPEG2TS deprecated.
        [Priority(1)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestStreamSelectionPersistence()
        {
            var channelOptions = new ChannelCreationOptions
            {
                Name = Guid.NewGuid().ToString().Substring(0, 30),
                Input = MakeChannelInput(),
                Preview = MakeChannelPreview(),
                Output = MakeChannelOutput(),
                EncodingType = ChannelEncodingType.Standard,
                Encoding = new ChannelEncoding
                {
                    SystemPreset = "Default720p",
                    VideoStreams = new List<VideoStream>
                    {
                        new VideoStream { Index = 0 }
                    }.AsReadOnly(),
                    AudioStreams = new List<AudioStream>
                    {
                        new AudioStream { Index = 1, Language = "eng" },
                        new AudioStream { Index = 2, Language = "fra" },
                        new AudioStream { Index = 3, Language = "spa" }
                    }.AsReadOnly()
                }
            };

            IChannel channel = _mediaContext.Channels.Create(channelOptions);
            Assert.AreEqual(channelOptions.Encoding.AudioStreams.Count, channel.Encoding.AudioStreams.Count);
            for (int i = 0; i < channelOptions.Encoding.AudioStreams.Count; ++i)
            {
                Assert.AreEqual(channelOptions.Encoding.AudioStreams[i].Index, channel.Encoding.AudioStreams[i].Index);
                Assert.AreEqual(channelOptions.Encoding.AudioStreams[i].Language, channel.Encoding.AudioStreams[i].Language);
            }
            Assert.AreEqual(channelOptions.Encoding.VideoStreams.Count, channel.Encoding.VideoStreams.Count);
            for (int i = 0; i < channelOptions.Encoding.VideoStreams.Count; ++i)
            {
                Assert.AreEqual(channelOptions.Encoding.VideoStreams[i].Index, channel.Encoding.VideoStreams[i].Index);
            }

            channel.Delete();
        }

        [TestMethod]
        [Ignore]    // RTPMPEG2TS deprecated.
        [Priority(1)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void LivePremiumEncodingChannelTest()
        {
            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = Guid.NewGuid().ToString().Substring(0, 30),
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput(),
                    EncodingType = ChannelEncodingType.Premium,
                    Encoding = MakeChannelEncoding(ChannelEncodingType.Premium)
                });

            try
            {
                channel.Start();

                Thread.Sleep(1000 * 10);

                channel.Stop();
            }
            finally
            {
                channel.Delete();
            }
        }


        static ChannelInput MakeChannelInput()
        {
            return new ChannelInput
            {
                KeyFrameInterval = TimeSpan.FromSeconds(2),
                StreamingProtocol = StreamingProtocol.RTPMPEG2TS,
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

        static ChannelEncoding MakeChannelEncoding(ChannelEncodingType encodingType)
        {
            return new ChannelEncoding
            {
                SystemPreset = encodingType == ChannelEncodingType.Standard ? "Default720p" : "Default1080p",
                IgnoreCea708ClosedCaptions = false,
                AdMarkerSource = AdMarkerSource.Api,
                AudioStreams = new List<AudioStream> {new AudioStream {Index = 103, Language = "eng"}}.AsReadOnly()
            };
        }
    }
}
