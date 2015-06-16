//-----------------------------------------------------------------------
// <copyright file="FilterTrackBitrateRangeData.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

using System.Globalization;
using System.Text;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Bitrate range condition data
    /// </summary>
    internal class FilterTrackBitrateRangeData
    {
        internal const char cRangeSign = '-';

        public FilterTrackBitrateRangeData()
        {            
        }

        public FilterTrackBitrateRangeData(FilterTrackBitrateRange range)
        {
            Range = Serialize(range);
        }

        public string Range { get; set; }

        /// <summary>
        /// Serialize bitrate to a string presentation like 128000-56000
        /// </summary>
        private static string Serialize(FilterTrackBitrateRange range)
        {
            if (!range.LowBound.HasValue && !range.HighBound.HasValue)
            {
                return string.Empty;
            }

            if (range.LowBound.HasValue && range.HighBound.HasValue && range.LowBound == range.HighBound)
            {
                return range.LowBound.Value.ToString(CultureInfo.InvariantCulture);
            }

            StringBuilder builder = new StringBuilder();

            if (range.LowBound.HasValue)
            {
                builder.Append(range.LowBound.Value.ToString(CultureInfo.InvariantCulture));
            }

            builder.Append(cRangeSign);

            if (range.HighBound.HasValue)
            {
                builder.Append(range.HighBound.Value.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

    }
}
