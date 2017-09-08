//-----------------------------------------------------------------------
// <copyright file="PresentationTimeRange.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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
using System.IO;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Define a presentation time range
    /// </summary>
    public class PresentationTimeRange
    {
        public const UInt64 TimescaleHns = 10000000;


        public PresentationTimeRange(
            UInt64? timescale = TimescaleHns, 
            UInt64? start = null,
            UInt64? end = null,
            TimeSpan? pwDuration = null,
            TimeSpan? backoff = null,
            bool forceEnd = false)
        {
            Timescale = timescale;
            StartTimestamp = start;
            EndTimestamp = end;
            PresentationWindowDuration = pwDuration;
            LiveBackoffDuration = backoff;
            ForceEndTimestamp = forceEnd;
            Validate();
        }

        internal PresentationTimeRange(PresentationTimeRangeData data)
        {
            if (data.Timescale != (Int64) TimescaleHns)
            {
                Timescale = (UInt64)data.Timescale;
            }

            if (data.StartTimestamp > 0)
            {
                StartTimestamp = (UInt64) data.StartTimestamp;
            }

            if (data.EndTimestamp != Int64.MaxValue)
            {
                EndTimestamp = (UInt64)data.EndTimestamp;
            }

            if (data.PresentationWindowDuration != Int64.MaxValue)
            {
                PresentationWindowDuration =
                    TimeSpan.FromMilliseconds((data.PresentationWindowDuration/data.Timescale)*1000);
            }

            if (data.LiveBackoffDuration != 0)
            {
                LiveBackoffDuration =
                    TimeSpan.FromMilliseconds((data.LiveBackoffDuration / data.Timescale) * 1000);
            }

            Validate();
        }

        /// <summary>
        ///  Timescale
        /// </summary>
        public UInt64? Timescale { get; private set; }

        /// <summary>
        /// Define an absolute start point
        /// </summary>
        public UInt64? StartTimestamp { get; private set; }

        /// <summary>
        /// Define an absolute end point
        /// </summary>
        public UInt64? EndTimestamp { get; private set; }

        /// <summary>
        /// Define a sliding window, left edge is relative to the end
        /// </summary>
        public TimeSpan? PresentationWindowDuration { get; private set; }

        /// <summary>
        /// Define a live back off, presentation window right edge is relative to the end
        /// </summary>
        public TimeSpan? LiveBackoffDuration { get; private set; }

        /// <summary>
        /// Define a property that forces server to apply end (right edge) to the resulting manifest 
        /// </summary>
        public bool ForceEndTimestamp { get; private set; }

        private void Validate()
        {
            if (StartTimestamp > EndTimestamp)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "StartTimestamp is larger than EndTimestamp"));
            }

            if (ForceEndTimestamp == true && !EndTimestamp.HasValue)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "ForceEndtimestamp is present when EndTimestamp is not present"));
            }
        }
    }
}
