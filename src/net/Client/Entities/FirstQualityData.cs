﻿//-----------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// First quality element for filter
    /// </summary>
    internal class FirstQualityData
    {
        public FirstQualityData()
        {
            Bitrate = 1;
        }

        public FirstQualityData(int bitrate)
        {
            Bitrate = bitrate;
        }

        public FirstQualityData(FirstQuality firstQuality)
        {
            if (firstQuality == null)
            {
                throw new ArgumentNullException("firstQuality");
            }

            Bitrate = firstQuality.Bitrate;
        }

        /// <summary>
        /// Bitrate of first quality
        /// </summary>
        public int Bitrate { get; set; } 
    }
}