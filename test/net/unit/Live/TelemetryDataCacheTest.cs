//-----------------------------------------------------------------------
// <copyright file="OperationTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit.Live
{
    [TestClass]
    public class TelemetryDataCacheTest
    {
        private TelemetryDataCache telemetryCache;
        private int uriRequestCount = 0;
        private readonly TimeSpan _expiryTime = TimeSpan.FromSeconds(90);
        private readonly TimeSpan _timeSkewInCache = TimeSpan.FromMinutes(1);

        private readonly DateTime _date1 = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime _date2 = new DateTime(2016, 01, 05, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime _date3 = new DateTime(2016, 01, 10, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime _date4 = new DateTime(2016, 01, 15, 0, 0, 0, DateTimeKind.Utc);

        [TestInitialize]
        public void Initialize()
        {
            telemetryCache = new TelemetryDataCache(GetSasUris);
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void CachingTest()
        {
            telemetryCache.GetRequiredUris(_date1, _date2);
            Assert.AreEqual(1, uriRequestCount);

            // Result should be cached.
            telemetryCache.GetRequiredUris(_date1.AddDays(1), _date2.AddDays(-1));
            Assert.AreEqual(1, uriRequestCount);

            telemetryCache.GetRequiredUris(_date3, _date4);
            Assert.AreEqual(2, uriRequestCount);

            // Data should not be present in cache.
            telemetryCache.GetRequiredUris(_date2.AddDays(-1), _date3.AddDays(1));
            Assert.AreEqual(3, uriRequestCount);

            telemetryCache.GetRequiredUris(_date1, _date4);
            Assert.AreEqual(3, uriRequestCount);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void ExpiryTest()
        {
            telemetryCache.GetRequiredUris(_date1, _date2);
            Assert.AreEqual(1, uriRequestCount);

            // Result should be cached.
            telemetryCache.GetRequiredUris(_date1.AddDays(1), _date2.AddDays(-1));
            Assert.AreEqual(1, uriRequestCount);

            telemetryCache.GetRequiredUris(_date3, _date4);
            Assert.AreEqual(2, uriRequestCount);

            Thread.Sleep(_expiryTime - _timeSkewInCache);

            // Results should have expired.
            telemetryCache.GetRequiredUris(_date1, _date2);
            Assert.AreEqual(3, uriRequestCount);

            telemetryCache.GetRequiredUris(_date1.AddDays(1), _date2.AddDays(-1));
            Assert.AreEqual(3, uriRequestCount);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void CacheCleanUpTest()
        {
            telemetryCache._cleanupInterval = _expiryTime;
            telemetryCache.GetRequiredUris(_date1, _date2);
            Assert.AreEqual((_date2 - _date1).Days + 1, telemetryCache._monitoringSasUriDictionary.Count);

            Thread.Sleep(_expiryTime);

            // Wait for existing records to expire and insert one record.
            Assert.AreEqual((_date2 - _date1).Days + 1, telemetryCache._monitoringSasUriDictionary.Count);
            telemetryCache.GetRequiredUris(_date1, _date1);
            Assert.AreEqual(1, telemetryCache._monitoringSasUriDictionary.Count);
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void OverlappingTimePeriodsRequestTest()
        {
            telemetryCache.GetRequiredUris(_date1, _date2);
            telemetryCache.GetRequiredUris(_date3, _date4);
            telemetryCache.GetRequiredUris(_date2.AddDays(-1), _date3.AddDays(1));
            Assert.AreEqual((_date4 - _date1).Days + 1, telemetryCache._monitoringSasUriDictionary.Count);
        }

        private IEnumerable<MonitoringSasUri> GetSasUris(DateTime start, DateTime end)
        {
            var uris = new List<MonitoringSasUri>();
            var expiry = DateTime.UtcNow.Add(_expiryTime);

            uriRequestCount++;
            for (var i = start.Date; i <= end; i = i.AddDays(1))
            {
                uris.Add(new MonitoringSasUri
                {
                    MetricDataDate = i,
                    SasUriExpiryDate = expiry,
                });
            }
            return uris;
        }
    }
}
