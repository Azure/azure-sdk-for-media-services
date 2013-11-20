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
using System.Collections.ObjectModel;
using System.Data.Services.Common;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class ChannelMetricData : BaseEntity<IChannelMetric>, IChannelMetric
    {
        /// <summary>
        /// Gets and sets Unique identifier of the Metric.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets channel name of the metric
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets metric last modification timestamp.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the <see cref="IngestMetrics"/> object containing the ingest metrics of the channel.
        /// </summary>
        public List<IngestMetricData> IngestMetrics { get; set; }

        /// <summary>
        /// Gets the <see cref="ProgramMetrics"/> object containing the program metrics of the channel.
        /// </summary>
        public List<ProgramMetricData> ProgramMetrics { get; set; }

        /// <summary>
        /// Gets the <see cref="IngestMetrics"/> object containing the ingest metrics of the channel.
        /// </summary>
        ReadOnlyCollection<IIngestMetric> IChannelMetric.IngestMetrics
        {
            get
            {
                return IngestMetrics.ToList<IIngestMetric>().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="ProgramMetrics"/> object containing the program metrics of the channel.
        /// </summary>
        ReadOnlyCollection<IProgramMetric> IChannelMetric.ProgramMetrics
        {
            get
            {
                return ProgramMetrics.ToList<IProgramMetric>().AsReadOnly();
            }
        }
    }
}
