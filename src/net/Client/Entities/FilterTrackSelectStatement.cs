//-----------------------------------------------------------------------
// <copyright file="FilterTrackSelectStatement.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Track select statement
    /// </summary>
    public class FilterTrackSelectStatement
    {
        public FilterTrackSelectStatement()
        {
            PropertyConditions = new List<IFilterTrackPropertyCondition>();
        }

        internal FilterTrackSelectStatement(FilterTrackSelectStatementData data)
        {
            PropertyConditions = data.PropertyConditions.Select(pc => FromData(pc)).ToList().AsReadOnly();
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<IFilterTrackPropertyCondition> PropertyConditions { get; set; }

        private static IFilterTrackPropertyCondition FromData(FilterTrackPropertyConditionData data)
        {
            if (data.Property == FilterTrackPropertyConditionData.TypeProperty)
            {
                return new FilterTrackTypeCondition(data.Value, data.Operator);
            }
            else if (data.Property == FilterTrackPropertyConditionData.NameProperty)
            {
                return new FilterTrackNameCondition(data.Value, data.Operator);
            }
            else if (data.Property == FilterTrackPropertyConditionData.LanguageProperty)
            {
                return new FilterTrackLanguageCondition(data.Value, data.Operator);
            }
            else if (data.Property == FilterTrackPropertyConditionData.FourCCProperty)
            {
                return new FilterTrackFourCCCondition(data.Value, data.Operator);
            }
            else if (data.Property == FilterTrackPropertyConditionData.BitrateProperty)
            {
                return new FilterTrackBitrateRangeCondition(data.Value, data.Operator);
            }

            return null;
        }
    }
}
