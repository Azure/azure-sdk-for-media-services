// Copyright 2014 Microsoft Corporation
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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a Streaming Endpoint.
    /// </summary>
    public interface IStreamingEndpoint
    {
        /// <summary>
        /// Gets Unique identifier of the streaming endpoint.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the streaming endpoint.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets description of the streaming endpoint.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets host name of the streaming endpoint.
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// Gets or sets custom host names
        /// </summary>
        IList<string> CustomHostNames { get; set; }

        /// <summary>
        /// Gets streaming endpoint creation date.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets streaming endpoint last modification date.
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// Gets the number of scale Units of the streaming endpoint.
        /// </summary>
        int? ScaleUnits { get; }

        /// <summary>
        /// Gets streaming endpoint state.
        /// </summary>
        StreamingEndpointState State { get; }

        /// <summary>
        /// Gets or sets if CDN to be enabled on this Streaming Endpoint.
        /// </summary>
        bool CdnEnabled { get; set; }

        /// <summary>
        /// Gets or sets Cdn provider.
        /// </summary>
        string CdnProvider { get; set; }

        /// <summary>
        /// Gets or sets Cdn profile.
        /// </summary>
        string CdnProfile { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint version.
        /// </summary>
        string StreamingEndpointVersion { get; set; }

        /// <summary>
        /// Gets the free trial end time as a string.
        /// </summary>
        DateTime FreeTrialEndTime { get; }

        /// <summary>
        /// Gets or sets cross site access policies.
        /// </summary>
        CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint access control
        /// </summary>
        StreamingEndpointAccessControl AccessControl { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint cache control
        /// </summary>
        StreamingEndpointCacheControl CacheControl { get; set; }
        
        /// <summary>
        /// Starts the streaming endpoint.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the streaming endpoint asynchronously.
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
        /// Stops the streaming endpoint.
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the streaming endpoint asynchronously.
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
        /// Updates this streaming endpoint instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Asynchronously updates this streaming endpoint instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task UpdateAsync();

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
        /// Deletes the streaming endpoint.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the streaming endpoint asynchronously.
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
        /// Scales the streaming endpoint.
        /// </summary>
        /// <param name="scaleUnits">New scale units.</param>
        void Scale(int scaleUnits);

        /// <summary>
        /// Scales the streaming endpoint.
        /// </summary>
        /// <param name="scaleUnits">New scale units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        Task ScaleAsync(int scaleUnits);

        /// <summary>
        /// Sends scale operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendScaleOperation(int scaleUnits);

        /// <summary>
        /// Sends scale operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendScaleOperationAsync(int scaleUnits);
    }
}
