//-----------------------------------------------------------------------
// <copyright file="IComponentMonitoringSetting.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// The interface of monitoring settings for a component.
    /// </summary>
    public interface IComponentMonitoringSetting
    {
        /// <summary>
        /// Gets or sets the monitoring component.
        /// </summary>
        MonitoringComponent Component { get; set; }

        /// <summary>
        /// Gets or sets the monitoring level.
        /// </summary>
        MonitoringLevel Level { get; set; }
    }
}