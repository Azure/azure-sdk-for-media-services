//-----------------------------------------------------------------------
// <copyright file="FilterTrackLanguageCondition.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
    /// Language condition
    /// </summary>
    public class FilterTrackLanguageCondition : FilterTrackPropertyBaseCondition
    {
        public FilterTrackLanguageCondition()
        {            
        }
        /// <summary>
        /// Initilizes FilterTrackLanguageCondition
        /// </summary>
        /// <param name="languageValue"></param>
        /// <param name="filterTrackCompareOperator"><see cref="FilterTrackCompareOperator"/></param>
        public FilterTrackLanguageCondition(string languageValue, FilterTrackCompareOperator filterTrackCompareOperator = FilterTrackCompareOperator.Equal)
            : base(filterTrackCompareOperator)
        {
            Value = languageValue;
        }
        /// <summary>
        /// Initilizes FilterTrackLanguageCondition
        /// </summary>
        ///<param name="languageValue"> Tag of a language you want to include, as specified in RFC 5646. For example, en.
        ///</param>
        /// <param name="filterTrackCompareOperator">string representation of <see cref="FilterTrackCompareOperator"/></param>
        internal FilterTrackLanguageCondition(string languageValue, string filterTrackCompareOperator)
            : base(filterTrackCompareOperator)
        {
            Value = languageValue;
        }
        /// <summary>
        /// Tag of a language you want to include, as specified in RFC 5646. For example, en.
        /// </summary>
        public string Value { get; private set; }
    }
}
