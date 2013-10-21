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
    /// Describes the Origin Metrics Entity
    /// </summary>
    public interface IOriginMetric : IMetric
    {
        /// <summary>
        /// Gets the origin name of the metric
        /// </summary>
        string OriginName { get; }

        /// <summary>
        /// Gets a collection of <see cref="Metric"/> objects containing the egress metrics of the origin.
        /// </summary>
        ReadOnlyCollection<Metric> EgressMetrics { get; }
    }
}
