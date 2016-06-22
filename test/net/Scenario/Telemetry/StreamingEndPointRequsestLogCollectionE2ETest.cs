//-----------------------------------------------------------------------
// <copyright file="StreamingEndPointRequsestLogCollectionE2ETest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    /// For fetching channel metrics test
    /// </summary>
    [TestClass]
    public class StreamingEndPointRequsestLogCollectionE2ETest
    {
        private class StreamingEndPointRequestLogEntity : TableEntity
        {
            public Guid ServiceId { get; set; }
            public DateTime ObservedTime { get; set; }
            public string HostName { get; set; }
            public int StatusCode { get; set; }
            public string ResultCode { get; set; }
            public int RequestCount { get; set; }
            public Int64 BytesSent { get; set; }
            public int ServerLatency { get; set; }
            public int E2ELatency { get; set; }
            public string Name { get; set; }

            public StreamingEndPointRequestLogEntity(
                string partitionKey,
                string rowKey,
                DateTime observedTime,
                string hostName,
                int statusCode,
                string resultCode,
                int requestCount,
                long bytesSent,
                int serverLatency,
                int endToEndLatency)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
                ServiceId = StreamingEndPointId;
                ObservedTime = observedTime;
                HostName = hostName;
                StatusCode = statusCode;
                ResultCode = resultCode;
                RequestCount = requestCount;
                BytesSent = (Int64) bytesSent;
                ServerLatency = serverLatency;
                E2ELatency = endToEndLatency;
                Name = "StreamingEndpointRequestLog";
            }
        }

        private MediaContextBase _mediaConext;
        private static readonly string[] TestTableNames = { "TelemetryMetrics20120302", "TelemetryMetrics20120303" };
        private static readonly Guid AccountId = Guid.Parse("aeeac671d8b44a06b34a7abd15044a06");
        private static readonly Guid StreamingEndPointId = Guid.Parse("ba00402a062d4a61be0708ffb2fc700f");
        private static readonly string PartitionKey = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", 
            AccountId.ToString("n"), StreamingEndPointId.ToString("n"));
        private static readonly StreamingEndPointRequestLogEntity[] TestData =
        {
            new StreamingEndPointRequestLogEntity(PartitionKey, "07581_00000", new DateTime(2012, 3, 2, 21, 53, 39, DateTimeKind.Utc), "hostname1",
                0, "S_OK", 1, 2, 30, 30),
            new StreamingEndPointRequestLogEntity(PartitionKey, "07581_00001", new DateTime(2012, 3, 2, 21, 53, 39, DateTimeKind.Utc), "hostname2",
                0, "S_OK", 1, 2, 30, 30),
            new StreamingEndPointRequestLogEntity(PartitionKey, "07581_00000", new DateTime(2012, 3, 3, 21, 53, 39, DateTimeKind.Utc), "hostname1",
                0, "S_OK", 1, 2, 30, 30),
            new StreamingEndPointRequestLogEntity(PartitionKey, "07581_00001", new DateTime(2012, 3, 3, 21, 53, 39, DateTimeKind.Utc), "hostname2",
                0, "S_OK", 1, 2, 30, 30),
        };

        private readonly TraceListener _consoleTraceListener = new ConsoleTraceListener();

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
        public void TestStreamingEndPointMetrics()
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
            var res = _mediaConext.StreamingEndPointRequestLogs.GetStreamingEndPointMetrics(
                GetTableEndPoint(),
                WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey,
                AccountId.ToString(),
                StreamingEndPointId.ToString(),
                new DateTime(2012, 3, 2, 21, 53, 38, DateTimeKind.Utc),
                new DateTime(2012, 3, 2, 21, 53, 40, DateTimeKind.Utc));
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(resArray.Length, 2);
            VerifyResult(resArray[0], TestData[0]);
            VerifyResult(resArray[1], TestData[1]);
        }

        private void TestQuery2()
        {
            var res = _mediaConext.StreamingEndPointRequestLogs.GetStreamingEndPointMetrics(
                GetTableEndPoint(),
                WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey,
                AccountId.ToString(),
                StreamingEndPointId.ToString(),
                new DateTime(2012, 3, 2, 21, 53, 38, DateTimeKind.Utc),
                new DateTime(2012, 3, 3, 21, 53, 40, DateTimeKind.Utc));
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(resArray.Length, 4);
            VerifyResult(resArray[0], TestData[0]);
            VerifyResult(resArray[1], TestData[1]);
            VerifyResult(resArray[2], TestData[2]);
            VerifyResult(resArray[3], TestData[3]);
        }

        private void VerifyResult(IStreamingEndPointRequestLog value, StreamingEndPointRequestLogEntity expected)
        {
            Assert.AreEqual(value.PartitionKey, expected.PartitionKey);
            Assert.AreEqual(value.RowKey, expected.RowKey);
            Assert.AreEqual(value.ObservedTime, expected.ObservedTime);
            Assert.AreEqual(value.AccountId, AccountId);
            Assert.AreEqual(value.StreamingEndpointId, StreamingEndPointId);
            Assert.AreEqual(value.ObservedTime, expected.ObservedTime);
            Assert.AreEqual(value.HostName, expected.HostName);
            Assert.AreEqual(value.StatusCode, expected.StatusCode);
            Assert.AreEqual(value.ResultCode, expected.ResultCode);
            Assert.AreEqual(value.RequestCount, expected.RequestCount);
            Assert.AreEqual(value.BytesSent, expected.BytesSent);
            Assert.AreEqual(value.ServerLatency, expected.ServerLatency);
            Assert.AreEqual(value.EndToEndLatency, expected.E2ELatency);
        }

        private static string GetTableEndPoint()
        {
            return "https://" + WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountName + ".table.core.windows.net/";
        }
    }
}
