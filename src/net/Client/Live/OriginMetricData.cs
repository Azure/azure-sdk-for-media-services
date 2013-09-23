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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class OriginMetricData : RestEntity<OriginMetricData>, IOriginMetric, ICloudMediaContextInit
    {
        /// <summary>
        /// Gets metric last modification timestamp.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets service name of the origin metric
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the Egress Metrics.
        /// </summary>
        public List<Metric> EgressMetrics { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="Metric"/> objects containing the egress metrics of the origin.
        /// </summary>
        ReadOnlyCollection<Metric> IOriginMetric.EgressMetrics
        {
            get
            {
                return EgressMetrics.AsReadOnly();
            }
        }

        #region ICloudMediaContextInit Members

        /// <summary>
        /// Initializes the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            _cloudMediaContext = context;
        }

        #endregion

        protected override string EntitySetName
        {
            get { return OriginMetricBaseCollection.OriginMetricSet; }
        }
    }
}
