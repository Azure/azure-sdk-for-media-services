//-----------------------------------------------------------------------
// <copyright file="MonitoringConfigurationSettingsCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of monitoring configuration.
    /// </summary>
    public sealed class MonitoringConfigurationCollection : CloudBaseCollection<IMonitoringConfiguration>
    {
        /// <summary>
        /// The entity set name for MonitoringConfigurations.
        /// </summary>
        internal const string MonitoringConfigurations = "MonitoringConfigurations";

        internal MonitoringConfigurationCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            Queryable = MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IMonitoringConfiguration, MonitoringConfiguration>(MonitoringConfigurations);
        }

        /// <summary>
        /// Create a monitoring configuration object in asynchronous mode.
        /// </summary>
        /// <param name="notificationEndPointId">notification endpoint id</param>
        /// <param name="settings">component settings</param>
        /// <returns>A Task that yields the monitoring configuration.</returns>
        public Task<IMonitoringConfiguration> CreateAsync(string notificationEndPointId, ICollection<ComponentMonitoringSetting> settings)
        {
            var monitoringConfiguration = new MonitoringConfiguration
            {
                NotificationEndPointId = notificationEndPointId,
                Settings = settings
            };
            monitoringConfiguration.SetMediaContext(MediaContext);

            var dataServiceContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataServiceContext.AddObject(MonitoringConfigurations, monitoringConfiguration);

            var retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataServiceContext as IRetryPolicyAdapter);
            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataServiceContext.SaveChangesAsync(monitoringConfiguration))
                .ContinueWith<IMonitoringConfiguration>(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        return (MonitoringConfiguration) t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously
                );
        }

        /// <summary>
        /// Create a monitoring configuration object.
        /// </summary>
        /// <param name="notificationEndPointId">notification endpoint id</param>
        /// <param name="settings">component settings</param>
        /// <returns>The monitoring configuration.</returns>
        public IMonitoringConfiguration Create(string notificationEndPointId, ICollection<ComponentMonitoringSetting> settings)
        {
            return AsyncHelper.Wait(CreateAsync(notificationEndPointId, settings));
        }
    }
}
