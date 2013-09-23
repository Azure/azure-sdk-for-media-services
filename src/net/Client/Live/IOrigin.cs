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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes an Origin.
    /// </summary>
    public interface IOrigin
    {
        /// <summary>
        /// Gets Unique identifier of the origin.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets name of the origin.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the origin.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets host name of the origin.
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// Gets or sets origin settings.
        /// </summary>
        OriginSettings Settings { get; set; }

        /// <summary>
        /// Gets origin creation date.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets origin last modification date.
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// Gets or sets the number of Reserved Units of the origin.
        /// </summary>
        int ReservedUnits { get; set; }

        /// <summary>
        /// Gets origin state.
        /// </summary>
        OriginState State { get; }

        /// <summary>
        /// Gets origin metrics monitor object
        /// </summary>
        SingleOriginMetricsMonitor MetricsMonitor { get; }

        /// <summary>
        /// Deletes the origin.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the origin asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Sends delete operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendDeleteOperation();

        /// <summary>
        /// Sends delete operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendDeleteOperationAsync();

        /// <summary>
        /// Starts the origin.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the origin asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task StartAsync();

        /// <summary>
        /// Sends start operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendStartOperation();

        /// <summary>
        /// Sends start operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendStartOperationAsync();

        /// <summary>
        /// Stops the origin.
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the origin asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task StopAsync();

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendStopOperation();

        /// <summary>
        /// Sends stop operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendStopOperationAsync();

        /// <summary>
        /// Asynchronously updates this origin instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Updates this origin instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Sends update request to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendUpdateOperation();

        /// <summary>
        /// Sends update request to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendUpdateOperationAsync();

        /// <summary>
        /// Scales the origin.
        /// </summary>
        /// <param name="reservedUnits">New reserved units.</param>
        void Scale(int reservedUnits);

        /// <summary>
        /// Scales the origin.
        /// </summary>
        /// <param name="reservedUnits">New reserved units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        Task ScaleAsync(int reservedUnits);

        /// <summary>
        /// Sends scale operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendScaleOperation(int reservedUnits);

        /// <summary>
        /// Sends scale operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendScaleOperationAsync(int reservedUnits);

        /// <summary>
        /// Get the latest origin metric.
        /// </summary>
        /// <returns>The latest OriginMetrics entity of this origin service</returns>
        IOriginMetric GetMetric();
    }
}
