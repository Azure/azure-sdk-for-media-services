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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IChannelMetric"/>.
    /// </summary>
    public sealed class ChannelMetricBaseCollection : MetricBaseCollection<IChannelMetric>
    {
        internal const string ChannelMetricSet = "ChannelMetrics";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMetricBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal ChannelMetricBaseCollection(CloudMediaContext cloudMediaContext) : base(cloudMediaContext)
        {
			Queryable = cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IChannelMetric, ChannelMetricData>(ChannelMetricSet);

            Initialize(Queryable);
        }
    }
}
