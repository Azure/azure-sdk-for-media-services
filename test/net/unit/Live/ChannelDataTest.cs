//-----------------------------------------------------------------------
// <copyright file="ChannelDataTest.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{
    [TestClass]
    public class ChannelDataTest
    {
        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void ChannelSettingsTest()
        {
            IChannel target = new ChannelData();

            var input = MakeChannelInput();
            target.Input = input;

            Assert.AreEqual(input.KeyFrameInterval, target.Input.KeyFrameInterval);
            Assert.AreEqual(input.StreamingProtocol, target.Input.StreamingProtocol);
            Assert.AreEqual(input.AccessControl.IPAllowList[0].Name, target.Input.AccessControl.IPAllowList[0].Name);
            Assert.AreEqual(input.AccessControl.IPAllowList[0].SubnetPrefixLength, target.Input.AccessControl.IPAllowList[0].SubnetPrefixLength);

            var preview = MakeChannelPreview();
            target.Preview = preview;

            Assert.AreEqual(preview.AccessControl.IPAllowList[0].Name, target.Preview.AccessControl.IPAllowList[0].Name);
            Assert.AreEqual(preview.AccessControl.IPAllowList[0].SubnetPrefixLength, target.Preview.AccessControl.IPAllowList[0].SubnetPrefixLength);

            var output = MakeChannelOutput();
            target.Output = output;

            Assert.AreEqual(output.Hls.FragmentsPerSegment, target.Output.Hls.FragmentsPerSegment);

            var encoding = MakeChannelEncoding();
            target.Encoding = encoding;

            Assert.AreEqual(encoding.SystemPreset, target.Encoding.SystemPreset);
            Assert.AreEqual(encoding.AudioStreams[0].Index, target.Encoding.AudioStreams[0].Index);
            Assert.AreEqual(encoding.AudioStreams[0].Language, target.Encoding.AudioStreams[0].Language);
            Assert.AreEqual(encoding.VideoStreams[0].Index, target.Encoding.VideoStreams[0].Index);
            Assert.AreEqual(encoding.IgnoreCea708ClosedCaptions, target.Encoding.IgnoreCea708ClosedCaptions);
            Assert.AreEqual(encoding.AdMarkerSource, target.Encoding.AdMarkerSource);

        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void LiveEncodingUrlTest()
        {
            IChannel target = new ChannelData
            {
                Id = Guid.NewGuid().ToString(),
            };

            target.Input = MakeChannelInput();
            target.Preview = MakeChannelPreview();
            target.Output = MakeChannelOutput();
            target.Encoding = MakeChannelEncoding();

            target.ShowSlateAsync(TimeSpan.FromMinutes(5), Guid.NewGuid().ToString());
            target.HideSlateAsync();
            target.StartAdvertisementAsync(TimeSpan.FromMinutes(10), 100, false);
            target.EndAdvertisementAsync();
        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void SettingsTestChannelSubProperties()
        {
            IChannel target = new ChannelData();

            var input = new ChannelInput
            {
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = new List<IPRange> { new IPRange { Address = IPAddress.Parse("192.168.0.0"), SubnetPrefixLength = 24 } }
                }
            };

            target.Input = input;

            Assert.IsNotNull(target.Input.AccessControl.IPAllowList.FirstOrDefault());
        }

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
                            Name = "testName2",
                            Address = IPAddress.Parse("2.2.2.2"),
                            SubnetPrefixLength = 16
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

        static ChannelEncoding MakeChannelEncoding()
        {
            return new ChannelEncoding
            {
                SystemPreset = "Default720p",
                AudioStreams = new List<AudioStream> { new AudioStream { Index = 103, Language = "zhn" } }.AsReadOnly(),
                VideoStreams = new List<VideoStream> { new VideoStream { Index = 104 } }.AsReadOnly(),
                IgnoreCea708ClosedCaptions = true,
                AdMarkerSource = AdMarkerSource.Api
            };
        }
    }
}
