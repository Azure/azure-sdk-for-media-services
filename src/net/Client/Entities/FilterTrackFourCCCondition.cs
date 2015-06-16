//-----------------------------------------------------------------------
// <copyright file="FilterTrackFourCCCondition.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
    /// FourCC condition
    /// </summary>
    public class FilterTrackFourCCCondition : FilterTrackPropertyBaseCondition
    {
        public FilterTrackFourCCCondition()
        {            
        }

        /// <summary>
        /// Initilizing FilterTrackFourCCCondition
        /// </summary>
        /// <param name="codecFormat">The first element of codecs format, as specified in RFC 6381.</param>
        /// <param name="filterTrackCompareOperator"><see cref="FilterTrackCompareOperator">FilterTrackCompareOperator</see></param>
        public FilterTrackFourCCCondition(string codecFormat, FilterTrackCompareOperator filterTrackCompareOperator = FilterTrackCompareOperator.Equal)
            : base(filterTrackCompareOperator)
        {
            Value = codecFormat;
        }

        /// <summary>
        ///  Initilizing FilterTrackFourCCCondition
        /// </summary>
        /// <param name="codecFormat">The first element of codecs format, as specified in RFC 6381.</param>
        /// <param name="filterTrackCompareOperator">String representation of <see cref="FilterTrackCompareOperator">FilterTrackCompareOperator</see></param>
        internal FilterTrackFourCCCondition(string codecFormat, string filterTrackCompareOperator)
            : base(filterTrackCompareOperator)
        {
            Value = codecFormat;
        }

        /// <summary>
        /// The first element of codecs format, as specified in RFC 6381.
        /// </summary>
        public string Value { get; private set; }
    }
}
