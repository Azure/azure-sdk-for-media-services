//-----------------------------------------------------------------------
// <copyright file="FairPlayTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class FilterTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void FirstQuality_ShouldVerifyParameter()
        {
            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(()=>{ new FirstQuality(0);}));
            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(() => { new FirstQuality(-100); }));
            Assert.IsFalse(ExecutionWithException<ArgumentOutOfRangeException>(() => { new FirstQuality(12); }));

            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                _mediaContext.Filters.Create("filter1", null, null, new FirstQuality(0));
            }));
            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                _mediaContext.Filters.Create("filter2", null, null, new FirstQuality(-1));
            }));
            Assert.IsFalse(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                _mediaContext.Filters.Create("filter3", null, null, new FirstQuality(1));
            }));

            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                asset.AssetFilters.Create("filter1", null, null, new FirstQuality(0));
            }));
            Assert.IsTrue(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                asset.AssetFilters.CreateAsync("filter1", null, null, new FirstQuality(0));
            }));

            Assert.IsFalse(ExecutionWithException<ArgumentOutOfRangeException>(() =>
            {
                asset.AssetFilters.CreateAsync("filter1", null, null, new FirstQuality(35));
            }));
        }

        [TestMethod]
        public void Filter_FirstQuality_CRUD()
        {
            const string testFilterName = "filter1";
            const int bitrate1 = 128000;
            const int bitrate2 = 360000;
            var filter = _mediaContext.Filters.Create(testFilterName, null, null, new FirstQuality(bitrate1));
            var resultFilter = _mediaContext.Filters.SingleOrDefault(f => f.Name == testFilterName);
            Assert.IsNotNull(resultFilter);
            Assert.AreEqual(resultFilter.FirstQuality.Bitrate, bitrate1);

            filter.FirstQuality = new FirstQuality(bitrate2);
            filter.Update();

            resultFilter = _mediaContext.Filters.SingleOrDefault(f => f.Name == testFilterName);
            Assert.IsNotNull(resultFilter);
            Assert.AreEqual(resultFilter.FirstQuality.Bitrate, bitrate2);

            filter.Delete();
            resultFilter = _mediaContext.Filters.SingleOrDefault(f => f.Name == testFilterName);
            Assert.IsNull(resultFilter);
        }

        private bool ExecutionWithException<T>(Action func) where T: Exception
        {
            bool expectedExceptionThrown = false;
            try
            {
                func();
            }
            catch (T)
            {
                expectedExceptionThrown = true;
            }

            return expectedExceptionThrown;
        }
    }
}