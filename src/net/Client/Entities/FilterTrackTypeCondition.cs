//-----------------------------------------------------------------------
// <copyright file="FilterTrackTypeCondition.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

using System.IO;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Type condition for track
    /// </summary>
    public class FilterTrackTypeCondition : FilterTrackPropertyBaseCondition
    {
        public FilterTrackTypeCondition()
        {            
        }

        public FilterTrackTypeCondition(FilterTrackType type, FilterTrackCompareOperator op = FilterTrackCompareOperator.Equal)
            : base(op)
        {
            Value = type;
        }

        internal FilterTrackTypeCondition(string strTypeValue, string strOperator)
            : base(strOperator)
        {
            Parse(strTypeValue);            
        }

        public FilterTrackType Value { get; set; }

        private void Parse(string strTypeValue)
        {
            FilterTrackType trackType;
            if (!FilterTrackType.TryParse(strTypeValue, true, out trackType))
            {
                throw new InvalidDataException("Filter track type is invalid");
            }

            Value = trackType;
        }
    }
}
