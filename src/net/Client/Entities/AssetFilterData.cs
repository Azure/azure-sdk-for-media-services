//-----------------------------------------------------------------------
// <copyright file="AssetFilterData.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// AssetFilter data class holds AssetFilter entity from REST
    /// </summary>
    [DataServiceKey("Id")]
    internal class AssetFilterData : StreamingFilterData, IStreamingAssetFilter
    {

        public AssetFilterData()
        {
            ResourceSetName = AssetFilterBaseCollection.AssetFilterSet;
            Id = String.Empty;
        }

        public AssetFilterData(
            string parentAssetId,
            string name, 
            PresentationTimeRange timeRange,
            IList<FilterTrackSelectStatement> trackConditions,
            FirstQuality firstQuality = null)
            : base(name, timeRange, trackConditions, firstQuality)
        {
            ParentAssetId = parentAssetId;
            Id = String.Empty;
            ResourceSetName = AssetFilterBaseCollection.AssetFilterSet;
        }

        public string Id { get; set; }

        public string ParentAssetId { get; set; }
        
    }
}
