//-----------------------------------------------------------------------
// <copyright file="FilterTrackPropertyConditionData.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Track property condition data
    /// </summary>
    internal class FilterTrackPropertyConditionData
    {
        internal static readonly string TypeProperty = "Type";
        internal static readonly string NameProperty = "Name";
        internal static readonly string LanguageProperty = "Language";
        internal static readonly string FourCCProperty = "FourCC";
        internal static readonly string BitrateProperty = "Bitrate";

        public FilterTrackPropertyConditionData()
        {            
        }

        public FilterTrackPropertyConditionData(IFilterTrackPropertyCondition baseCondition)
        {
            Set(baseCondition as FilterTrackTypeCondition);
            Set(baseCondition as FilterTrackNameCondition);
            Set(baseCondition as FilterTrackLanguageCondition);
            Set(baseCondition as FilterTrackFourCCCondition);
            Set(baseCondition as FilterTrackBitrateRangeCondition);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private void Set(FilterTrackTypeCondition condition)
        {
            if (condition != null)
            {
                Property = TypeProperty;
                Value = condition.Value.ToString().ToLower(CultureInfo.InvariantCulture);
                Operator = condition.Operator.ToString();
            }
        }

        private void Set(FilterTrackNameCondition condition)
        {
            if (condition != null)
            {
                Property = NameProperty;
                Value = condition.Value;
                Operator = condition.Operator.ToString();
            }
        }

        private void Set(FilterTrackLanguageCondition condition)
        {
            if (condition != null)
            {
                Property = LanguageProperty;
                Value = condition.Value;
                Operator = condition.Operator.ToString();
            }
        }

        private void Set(FilterTrackFourCCCondition condition)
        {
            if (condition != null)
            {
                Property = FourCCProperty;
                Value = condition.Value;
                Operator = condition.Operator.ToString();
            }
        }

        private void Set(FilterTrackBitrateRangeCondition condition)
        {
            if (condition != null)
            {
                Property = BitrateProperty;
                FilterTrackBitrateRangeData rangeData =
                    new FilterTrackBitrateRangeData(condition.Value);
                Value = rangeData.Range;
                Operator = condition.Operator.ToString();
            }
        }

        /// <summary>
        /// FilterTrackProperty
        /// </summary>
        public String Property { get; set; }

        /// <summary>
        /// Value of property, depend on type
        /// </summary>
        public String Value { get; set; }

        /// <summary>
        /// FilterTrackCompareOperator
        /// </summary>
        public String Operator { get; set; }

    }
}
