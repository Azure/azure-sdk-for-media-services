//-----------------------------------------------------------------------
// <copyright file="AssetFilterBaseCollection.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class AssetFilterBaseCollection : BaseCollection<IStreamingAssetFilter>
    {
        public static readonly string AssetFilterSet = "AssetFilters";
        private IAsset _parentAsset;
        private List<IStreamingAssetFilter> _filterData;

        internal AssetFilterBaseCollection(MediaContextBase cloudMediaContext, IAsset parentAsset, List<AssetFilterData> filterDatas)
            : base(cloudMediaContext)
        {
            _parentAsset = parentAsset;
            _filterData = filterDatas.Select(af => af as IStreamingAssetFilter).ToList();
        }

        /// <summary>
        /// Gets the queryable collection of programs.
        /// </summary>
        protected override IQueryable<IStreamingAssetFilter> Queryable
        {
            get { return _filterData.AsQueryable(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Creates new Filter
        /// </summary>
        /// <param name="name">filter name</param>
        /// <param name="timeRange">streaming time range</param>
        /// <param name="trackConditions">filter track conditions</param>
        /// <param name="firstQuality">first quality</param>
        /// <returns>The created filter.</returns>
        public IStreamingAssetFilter Create(
            string name, 
            PresentationTimeRange timeRange, 
            IList<FilterTrackSelectStatement> trackConditions,
            FirstQuality firstQuality = null)
        {
            return AsyncHelper.Wait(CreateAsync(name, timeRange, trackConditions, firstQuality));
        }

        /// <summary>
        /// Asynchronously creates new StreamingFilter.
        /// </summary>
        /// <param name="name">filter name</param>
        /// <param name="timeRange">filter boundaries</param>
        /// <param name="trackConditions">filter track conditions</param>
        /// <param name="firstQuality">first quality</param>
        /// <returns>The task to create the filter.</returns>
        public Task<IStreamingAssetFilter> CreateAsync(
            string name, 
            PresentationTimeRange timeRange, 
            IList<FilterTrackSelectStatement> trackConditions,
            FirstQuality firstQuality = null)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            AssetFilterData filter = new AssetFilterData(_parentAsset.Id, name, timeRange, trackConditions, firstQuality);

            filter.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(AssetFilterSet, filter);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(filter))
                .ContinueWith<IStreamingAssetFilter>(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        return (AssetFilterData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
