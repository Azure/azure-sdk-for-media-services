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
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;


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
        public void SimpleChannelQueries()
        {
            var i = _mediaContext.Assets.Count();
            var channel = _mediaContext.Channels.Where(c => c.Name == Guid.NewGuid().ToString()).FirstOrDefault();
            var programs = _mediaContext.Programs.Where(c => c.Name == Guid.NewGuid().ToString()).FirstOrDefault();

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

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        public void ChannelTestCreateInRunningStateWithVanityUrlFlag()
        {
            var channelName = Guid.NewGuid().ToString().Substring(0, 30);

            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = channelName,
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput(),
                    State = ChannelState.Running,
                    VanityUrl = true,
                });
            Assert.AreEqual(ChannelState.Running, channel.State);
            Assert.AreEqual(true, channel.VanityUrl);

            channel.Stop();
            channel.Delete();
            channel = _mediaContext.Channels.Where(c => c.Name == channelName).SingleOrDefault();
            Assert.IsNull(channel);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(1)]
        public void GetChannelMetricsTest()
        {
            var channelName = Guid.NewGuid().ToString().Substring(0, 30);

            string accountId = WindowsAzureMediaServicesTestConfiguration.AccountId;

            IChannel channel = _mediaContext.Channels.Create(
                new ChannelCreationOptions
                {
                    Name = channelName,
                    Input = MakeChannelInput(),
                    Preview = MakeChannelPreview(),
                    Output = MakeChannelOutput()
                });

            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountName, WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey),
             true);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var table1 = cloudTableClient.GetTableReference(TestTableNames[0]);
            var table2 = cloudTableClient.GetTableReference(TestTableNames[1]);

            var channelId = new Guid(channel.Id.Split(':').Last());
            var partitionKey = $"{accountId}_{channelId.ToString("N")}";
            var testData = GetTestData(partitionKey, channelId);

            try
            {
                var dataCachce = channel.GetTelemetry();

                table1.CreateIfNotExists();
                var op1 = new TableBatchOperation();
                op1.Insert(testData[0]);
                op1.Insert(testData[1]);
                table1.ExecuteBatch(op1);

                TestTableNotExists(dataCachce, testData, new Guid(accountId), channelId);

                table2.CreateIfNotExists();
                var op2 = new TableBatchOperation();
                op2.Insert(testData[2]);
                op2.Insert(testData[3]);
                table2.ExecuteBatch(op2);

                // case 1: both start and end time are on the same day
                TestQuery1(dataCachce, testData, new Guid(accountId), channelId);
                // case 2: the start and end time are on different day
                TestQuery2(dataCachce, testData, new Guid(accountId), channelId);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                table1?.DeleteIfExists();
                table2?.DeleteIfExists();
                _mediaContext.Channels.Where(c => c.Id == channel.Id).Single().Delete();
            }
        }

        #region Helper/utility methods and classes.

        private static readonly TimeSpan TimeOfDay = new TimeSpan(21, 53, 39);

        private static readonly DateTime[] dates = 
        {
            new DateTime(9999, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(9999, 12, 30, 0, 0, 0, DateTimeKind.Utc)
        };

        private static readonly string[] TestTableNames =
        {
             $"TelemetryMetrics{dates[0].ToString("yyyyMMdd")}",
             $"TelemetryMetrics{dates[1].ToString("yyyyMMdd")}"
        };

        private void TestQuery1(
            ChannelTelemetryDataProvider cache,
            ChannelHeartbeatEntity[] testData,
            Guid accountId,
            Guid channelId)
        {
            var dateTime = dates[0].Add(TimeOfDay);
            var res = cache.GetChannelHeartbeats(dateTime.AddMinutes(-1), dateTime.AddMinutes(1));
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(2, resArray.Length);
            VerifyResult(
                resArray.Take(2).ToArray(),
                testData.Take(2).ToArray(),
                channelId,
                accountId);
        }

        private void TestQuery2(
            ChannelTelemetryDataProvider cache,
            ChannelHeartbeatEntity[] testData,
            Guid accountId,
            Guid channelId)
        {
            var dateTime = dates[1].Add(TimeOfDay);
            var res = cache.GetChannelHeartbeats(dateTime.AddDays(-1).AddMinutes(-1), dateTime.AddMinutes(1));
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(4, resArray.Length);
            VerifyResult(
                resArray.Take(4).ToArray(),
                testData.Take(4).ToArray(),
                channelId,
                accountId);
        }

        private void TestTableNotExists(
            ChannelTelemetryDataProvider cache,
            ChannelHeartbeatEntity[] testData,
            Guid accountId,
            Guid channelId)
        {
            var dateTime = dates[1].Add(TimeOfDay);
            var res = cache.GetChannelHeartbeats(dateTime.AddDays(-1).AddMinutes(-1), dateTime.AddMinutes(1));
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(2, resArray.Length);
            VerifyResult(
                resArray.Take(2).ToArray(),
                testData.Take(2).ToArray(),
                channelId,
                accountId);
        }

        private static ChannelHeartbeatEntity[] GetTestData(string partitionKey, Guid serviceId)
        {
            DateTime day1 = dates[0].Date.Add(TimeOfDay);
            DateTime day2 = day1.AddDays(1);

            return new[]
            {
                new ChannelHeartbeatEntity(partitionKey, "07581_00000", day1, "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, serviceId, 10, false, false, false, true),
                new ChannelHeartbeatEntity(partitionKey, "07581_00001", day1, "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, serviceId, 10, false, false, false, true),
                new ChannelHeartbeatEntity(partitionKey, "07581_00000", day2, "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, serviceId, 10, false, false, false, true),
                new ChannelHeartbeatEntity(partitionKey, "07581_00001", day2, "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, serviceId, 10, false, false, false, true)
            };
        }

        private void VerifyResult(IChannelHeartbeat[] values, ChannelHeartbeatEntity[] expectedValues, Guid channelId, Guid accountId)
        {
            Assert.AreEqual(expectedValues.Length, values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                var expected = expectedValues[i];

                Assert.AreEqual(value.PartitionKey, expected.PartitionKey);
                Assert.AreEqual(value.RowKey, expected.RowKey);
                Assert.AreEqual(value.AccountId, accountId);
                Assert.AreEqual(value.ChannelId, channelId);
                Assert.AreEqual(value.ObservedTime, expected.ObservedTime);
                Assert.AreEqual(value.CustomAttributes, expected.CustomAttributes);
                Assert.AreEqual(value.TrackType, expected.TrackType);
                Assert.AreEqual(value.TrackName, expected.TrackName);
                Assert.AreEqual(value.Bitrate, expected.Bitrate);
                Assert.AreEqual(value.IncomingBitrate, expected.IncomingBitrate);
                Assert.AreEqual(value.OverlapCount, expected.OverlapCount);
                Assert.AreEqual(value.DiscontinuityCount, expected.DiscontinuityCount);
                Assert.AreEqual(value.NonincreasingCount, expected.NonincreasingCount);
                Assert.AreEqual(value.UnalignedKeyFrames, expected.UnalignedKeyFrames);
                Assert.AreEqual(value.UnalignedPresentationTime, expected.UnalignedPresentationTime);
                Assert.AreEqual(value.UnexpectedBitrate, expected.UnexpectedBitrate);
                Assert.AreEqual(value.Healthy, expected.Healthy);
            }
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

        private class ChannelHeartbeatEntity : TableEntity
        {
            public DateTime ObservedTime { get; set; }
            public string CustomAttributes { get; set; }
            public string TrackType { get; set; }
            public string TrackName { get; set; }
            public int Bitrate { get; set; }
            public int IncomingBitrate { get; set; }
            public int OverlapCount { get; set; }
            public int DiscontinuityCount { get; set; }
            public Int64 LastTimestamp { get; set; }
            public string Type { get; set; }
            public Guid ServiceId { get; set; }
            public string Name { get; set; }
            public int NonincreasingCount { get; set; }
            public bool UnalignedKeyFrames { get; set; }
            public bool UnalignedPresentationTime { get; set; }
            public bool UnexpectedBitrate { get; set; }
            public bool Healthy { get; set; }

            public ChannelHeartbeatEntity(
                string partitionKey,
                string rowKey,
                DateTime observedTime,
                string customAttributes,
                string trackType,
                string trackName,
                int bitrate,
                int incomingBitrate,
                int overlapCount,
                int discontinuityCount,
                ulong lastTimestamp,
                Guid serviceId,
                int nonincreasingCount,
                bool unalignedKeyFrames,
                bool unalignedPresentationTime,
                bool unexpectedBitrate,
                bool healthy)
            {
                this.PartitionKey = partitionKey;
                this.RowKey = rowKey;
                ObservedTime = observedTime;
                CustomAttributes = customAttributes;
                TrackType = trackType;
                TrackName = trackName;
                Bitrate = bitrate;
                IncomingBitrate = incomingBitrate;
                OverlapCount = overlapCount;
                DiscontinuityCount = discontinuityCount;
                LastTimestamp = (Int64)lastTimestamp;
                ServiceId = serviceId;
                NonincreasingCount = nonincreasingCount;
                UnalignedKeyFrames = unalignedKeyFrames;
                UnalignedPresentationTime = unalignedPresentationTime;
                UnexpectedBitrate = unexpectedBitrate;
                Healthy = healthy;
                Type = "Channel";
                Name = "ChannelHeartbeat";
            }
        }

        #endregion
    }
}