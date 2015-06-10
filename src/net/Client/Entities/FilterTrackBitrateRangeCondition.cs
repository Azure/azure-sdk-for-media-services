//-----------------------------------------------------------------------
// <copyright file="FilterTrackBitrateRangeCondition.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Bitrate range condition
    /// </summary>
    public class FilterTrackBitrateRangeCondition : FilterTrackPropertyBaseCondition
    {
        public FilterTrackBitrateRangeCondition()
        {            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterTrackBitrateRange">A range of bitrates or a specific bitrate. For example, 0-2427000.</param>
        /// <param name="filterTrackCompareOperator"><see cref="FilterTrackCompareOperator"/></param>
        public FilterTrackBitrateRangeCondition(FilterTrackBitrateRange filterTrackBitrateRange, FilterTrackCompareOperator filterTrackCompareOperator = FilterTrackCompareOperator.Equal)
            : base(filterTrackCompareOperator)
        {
            Value = filterTrackBitrateRange;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterTrackBitrateRange">string representation of <see cref="FilterTrackBitrateRange"/></param>
        /// <param name="filterTrackCompareOperator"> string representation of <see cref="FilterTrackCompareOperator"/></param>
        internal FilterTrackBitrateRangeCondition(string filterTrackBitrateRange, string filterTrackCompareOperator)
            : base(filterTrackCompareOperator)
        {
            Value = new FilterTrackBitrateRange(filterTrackBitrateRange);
        }
        /// <summary>
        /// A range of bitrates or a specific bitrate. For example, 0-2427000.
        /// </summary>
        public FilterTrackBitrateRange Value { get; private set; }
    }
}
