// Copyright 2012 Microsoft Corporation
// 
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

using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the IngestMetrics complex type
    /// </summary>
    public class IngestMetric
    {
        /// <summary>
        /// ID of different streams (qualities)
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Stream track ID, audio or video
        /// e.g. 1
        /// </summary>
        public Int32 TrackId { get; set; }

        /// <summary>
        /// Incoming fragment track name
        /// </summary>
        public string TrackName { get; set; }

        /// <summary>
        /// a collection of stream metrics
        /// </summary>
        public List<Metric> StreamMetrics { get; set; }
    }
}
