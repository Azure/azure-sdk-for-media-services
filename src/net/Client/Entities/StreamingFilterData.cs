//-----------------------------------------------------------------------
// <copyright file="StreamingFilterData.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Data.Services.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Name")]
    internal class StreamingFilterData : BaseEntity<IStreamingFilter>, IStreamingFilter
    {
        public StreamingFilterData()
        {
            FirstQuality = null;
            PresentationTimeRange = new PresentationTimeRangeData();
            Tracks = new List<FilterTrackSelectStatementData>();
            ResourceSetName = StreamingFilterBaseCollection.FilterSet;
        }

        public StreamingFilterData(
            string name, 
            PresentationTimeRange timeRange,
            IList<FilterTrackSelectStatement> trackConditions,
            FirstQuality firstQuality = null)
        {
            Name = name;
            FirstQuality = firstQuality != null ? new FirstQualityData(firstQuality) : null;
            PresentationTimeRange = timeRange != null
                ? new PresentationTimeRangeData(timeRange)
                : new PresentationTimeRangeData();
            Tracks = trackConditions != null
                ? trackConditions.Select(track => new FilterTrackSelectStatementData(track)).ToList()
                : new List<FilterTrackSelectStatementData>();
            ResourceSetName = StreamingFilterBaseCollection.FilterSet;
        }

        /// <summary>
        /// Name of filter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// First Quality
        /// </summary>
        public FirstQualityData FirstQuality { get; set; }

        FirstQuality IStreamingFilter.FirstQuality
        {
            get { return FirstQuality != null ? new FirstQuality(FirstQuality) : null; }
            set
            {
                if (value != null)
                {
                    FirstQuality = new FirstQualityData(value);
                }
            }
        }

        /// <summary>
        /// Presentation time range
        /// </summary>
        public PresentationTimeRangeData PresentationTimeRange { get; set; }

        PresentationTimeRange IStreamingFilter.PresentationTimeRange
        {
            get
            {
                return new PresentationTimeRange(PresentationTimeRange);
            }
            set
            {
                if (value != null)
                {
                    PresentationTimeRange = new PresentationTimeRangeData(value);
                }
            }
        }

        /// <summary>
        /// Track selection conditions
        /// </summary>
        public List<FilterTrackSelectStatementData> Tracks { get; set; }

        protected string ResourceSetName { get; set; }

        IList<FilterTrackSelectStatement> IStreamingFilter.Tracks
        {
            get { return Tracks.Select(track => new FilterTrackSelectStatement(track)).ToList().AsReadOnly(); }
            set
            {
                if (value != null)
                {
                    Tracks = value.Select(sel => new FilterTrackSelectStatementData(sel)).ToList();
                }
            }
        }

        /// <summary>
        /// Updates Filter asynchronouslly
        /// </summary>
        /// <returns></returns>
        public virtual Task<IMediaDataServiceResponse> UpdateAsync()
        {
            Validate();

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ResourceSetName, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Updates this instance
        /// </summary>
        public void Update()
        {
            AsyncHelper.Wait(UpdateAsync());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>        
        public virtual void Delete()
        {
            AsyncHelper.Wait(DeleteAsync());
        }

        /// <summary>
        /// Deletes this instance asynchronously.
        /// </summary>        
        public virtual Task<IMediaDataServiceResponse> DeleteAsync()
        {
            Validate();

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ResourceSetName, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(this));
        }

        private void Validate()
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new InvalidDataException("Filter name is empty");
            }

            if (Tracks == null)
            {
                Tracks = new List<FilterTrackSelectStatementData>();
            }
        }
    }
}
