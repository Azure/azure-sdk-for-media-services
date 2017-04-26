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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Live;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class StreamingEndpointTests
    {
        private CloudMediaContext _mediaContext;
        private static readonly string DefaultCdnProfile = "AzureMediaStreamingPlatformCdnProfile-StandardVerizon";

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
            Assert.IsTrue(string.IsNullOrWhiteSpace(createdToValidate.CdnProfile));

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
            Assert.AreEqual(CdnProviderType.StandardVerizon.ToString(), createdToValidate.CdnProvider);
            Assert.AreEqual(DefaultCdnProfile, createdToValidate.CdnProfile);
            Assert.AreEqual(new Version("2.0").ToString(), createdToValidate.StreamingEndpointVersion);
            Assert.IsNotNull(createdToValidate.FreeTrialEndTime);

            createdToValidate.CdnProfile = "newTestcdnProfile";
            createdToValidate.CdnProvider = CdnProviderType.PremiumVerizon.ToString();
            createdToValidate.Update();

            createdToValidate = _mediaContext.StreamingEndpoints.Where(c => c.Id == streamingEndpoint.Id).FirstOrDefault();
            Assert.AreEqual(CdnProviderType.PremiumVerizon.ToString(), createdToValidate.CdnProvider);
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

        [TestMethod]
        public void StreamingEndpointMetricsTest()
        {
            string accountId = WindowsAzureMediaServicesTestConfiguration.AccountId;

            // Create streaming endpoint.
            var name = "StreamingEndpoint" + DateTime.UtcNow.ToString("hhmmss");
            var option = new StreamingEndpointCreationOptions(name, 1);
            var streamingEndpoint = _mediaContext.StreamingEndpoints.Create(option);

            // Get table reference.
            var cloudStorageAccount = new CloudStorageAccount(
                new StorageCredentials(WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountName, WindowsAzureMediaServicesTestConfiguration.TelemetryStorageAccountKey),
                true);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var endDate = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
            var table1 = cloudTableClient.GetTableReference(GetTableName(endDate.AddDays(-1)));
            var table2 = cloudTableClient.GetTableReference(GetTableName(endDate));

            try
            {
                var dataCache = streamingEndpoint.GetTelemetry();

                var streamingEndpointId = new Guid(streamingEndpoint.Id.Split(':').Last());
                var partitionKey = $"{accountId}_{streamingEndpointId.ToString("N")}";
                var testData = GetTestData(partitionKey , streamingEndpointId, endDate.Date);

                table1.CreateIfNotExists();
                var op1 = new TableBatchOperation();
                op1.Insert(testData[0]);
                op1.Insert(testData[1]);
                table1.ExecuteBatch(op1);

                TestTableNotExists(dataCache, testData, endDate.Date.Add(TimeOfDay), streamingEndpointId);

                table2.CreateIfNotExists();
                var op2 = new TableBatchOperation();
                op2.Insert(testData[2]);
                op2.Insert(testData[3]);
                table2.ExecuteBatch(op2);

                // case 1: both start and end time are on the same day
                TestQuery1(dataCache, testData, endDate.Date.Add(TimeOfDay), streamingEndpointId);
                // case 2: the start and end time are on different day
                TestQuery2(dataCache, testData, endDate.Date.Add(TimeOfDay), streamingEndpointId);
            }
            finally
            {
                _mediaContext.StreamingEndpoints.Where(s => s.Id == streamingEndpoint.Id).Single().Delete();

                table1?.DeleteIfExists();

                table2?.DeleteIfExists();
            }
        }

        #region Helper Methods

        private const string MonitoringTableNamePrefix = "TelemetryMetrics";
        private static readonly TimeSpan TimeOfDay = new TimeSpan(21, 52, 39);

        private string GetTableName(DateTime date)
        {
            return $"{MonitoringTableNamePrefix}{date.Date.ToString("yyyyMMdd")}";
        }

        private StreamingEndPointRequestLogEntity[] GetTestData(string partitionKey, Guid serviceId, DateTime currentDayUtc)
        {
            DateTime today = currentDayUtc.Date.Add(TimeOfDay);
            DateTime yesterday = today.AddDays(-1);
            
            return new[]
            {
                new StreamingEndPointRequestLogEntity(partitionKey, "07581_00000", serviceId,
                    yesterday, "hostname1", 0, "S_OK", 1, 2, 30, 30),
                new StreamingEndPointRequestLogEntity(partitionKey, "07581_00001", serviceId,
                    yesterday, "hostname2", 0, "S_OK", 1, 2, 30, 30),
                new StreamingEndPointRequestLogEntity(partitionKey, "07581_00000", serviceId,
                    today, "hostname1", 0, "S_OK", 1, 2, 30, 30),
                new StreamingEndPointRequestLogEntity(partitionKey, "07581_00001", serviceId,
                    today, "hostname2", 0, "S_OK", 1, 2, 30, 30),
            };
        }

        private void TestQuery1(
            StreamingEndpointTelemetryDataProvider cache,
            StreamingEndPointRequestLogEntity[] testData,
            DateTime date,
            Guid streamingEndpointId)
        {
            date = date.AddDays(-1);
            var res = cache.GetStreamingEndpointRequestLogs(date, date);
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(2, resArray.Length);
            VerifyResult(
                resArray.Take(2).ToArray(),
                testData.Take(2).ToArray(),
                streamingEndpointId);
        }

        private void TestQuery2(
            StreamingEndpointTelemetryDataProvider cache,
            StreamingEndPointRequestLogEntity[] testData,
            DateTime date,
            Guid streamingEndpointId)
        {
            var res = cache.GetStreamingEndpointRequestLogs(date.AddDays(-1), date);
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(4, resArray.Length);
            VerifyResult(
                resArray.Take(4).ToArray(),
                testData.Take(4).ToArray(),
                streamingEndpointId);
        }

        private void TestTableNotExists(
            StreamingEndpointTelemetryDataProvider cache,
            StreamingEndPointRequestLogEntity[] testData,
            DateTime date,
            Guid streamingEndpointId)
        {
            var res = cache.GetStreamingEndpointRequestLogs(date.AddDays(-1), date);
            Assert.IsNotNull(res);
            var resArray = res.ToArray();
            Assert.AreEqual(2, resArray.Length);
            VerifyResult(
                resArray.Take(2).ToArray(),
                testData.Take(2).ToArray(),
                streamingEndpointId);
        }

        private void VerifyResult(IStreamingEndpointRequestLog[] values, StreamingEndPointRequestLogEntity[] expectedValues, Guid streamingEndpointId)
        {
            string accountId = WindowsAzureMediaServicesTestConfiguration.AccountId;

            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                var expected = expectedValues[i];

                Assert.AreEqual(value.PartitionKey, expected.PartitionKey);
                Assert.AreEqual(value.RowKey, expected.RowKey);
                Assert.AreEqual(value.ObservedTime, expected.ObservedTime);
                Assert.AreEqual(value.AccountId, accountId);
                Assert.AreEqual(value.StreamingEndpointId, streamingEndpointId);
                Assert.AreEqual(value.ObservedTime, expected.ObservedTime);
                Assert.AreEqual(value.HostName, expected.HostName);
                Assert.AreEqual(value.StatusCode, expected.StatusCode);
                Assert.AreEqual(value.ResultCode, expected.ResultCode);
                Assert.AreEqual(value.RequestCount, expected.RequestCount);
                Assert.AreEqual(value.BytesSent, expected.BytesSent);
                Assert.AreEqual(value.ServerLatency, expected.ServerLatency);
                Assert.AreEqual(value.EndToEndLatency, expected.E2ELatency);
            }
        }

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
                Guid serviceId,
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
                ServiceId = serviceId;
                ObservedTime = observedTime;
                HostName = hostName;
                StatusCode = statusCode;
                ResultCode = resultCode;
                RequestCount = requestCount;
                BytesSent = (Int64)bytesSent;
                ServerLatency = serverLatency;
                E2ELatency = endToEndLatency;
                Name = "StreamingEndpointRequestLog";
            }
        }

        #endregion
    }
}