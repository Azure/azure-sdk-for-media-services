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
    /// Describes a program.
    /// </summary>
    public interface IProgram
    {
        /// <summary>
        /// Gets Unique identifier of the program.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets name of the program.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets description of the program.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets program creation date.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Gets program last modification date.
        /// </summary>
        DateTime LastModified { get; }

        /// <summary>
        /// Gets id of the channel containing the program.
        /// </summary>
        string ChannelId { get; }

        /// <summary>
        /// Gets or sets id of the asset for storing channel content.
        /// </summary>
        string AssetId { get; set; }

        /// <summary>
        /// Gets or sets the streaming manifest name. 
        /// </summary>
        string ManifestName { get; set; }

        /// <summary>
        /// Gets or sets the length of the DVR window.
        /// </summary>
        TimeSpan? DvrWindowLength { get; set; }

        /// <summary>
        /// Gets or sets the estimated length of the program duration.
        /// </summary>
        TimeSpan EstimatedDuration { get; set; }

        /// <summary>
        /// Enables or disables archiving.
        /// </summary>
        bool EnableArchive { get; set; }

        /// <summary>
        /// Gets program state.
        /// </summary>
        ProgramState State { get; }

        /// <summary>
        /// Deletes the program.
        /// </summary>
        void Delete();

        /// <summary>
        /// Deletes the program asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Starts the program.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts the program asynchronously.
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
        /// Stops the program.
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops the program asynchronously.
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
        /// Gets the channel associated with the program.
        /// </summary>
        IChannel Channel { get; }

        /// <summary>
        /// Updates this program instance asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task UpdateAsync();

        /// <summary>
        /// Updates this program instance.
        /// </summary>        
        void Update();
    }
}
