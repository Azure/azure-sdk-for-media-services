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

using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the IngestMetrics complex type
    /// </summary>
    public class IngestMetrics
    {
        /// <summary>
        /// IP address of different encoders for the same stream
        /// e.g. 127.0.0.1
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// ID of different streams (qualities)
        /// </summary>
        public string StreamId { get; set; }

        /// <summary>
        /// Stream track ID, audio or video
        /// e.g. 1
        /// </summary>
        public string TrackId { get; set; }

        /// <summary>
        /// a collection of stream metrics
        /// </summary>
        public ReadOnlyCollection<Metric> StreamMetrics { get; set; }
    }
}
