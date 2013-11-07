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
using System.Collections.ObjectModel;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public interface IProgramMetric
    {
        /// <summary>
        /// The program Id of the metrics
        /// </summary>
        Guid ProgramId { get; }

        /// <summary>
        /// ID of different streams (qualities)
        /// </summary>
        string StreamId { get; }

        /// <summary>
        /// Stream track ID, audio or video
        /// e.g. 1
        /// </summary>
        Int32 TrackId { get; }

        /// <summary>
        /// Stream track name, as set by the encoder
        /// </summary>
        string TrackName { get; }

        /// <summary>
        /// a collection of archieve metrics
        /// </summary>
        ReadOnlyCollection<Metric> ArchiveMetrics { get; }
    }
}
