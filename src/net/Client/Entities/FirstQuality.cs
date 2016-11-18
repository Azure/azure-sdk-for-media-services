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

using System;
using System.Globalization;
using System.Web.UI;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// First Quality of Manifest Filter, indicates the first quality bitrate for HLS
    /// </summary>
    public class FirstQuality
    {
        public FirstQuality(int bitrate)
        {
            Bitrate = bitrate;

            Validate();
        }

        internal FirstQuality(FirstQualityData firstQualityData)
        {
            Bitrate = firstQualityData.Bitrate;

            Validate();   
        }

        /// <summary>
        /// Bitrate of First Quality
        /// </summary>
        public int Bitrate { get; private set; }

        private void Validate()
        {
            if (Bitrate <= 0)
            {
                throw new ArgumentOutOfRangeException("Bitrate", "Bitrate must be larger than 0");
            }
        }
    }
}