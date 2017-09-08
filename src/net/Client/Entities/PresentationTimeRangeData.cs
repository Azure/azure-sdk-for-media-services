//-----------------------------------------------------------------------
// <copyright file="PresentationTimeRangeData.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// PresentationTimeRangeData holds PresentationTimeRange property from REST
    /// </summary>
    public class PresentationTimeRangeData
    {
        public PresentationTimeRangeData()
        {
            Timescale = (Int64)PresentationTimeRange.TimescaleHns;
            StartTimestamp = 0;
            EndTimestamp = Int64.MaxValue;
            PresentationWindowDuration = Int64.MaxValue;
            LiveBackoffDuration = 0;
            ForceEndTimestamp = false;
        }

        public PresentationTimeRangeData(PresentationTimeRange range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }

            Timescale = (Int64)(range.Timescale ?? PresentationTimeRange.TimescaleHns);
            StartTimestamp = (Int64) (range.StartTimestamp ?? 0);
            EndTimestamp = (Int64) (range.EndTimestamp ?? Int64.MaxValue);
            ForceEndTimestamp = range.ForceEndTimestamp;

            PresentationWindowDuration = range.PresentationWindowDuration.HasValue && range.PresentationWindowDuration.Value != TimeSpan.MaxValue ? 
                (Int64)range.PresentationWindowDuration.Value.TotalMilliseconds * (Timescale / 1000) : 
                Int64.MaxValue;
            LiveBackoffDuration = range.LiveBackoffDuration.HasValue && range.LiveBackoffDuration.Value != TimeSpan.MaxValue ? 
                (Int64)range.LiveBackoffDuration.Value.TotalMilliseconds * (Timescale / 1000) : 
                0;
        }

        /// <summary>
        ///  Timescale
        /// </summary>
        public Int64 Timescale { get; set; }

        /// <summary>
        /// Define an absolute start point
        /// </summary>
        public Int64 StartTimestamp { get; set; }

        /// <summary>
        /// Define an absolute end point
        /// </summary>
        public Int64 EndTimestamp { get; set; }

        /// <summary>
        /// Define a sliding window, left edge is relative to the end
        /// </summary>
        public Int64 PresentationWindowDuration { get; set; }

        /// <summary>
        /// Define a live back off, presentation window right edge is relative to the end
        /// </summary>
        public Int64 LiveBackoffDuration { get; set; }

        /// <summary>
        /// Define a property that forces server to apply end (right edge) to the resulting manifest 
        /// </summary>
        public bool ForceEndTimestamp { get; set; }

    }
}
