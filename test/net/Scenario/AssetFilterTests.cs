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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.Storage;

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
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackFourCCCondition("mp4a", FilterTrackCompareOperator.NotEqual));
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

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [DeploymentItem(@"Configuration\MP4 to Smooth Streams.xml", "Configuration")]
        [DeploymentItem(@"Media\SmallMP41.mp4", "Media")]
        public void ApplyDynamicManifestFilter()
        {
            const string typeAudio = "Type=\"audio\"";
            const string typeVideo = "Type=\"video\"";

            string configuration = File.ReadAllText(WindowsAzureMediaServicesTestConfiguration.DefaultMp4ToSmoothConfig);
            IAsset inputAsset = AssetTests.CreateAsset(_mediaContext, WindowsAzureMediaServicesTestConfiguration.SmallMp41, AssetCreationOptions.None);
            IMediaProcessor mediaProcessor = JobTests.GetMediaProcessor(_mediaContext, WindowsAzureMediaServicesTestConfiguration.MpPackagerName);
            IJob job = JobTests.CreateAndSubmitOneTaskJob(_mediaContext, "ApplyDynamicManifestFilter" + Guid.NewGuid().ToString().Substring(0, 5), mediaProcessor, configuration, inputAsset, TaskOptions.None);
            JobTests.WaitForJob(job.Id, JobState.Finished, JobTests.VerifyAllTasksFinished);


            var outputAsset = job.OutputMediaAssets.FirstOrDefault();
            outputAsset = _mediaContext.Assets.Where(c => c.Id == outputAsset.Id).FirstOrDefault();
            var assetFile = outputAsset.AssetFiles.Where(c => c.Name.EndsWith(".ism")).First();

            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("ApplyDynamicManifestFilter" + Guid.NewGuid().ToString().Substring(0, 5), TimeSpan.FromDays(30), AccessPermissions.Read);
            ILocator originLocator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, outputAsset, policy, DateTime.UtcNow.AddMinutes(-5));
            
            string urlForClientStreaming = originLocator.Path + assetFile.Name + "/manifest";
            HttpClient client = new HttpClient();
            var message = client.GetAsync(urlForClientStreaming).Result;
            var content = message.Content;
            var result = content.ReadAsStringAsync().Result;
            Assert.AreEqual(message.StatusCode,HttpStatusCode.OK);
            Assert.IsTrue(result.Length >0);
            
            Assert.IsTrue(result.Contains(typeAudio));
            
            Assert.IsTrue(result.Contains(typeVideo));

            var manifestLength = result.Length;

           // string filterName = "ApplyDynamicManifestFilter_" + DateTime.Now;
            string filterName = "ApplyDynamicManifestFilter_" + Guid.NewGuid().ToString().Substring(0,5);
            List<FilterTrackSelectStatement> filterTrackSelectStatements = new List<FilterTrackSelectStatement>();
            FilterTrackSelectStatement filterTrackSelectStatement = new FilterTrackSelectStatement();
            filterTrackSelectStatement.PropertyConditions = new List<IFilterTrackPropertyCondition>();
            filterTrackSelectStatement.PropertyConditions.Add(new FilterTrackTypeCondition(FilterTrackType.Video, FilterTrackCompareOperator.NotEqual));
            filterTrackSelectStatements.Add(filterTrackSelectStatement);
            IStreamingFilter filter = _mediaContext.Filters.Create(filterName, new PresentationTimeRange(), filterTrackSelectStatements);
            Assert.IsNotNull(filter);


            var filterUrlForClientStreaming = originLocator.Path + assetFile.Name + String.Format("/manifest(filter={0})",filterName);
            HttpClient filterclient = new HttpClient();
            var filtermessage = filterclient.GetAsync(filterUrlForClientStreaming).Result;
            Assert.AreEqual(filtermessage.StatusCode, HttpStatusCode.OK);
            var filtercontent = filtermessage.Content;
            var filterresult = filtercontent.ReadAsStringAsync().Result;
            Assert.IsTrue(filterresult.Length > 0);
            Assert.AreNotEqual(manifestLength, filterresult);
            Assert.IsTrue(filterresult.Contains(typeAudio));
            Assert.IsFalse(filterresult.Contains(typeVideo));

            outputAsset.DeleteAsync();
            inputAsset.DeleteAsync();
            job.DeleteAsync();
            filter.DeleteAsync();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void GlobalFilterFirstQualityTest()
        {
            string filterName = "Filter_FirstQuality_" + Guid.NewGuid().ToString();
            const int firstQualityBitrate = 32000;
            const int updatedFilterQualityBitrate = 128000;

            // Create filter with firstquality
            var filter = _mediaContext.Filters.Create(
                filterName,
                new PresentationTimeRange(10000000, 20000000, 320000000, TimeSpan.FromMinutes(20)),
                new FilterTrackSelectStatement[]
                {
                    new FilterTrackSelectStatement()
                    {
                        PropertyConditions = new IFilterTrackPropertyCondition[]
                        {
                            new FilterTrackBitrateRangeCondition(new FilterTrackBitrateRange(64000, 256000)),
                            new FilterTrackTypeCondition(FilterTrackType.Audio),
                        }
                    }
                },
                new FirstQuality(firstQualityBitrate));

            // Read filter
            var getFilter = _mediaContext.Filters.Where(f => f.Name == filterName).SingleOrDefault();
            Assert.IsNotNull(getFilter);
            Assert.AreEqual(getFilter.FirstQuality.Bitrate, firstQualityBitrate);

            // Update filter firstQuality
            filter.FirstQuality = new FirstQuality(updatedFilterQualityBitrate);
            filter.Update();

            // Read filter
            getFilter = _mediaContext.Filters.Where(f => f.Name == filterName).SingleOrDefault();
            Assert.IsNotNull(getFilter);
            Assert.AreEqual(getFilter.FirstQuality.Bitrate, updatedFilterQualityBitrate); 

            // Delete filter
            getFilter.Delete();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void AssetFilterFirstQualityTest()
        {
            string assetName = "Asset_Filter_FirstQuality" + Guid.NewGuid().ToString();
            var asset = _mediaContext.Assets.Create(assetName, AssetCreationOptions.None);
            Assert.IsNotNull(asset);

            string filterName = "AssetFilter_FirstQuality_" + Guid.NewGuid().ToString();
            const int firstQualityBitrate = 32000;
            const int updatedFilterQualityBitrate = 128000;

            // Create filter with firstquality
            var filter = asset.AssetFilters.Create(
                filterName,
                new PresentationTimeRange(10000000, 20000000, 320000000, TimeSpan.FromMinutes(20)),
                new FilterTrackSelectStatement[]
                {
                    new FilterTrackSelectStatement()
                    {
                        PropertyConditions = new IFilterTrackPropertyCondition[]
                        {
                            new FilterTrackBitrateRangeCondition(new FilterTrackBitrateRange(64000, 256000)),
                            new FilterTrackTypeCondition(FilterTrackType.Audio),
                        }
                    }
                },
                new FirstQuality(firstQualityBitrate));

            // Read filter
            asset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(asset);
            var getFilter = asset.AssetFilters.Where(f => f.Name == filterName).SingleOrDefault();
            Assert.IsNotNull(getFilter);
            Assert.AreEqual(getFilter.FirstQuality.Bitrate, firstQualityBitrate);

            // Update filter firstQuality
            filter.FirstQuality = new FirstQuality(updatedFilterQualityBitrate);
            filter.Update();

            // Read filter
            asset = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(asset);
            getFilter = asset.AssetFilters.Where(f => f.Name == filterName).SingleOrDefault();
            Assert.IsNotNull(getFilter);
            Assert.AreEqual(getFilter.FirstQuality.Bitrate, updatedFilterQualityBitrate);

            // Delete filter
            getFilter.Delete();

            // Delete asset
            asset.Delete();
        }        
    }
}