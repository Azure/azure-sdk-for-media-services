//-----------------------------------------------------------------------
// <copyright file="MonitoringConfiguration.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// The monitoring configuration.
    /// </summary>
    [DataServiceKey("Id")]
    internal class MonitoringConfiguration : BaseEntity<IMonitoringConfiguration>, IMonitoringConfiguration
    {
        private string _notificationEndPointId;
        private ICollection<ComponentMonitoringSetting> _settings;

        /// <summary>
        /// Gets or sets the item ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the created time of the item
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the last modified time of the item
        /// </summary>
        public DateTime LastModified { get; set; }
        
        /// <summary>
        /// Gets or sets the notification endpoint ID.
        /// </summary>
        public string NotificationEndPointId
        {
            get { return _notificationEndPointId; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("NotificationEndPointId");
                }
                _notificationEndPointId = value;
            }
        }

        /// <summary>
        /// Gets or sets the component monitoring settings.
        /// </summary>
        public ICollection<ComponentMonitoringSetting> Settings
        {
            get { return _settings; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Settings");
                }
                _settings = value;
            }
        }

        /// <summary>
        /// Gets or sets the component monitoring settings of the interface format.
        /// </summary>
        ICollection<IComponentMonitoringSetting> IMonitoringConfiguration.Settings
        {
            get { return _settings.Cast<IComponentMonitoringSetting>().ToList(); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Settings");
                }
                _settings = value.Cast<ComponentMonitoringSetting>().ToList();
            }
        }

        /// <summary>
        /// Update the monitoring configuration object.
        /// </summary>
        public void Update()
        {
            AsyncHelper.Wait(this.UpdateAsync());
        }

        /// <summary>
        /// Update the monitoring configuration object in asynchronous mode.
        /// </summary>
        /// <returns>Task of updating the monitoring configuration.</returns>
        public Task UpdateAsync()
        {
            var dataServiceContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataServiceContext.AttachTo(MonitoringConfigurationCollection.MonitoringConfigurations, this);
            dataServiceContext.UpdateObject(this);

            var retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataServiceContext as IRetryPolicyAdapter);
            return retryPolicy.ExecuteAsync(() => dataServiceContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Delete the instance of monitoring configuration object.
        /// </summary>
        public void Delete()
        {
            AsyncHelper.Wait(DeleteAsync());
        }

        /// <summary>
        /// Delete the instance of monitoring configuration object in asynchronous mode.
        /// </summary>
        /// <returns>Task of deleting the monitoring configuration.</returns>
        public Task DeleteAsync()
        {
            var dataServiceContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataServiceContext.AttachTo(MonitoringConfigurationCollection.MonitoringConfigurations, this);
            dataServiceContext.DeleteObject(this);

            var retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataServiceContext as IRetryPolicyAdapter);
            return retryPolicy.ExecuteAsync(() => dataServiceContext.SaveChangesAsync(this));
        }
    }
}
