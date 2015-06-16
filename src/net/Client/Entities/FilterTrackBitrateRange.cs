//-----------------------------------------------------------------------
// <copyright file="FilterTrackBitrateRange.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Define a bitrate range for track selection condition
    /// </summary>
    public class FilterTrackBitrateRange
    {
        public FilterTrackBitrateRange()
        {
        }

        public FilterTrackBitrateRange(Int32? lowBound, Int32? highBound)
        {
            LowBound = lowBound;
            HighBound = highBound;
        }

        internal FilterTrackBitrateRange(FilterTrackBitrateRange other)
        {
            LowBound = other.LowBound;
            HighBound = other.HighBound;
        }

        internal FilterTrackBitrateRange(string strValue)
        {
            Parse(strValue);
        }

        /// <summary>
        /// Define low bound of bitrate
        /// </summary>
        public Int32? LowBound { get; set; }

        /// <summary>
        /// Define high bound of bitrate
        /// </summary>
        public Int32? HighBound { get; set; }

        private void Parse(string strBitrate)
        {
            String[] sepStrings = strBitrate.Split(new char[] { FilterTrackBitrateRangeData.cRangeSign });

            if (sepStrings.Count() > 1)
            {
                if (!String.IsNullOrEmpty(sepStrings[0]))
                {
                    LowBound = Int32.Parse(sepStrings[0], CultureInfo.InvariantCulture);
                }
                else
                {
                    LowBound = null;
                }

                if (!String.IsNullOrEmpty(sepStrings[1]))
                {
                    HighBound = Int32.Parse(sepStrings[1], CultureInfo.InvariantCulture);
                }
                else
                {
                    HighBound = null;
                }
            }
            else
            {
                LowBound = HighBound = Int32.Parse(strBitrate, CultureInfo.InvariantCulture);
            }            
        }
    }
}
