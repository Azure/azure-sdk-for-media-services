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
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;

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
        /// Gets state of the channel.
        /// </summary>
        ChannelState State { get; }

        /// <summary>
        /// Gets or sets the cross site access policies for the channel.
        /// </summary>
        CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        ChannelInput Input { get; set; }

        /// <summary>
        /// Gets or sets the channel preview properties.
        /// </summary>
        ChannelPreview Preview { get; set; }

        /// <summary>
        /// Gets channel vanity url flag property.
        /// </summary>
        bool VanityUrl { get; }

        /// <summary>
        /// Gets or sets the channel output properties.
        /// </summary>
        ChannelOutput Output { get; set; }

        /// <summary>
        /// Gets or sets the channel Encoding properties.
        /// </summary>
        ChannelEncoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets the channel encoding type
        /// </summary>
        ChannelEncodingType EncodingType { get; set; }

        /// <summary>
        /// Gets or sets the channel slate
        /// </summary>
        ChannelSlate Slate { get; set; }

        /// <summary>
        /// Collection of programs associated with the channel.
        /// </summary>
        ProgramBaseCollection Programs { get; }

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
        /// Resets the channel.
        /// </summary>
        void Reset();

        /// <summary>
        /// Resets the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task ResetAsync();

        /// <summary>
        /// Sends reset operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendResetOperation();

        /// <summary>
        /// Sends reset operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendResetOperationAsync();

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

        /// <summary>
        /// Show a slate on the channel.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        void ShowSlate(TimeSpan duration, string assetId);

        /// <summary>
        /// Show a slate on the channel asynchronously.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        Task ShowSlateAsync(TimeSpan duration, string assetId);

        /// <summary>
        /// Sends show slate operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendShowSlateOperation(TimeSpan duration, string assetId);

        /// <summary>
        /// Sends show slate operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendShowSlateOperationAsync(TimeSpan duration, string assetId);

        /// <summary>
        /// Hide the currently running slate if any.
        /// </summary>
        void HideSlate();

        /// <summary>
        /// Hide the currently running slate if any asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task HideSlateAsync();

        /// <summary>
        /// Sends hide slate operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendHideSlateOperation();

        /// <summary>
        /// Sends hide slate operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendHideSlateOperationAsync();

        /// <summary>
        /// Start an Ad marker on the channel.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        void StartAdvertisement(TimeSpan duration, int cueId, bool showSlate = true);

        /// <summary>
        /// Start an Ad marker on the channel asynchronously.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show the slate for the duration of the ad.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        Task StartAdvertisementAsync(TimeSpan duration, int cueId, bool showSlate = true);

        /// <summary>
        /// Sends start advertisement operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of ad.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        IOperation SendStartAdvertisementOperation(TimeSpan duration, int cueId, bool showSlate = true);

        /// <summary>
        /// Sends start advertisement operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        Task<IOperation> SendStartAdvertisementOperationAsync(TimeSpan duration, int cueId, bool showSlate = true);

        /// <summary>
        /// Ends the ad marker on the channel.
        /// </summary>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        void EndAdvertisement(int cueId);

        /// <summary>
        /// Ends the ad marker on the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        Task EndAdvertisementAsync(int cueId);

        /// <summary>
        /// Sends end advertisement operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        IOperation SendEndAdvertisementOperation(int cueId);

        /// <summary>
        /// Sends end advertisement operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        Task<IOperation> SendEndAdvertisementOperationAsync(int cueId);

        /// <summary>
        /// Returns an object that can be queried to get channel monitoring data.
        /// </summary>
        /// <returns>Returns instance of <see cref="ChannelTelemetryDataProvider"/> </returns>
        ChannelTelemetryDataProvider GetTelemetry();
    }
}
