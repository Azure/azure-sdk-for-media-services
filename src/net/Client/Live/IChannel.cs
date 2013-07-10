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
    /// Describes a Channel.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Gets unique identifier of the Channel.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the description of the channel.
        /// </summary>
        string Description { get; set; }
        
        /// <summary>
        /// Gets channel creation date.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets channel last modification date.
        /// </summary>
        DateTime LastModified { get; set; }

        /// <summary>
        /// Gets Url of the preview.
        /// </summary>
        Uri PreviewUrl { get; }

        /// <summary>
        /// Gets ingest Url.
        /// </summary>
        Uri IngestUrl { get; }

        /// <summary>
        /// Gets state of the channel.
        /// </summary>
        ChannelState State { get; }

        /// <summary>
        /// Gets or sets size of the channel.
        /// </summary>
        ChannelSize Size { get; set; }

        /// <summary>
        /// Gets or sets channel settings.
        /// </summary>
        ChannelSinkSettings Settings { get; set; }

        /// <summary>
        /// Deletes the channel.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the channel asynchronously.
        /// </summary>
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
        /// Collection of programs associated with the channel.
        /// </summary>
        ProgramBaseCollection Programs { get; }

        /// <summary>
        /// Starts the channel.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the channel asynchronously.
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
        /// Stops the channel.
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the channel asynchronously.
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
        /// Updates this channel instance asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Updates this channel instance.
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
    }
}
