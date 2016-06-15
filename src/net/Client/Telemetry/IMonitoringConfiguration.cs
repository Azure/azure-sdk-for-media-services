//-----------------------------------------------------------------------
// <copyright file="IMonitoringConfiguration.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// The monitoring configuration.
    /// </summary>
    public interface IMonitoringConfiguration
    {
        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the created time of the item
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the last modified time of the item
        /// </summary>
        DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the notification endpoint ID.
        /// </summary>
        string NotificationEndPointId { get; set; }

        /// <summary>
        /// Gets or sets the component monitoring settings.
        /// </summary>
        ICollection<IComponentMonitoringSetting> Settings { get; set; }

        /// <summary>
        /// Update the monitoring configuration object.
        /// </summary>
        void Update();

        /// <summary>
        /// Update the monitoring configuration object in asynchronous mode.
        /// </summary>
        /// <returns>Task of updating the monitoring configuration.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Delete the instance of monitoring configuration object.
        /// </summary>
        void Delete();

        /// <summary>
        /// Delete the instance of monitoring configuration object in asynchronous mode.
        /// </summary>
        /// <returns>Task of deleting the monitoring configuration.</returns>
        Task DeleteAsync();
    }
}
