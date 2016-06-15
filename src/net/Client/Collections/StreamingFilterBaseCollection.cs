//-----------------------------------------------------------------------
// <copyright file="StreamingFilterBaseCollection.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
    public class StreamingFilterBaseCollection : BaseCollection<IStreamingFilter>
    {
        public static readonly string FilterSet = "Filters";
        private readonly Lazy<IQueryable<IStreamingFilter>> _filterQuery;

        internal StreamingFilterBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            var dataContext = cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            _filterQuery = new Lazy<IQueryable<IStreamingFilter>>(() => dataContext.CreateQuery<IStreamingFilter, StreamingFilterData>(FilterSet));
        }

        /// <summary>
        /// Gets the queryable collection of programs.
        /// </summary>
        protected override IQueryable<IStreamingFilter> Queryable
        {
            get { return _filterQuery.Value; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Creates new Filter
        /// </summary>
        /// <param name="name">filter name</param>
        /// <param name="timeRange">streaming time range</param>
        /// <param name="trackConditions">filter track conditions</param>
        /// <param name="firstQuality">filter first quality bitrate</param>
        /// <returns>The created filter.</returns>
        public IStreamingFilter Create(
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
        /// <param name="timeRange">streaming time range</param>
        /// <param name="trackConditions">filter track conditions</param>
        /// <param name="firstQuality">filter first quality bitrate</param>
        /// <returns>The task to create the filter.</returns>
        public Task<IStreamingFilter> CreateAsync(
            string name, 
            PresentationTimeRange timeRange, 
            IList<FilterTrackSelectStatement> trackConditions,
            FirstQuality firstQuality = null)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            StreamingFilterData filter = new StreamingFilterData(name, timeRange, trackConditions, firstQuality);

            filter.SetMediaContext(MediaContext);
            
            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(FilterSet, filter);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(filter))
                .ContinueWith<IStreamingFilter>(                    
                    t =>
                    {
                        t.ThrowIfFaulted();
                        return (StreamingFilterData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
