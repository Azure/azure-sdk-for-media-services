//-----------------------------------------------------------------------
// <copyright file="AssetFilterTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class AssetFilterTests
    {
        private CloudMediaContext _mediaContext;
        private string _smallWmv;
       
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
            _smallWmv = WindowsAzureMediaServicesTestConfiguration.GetVideoSampleFilePath(TestContext, WindowsAzureMediaServicesTestConfiguration.SmallWmv);
           
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void CreateUpdateDeleteFilterWithDefaultPresentationTimeRangeAndEmptyFilterTrackSelectStatement()
        {
            string filterName = "CreateUpdateDeleteFilter_" + Guid.NewGuid().ToString();
            IStreamingFilter filter = _mediaContext.Filters.Create(filterName, new PresentationTimeRange(), new List<FilterTrackSelectStatement>());
            Assert.IsNotNull(filter);
            Assert.AreEqual(1, _mediaContext.Filters.Where(c => c.Name == filter.Name).Count());
            Assert.AreNotEqual(0, _mediaContext.Filters.Count());
            filter.PresentationTimeRange = new PresentationTimeRange(timescale:500);
            filter.Update();
            Assert.IsNotNull(_mediaContext.Filters.Where(c => c.Name == filter.Name).FirstOrDefault());
            filter.Delete();
            Assert.IsNull(_mediaContext.Filters.Where(c=>c.Name == filter.Name).FirstOrDefault());

            
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void CreateUpdateDeleteFilterWithAllSelectStatements()
        {
            string filterName = "CreateUpdateDeleteFilter_" + Guid.NewGuid().ToString();
            List<FilterTrackSelectStatement> filterTrackSelectStatements = new List<FilterTrackSelectStatement>();
            FilterTrackSelectStatement filterTrackSelectStatement = new FilterTrackSelectStatement();
            filterTrackSelectStatement.PropertyConditions = new List<IFilterTrackPropertyCondition>();
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackNameCondition("Track Name",FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackFourCCCondition("AACL", FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackBitrateRangeCondition(new FilterTrackBitrateRange(0,1), FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackLanguageCondition("ru", FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackTypeCondition(FilterTrackType.Text, FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatements.Add(filterTrackSelectStatement);
            IStreamingFilter filter = _mediaContext.Filters.Create(filterName, new PresentationTimeRange(), filterTrackSelectStatements);
            Assert.IsNotNull(filter);
            Assert.AreEqual(1, _mediaContext.Filters.Where(c => c.Name == filter.Name).Count());
            Assert.AreNotEqual(0, _mediaContext.Filters.Count());
            Assert.AreEqual(5, _mediaContext.Filters.Where(c => c.Name == filter.Name).First().Tracks.First().PropertyConditions.Count);
            filter.PresentationTimeRange = new PresentationTimeRange(timescale: 500);
            filter.Update();
            Assert.AreEqual(5, _mediaContext.Filters.Where(c => c.Name == filter.Name).First().Tracks.First().PropertyConditions.Count);
            Assert.IsNotNull(_mediaContext.Filters.Where(c => c.Name == filter.Name).FirstOrDefault());
            filter.Delete();
            Assert.IsNull(_mediaContext.Filters.Where(c => c.Name == filter.Name).FirstOrDefault());


        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void InvalidFilterTrackLanguageConditionShouldThrow()
        {
            string filterName = "InvalidFilterTrackLanguageConditionShouldThrow_" + Guid.NewGuid().ToString();
            List<FilterTrackSelectStatement> filterTrackSelectStatements = new List<FilterTrackSelectStatement>();
            FilterTrackSelectStatement filterTrackSelectStatement = new FilterTrackSelectStatement();
            filterTrackSelectStatement.PropertyConditions = new List<IFilterTrackPropertyCondition>();
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackLanguageCondition("expecting language validation here", FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatements.Add(filterTrackSelectStatement);
            IStreamingFilter filter = _mediaContext.Filters.Create(filterName, new PresentationTimeRange(), filterTrackSelectStatements);

        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [ExpectedException(typeof(DataServiceRequestException))]
        public void InvalidFourCCConditionShouldThrow()
        {
            string filterName = "InvalidFourCCConditionShouldThrow_" + Guid.NewGuid().ToString();
            List<FilterTrackSelectStatement> filterTrackSelectStatements = new List<FilterTrackSelectStatement>();
            FilterTrackSelectStatement filterTrackSelectStatement = new FilterTrackSelectStatement();
            filterTrackSelectStatement.PropertyConditions = new List<IFilterTrackPropertyCondition>();
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackFourCCCondition("FourCCCondition validation here", FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatements.Add(filterTrackSelectStatement);
            IStreamingFilter filter = _mediaContext.Filters.Create(filterName, new PresentationTimeRange(), filterTrackSelectStatements);

        }

        
        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void CRUDAssetFilter()
        {
            string assetName = "CRUDAssetFilter_" + Guid.NewGuid().ToString();
            var asset = _mediaContext.Assets.Create(assetName,AssetCreationOptions.None);
            Assert.IsNotNull(asset);

            string filterName = "CRUDAssetFilter_" + Guid.NewGuid().ToString();
            IStreamingAssetFilter filter = asset.AssetFilters.Create(filterName, new PresentationTimeRange(), new List<FilterTrackSelectStatement>());
            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.ParentAssetId);
            Assert.AreEqual(asset.Id,filter.ParentAssetId);

            asset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(asset);
            Assert.AreEqual(1, asset.AssetFilters.Count());

            //Why we are througing internal server exception here
            filter.PresentationTimeRange = new PresentationTimeRange(timescale: 500);
            filter.Update();

            asset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(asset);
            var filterUpdated = asset.AssetFilters.FirstOrDefault();
            Assert.IsNotNull(filterUpdated);

            Assert.AreEqual(filter.PresentationTimeRange.Timescale, filterUpdated.PresentationTimeRange.Timescale);
            Assert.AreEqual((ulong)500, filterUpdated.PresentationTimeRange.Timescale);


            //We don't have acess to asset filters here 
            var globalFilter = _mediaContext.Filters.Where(c => c.Name == filterName).FirstOrDefault();
            Assert.IsNull(globalFilter);

            //Why we are failing here
            filter.Delete();

            asset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(asset);
            Assert.AreEqual(0,asset.AssetFilters.Count());

            globalFilter = _mediaContext.Filters.Where(c => c.Name == filterName).FirstOrDefault();
            Assert.IsNull(globalFilter);

        }
        
    }
}