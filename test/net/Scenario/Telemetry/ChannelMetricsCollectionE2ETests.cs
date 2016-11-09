//-----------------------------------------------------------------------
// <copyright file="ChannelMetricsCollectionE2ETests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    /// For fetching channel metrics test
    /// </summary>
    [TestClass]
    public class ChannelMetricsCollectionE2ETests
    {
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
                ServiceId = ChannelId;
                NonincreasingCount = nonincreasingCount;
                UnalignedKeyFrames = unalignedKeyFrames;
                UnalignedPresentationTime = unalignedPresentationTime;
                UnexpectedBitrate = unexpectedBitrate;
                Healthy = healthy;
                Type = "Channel";
                Name = "ChannelHeartbeat";
            }
        }

        private MediaContextBase _mediaConext;
        private static readonly string[] TestTableNames = { "TelemetryMetrics20110302", "TelemetryMetrics20110303" };
        private static readonly Guid AccountId = Guid.Parse("aeeac671d8b44a06b34a7abd15044a06");
        private static readonly Guid ChannelId = Guid.Parse("ba00402a062d4a61be0708ffb2fc700f");
        private static readonly string PartitionKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", AccountId.ToString("n"), ChannelId.ToString("n"));
        private static readonly ChannelHeartbeatEntity[] TestData =
        {
            new ChannelHeartbeatEntity(PartitionKey, "07581_00000", new DateTime(2011, 3, 2, 21, 53, 39, DateTimeKind.Utc), "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, 10, false, false, false, true),
            new ChannelHeartbeatEntity(PartitionKey, "07581_00001", new DateTime(2011, 3, 2, 21, 53, 39, DateTimeKind.Utc), "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, 10, false, false, false, true),
            new ChannelHeartbeatEntity(PartitionKey, "07581_00000", new DateTime(2011, 3, 3, 21, 53, 39, DateTimeKind.Utc), "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, 10, false, false, false, true),
            new ChannelHeartbeatEntity(PartitionKey, "07581_00001", new DateTime(2011, 3, 3, 21, 53, 39, DateTimeKind.Utc), "",
                "video", "video", 2000000, 123456, 0, 0, 131126004929427, 10, false, false, false, true)
        };

        private readonly TraceListener _consoleTraceListener = new ConsoleTraceListener();

        private const string ChannelIdentifierPrefix = "nb:chid:UUID:";

        [TestInitialize]
        public void SetupTest()
        {
            Trace.Listeners.Add(_consoleTraceListener);
            _mediaConext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Trace.Flush();
            Trace.Listeners.Remove(_consoleTraceListener);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void TestChannelMetrics()
        {
            // prepare the test data
            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountName, WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey),
                true);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var table1 = cloudTableClient.GetTableReference(TestTableNames[0]);
            var table2 = cloudTableClient.GetTableReference(TestTableNames[1]);

            try
            {
                table1.Create();
                var op1 = new TableBatchOperation();
                op1.Insert(TestData[0]);
                op1.Insert(TestData[1]);
                table1.ExecuteBatch(op1);


                table2.Create();
                var op2 = new TableBatchOperation();
                op2.Insert(TestData[2]);
                op2.Insert(TestData[3]);
                table2.ExecuteBatch(op2);
                // case 1: both start and end time are on the same day
                TestQuery1();
                // case 2: the start and end time are on different day
                TestQuery2();
            }
            finally
            {
                if (table1 != null)
                {
                    table1.DeleteIfExists();
                }

                if (table2 != null)
                {
                    table2.DeleteIfExists();
                }
            }
        }

        private void TestQuery1()
        {
            GetChannelIds().ForEach(channelId =>
            {
                var res = _mediaConext.ChannelMetrics.GetChannelMetrics(
                    GetTableEndPoint(),
                    WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey,
                    AccountId.ToString(),
                    channelId,
                    new DateTime(2011, 3, 2, 21, 53, 38, DateTimeKind.Utc),
                    new DateTime(2011, 3, 2, 21, 53, 40, DateTimeKind.Utc));
                Assert.IsNotNull(res);
                var resArray = res.ToArray();
                Assert.AreEqual(resArray.Length, 2);
                VerifyResult(resArray[0], TestData[0]);
                VerifyResult(resArray[1], TestData[1]);
            });
        }

        private void TestQuery2()
        {
            GetChannelIds().ForEach(channelId =>
            {
                var res = _mediaConext.ChannelMetrics.GetChannelMetrics(
                    GetTableEndPoint(),
                    WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey,
                    AccountId.ToString(),
                    channelId,
                    new DateTime(2011, 3, 2, 21, 53, 38, DateTimeKind.Utc),
                    new DateTime(2011, 3, 3, 21, 53, 40, DateTimeKind.Utc));
                Assert.IsNotNull(res);
                var resArray = res.ToArray();
                Assert.AreEqual(resArray.Length, 4);
                VerifyResult(resArray[0], TestData[0]);
                VerifyResult(resArray[1], TestData[1]);
                VerifyResult(resArray[2], TestData[2]);
                VerifyResult(resArray[3], TestData[3]);
            });
        }

        private void VerifyResult(IChannelHeartbeat value, ChannelHeartbeatEntity expected)
        {
            Assert.AreEqual(value.PartitionKey, expected.PartitionKey);
            Assert.AreEqual(value.RowKey, expected.RowKey);
            Assert.AreEqual(value.AccountId, AccountId);
            Assert.AreEqual(value.ChannelId, ChannelId);
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

        private static string GetTableEndPoint()
        {
            return "https://" + WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountName + ".table.core.windows.net/";
        }

        private static List<string> GetChannelIds()
        {
            return new List<string>
            {
                ChannelId.ToString(), 
                ChannelIdentifierPrefix + ChannelId,
            };
        }
    }
}
