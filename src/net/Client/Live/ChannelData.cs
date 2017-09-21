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
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a Channel and executes actions on it.
    /// </summary>
    [DataServiceKey("Id")]
    internal class ChannelData : RestEntity<ChannelData>, IChannel
    {
        private ChannelInput _input;
        private ChannelPreview _preview;

        private ProgramBaseCollection _programCollection;

        private ChannelEncoding _encoding;

        private bool _vanityUrl;

        protected override string EntitySetName { get { return ChannelBaseCollection.ChannelSet; } }

        /// <summary>
        /// Gets or sets the name of the channel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the channel.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets channel creation date.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets channel last modification date.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets state of the channel.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string State { get; set; }

        /// <summary>
        /// Gets state of the channel.
        /// </summary>
        ChannelState IChannel.State 
        {
            get 
            {
                return (ChannelState)Enum.Parse(typeof(ChannelState), State, true);
            } 
        }

        /// <summary>
        /// Gets or sets the channel encoding type
        /// </summary>
        public string EncodingType { get; set; }

        /// <summary>
        /// Gets or sets the channel encoding type
        /// </summary>
        ChannelEncodingType IChannel.EncodingType
        {
            get { return (ChannelEncodingType)Enum.Parse(typeof(ChannelEncodingType), EncodingType, true); }
            set { EncodingType = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the cross site access policies for the channel.
        /// </summary>
        public CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        public ChannelInputData Input
        {
            get { return _input == null ? null : new ChannelInputData(_input); }
            set { _input = (ChannelInput) value; }
        }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        ChannelInput IChannel.Input
        {
            get { return _input; }
            set { _input = value; }
        }

        /// <summary>
        /// Gets or sets the channel preview properties.
        /// </summary>
        public ChannelPreviewData Preview
        {
            get { return _preview == null ? null : new ChannelPreviewData(_preview);}
            set { _preview = (ChannelPreview) value; }
        }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        ChannelPreview IChannel.Preview
        {
            get { return _preview; }
            set { _preview = value; }
        }

        /// <summary>
        /// Gets or sets the channel output properties.
        /// </summary>
        public ChannelOutput Output { get; set; }

        /// <summary>
        /// Collection of programs associated with the channel.
        /// </summary>
        ProgramBaseCollection IChannel.Programs
        {
            get
            {
                if (_programCollection == null && GetMediaContext() != null)
                {
                    _programCollection = new ProgramBaseCollection(GetMediaContext(), this);
                }

                return _programCollection;
            }
        }

        /// <summary>
        /// Gets or sets the channel Encoding properties.
        /// </summary>
        public ChannelEncodingData Encoding
        {
            get { return _encoding == null ? null : new ChannelEncodingData(_encoding); }
            set { _encoding = (ChannelEncoding)value; }
        }

        /// <summary>
        /// Gets or sets the channel Encoding properties.
        /// </summary>
        ChannelEncoding IChannel.Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        /// <summary>
        /// Gets or sets channel vanity url flag property.
        /// </summary>
        public bool VanityUrl
        {
            get { return _vanityUrl; }
            set { _vanityUrl = value; }
        }

        /// <summary>
        /// Gets or sets the channel vanityUrl property.
        /// </summary>
        bool IChannel.VanityUrl
        {
            get { return _vanityUrl; }
        }

        /// <summary>
        /// Gets or sets the channel slate
        /// </summary>
        public ChannelSlate Slate { get; set; }

        #region IChannel Methods
        /// <summary>
        /// Starts the channel.
        /// </summary>
        public void Start()
        {
            AsyncHelper.Wait(StartAsync());
        }

        /// <summary>
        /// Starts the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StartAsync()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStartUriFormat, Id), 
                UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends start channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartOperation()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStartUriFormat, Id), 
                UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends start operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendStartOperationAsync()
        {
            return Task.Factory.StartNew(() => SendStartOperation());
        }

        /// <summary>
        /// Resets the channel.
        /// </summary>
        public void Reset()
        {
            AsyncHelper.Wait(ResetAsync());
        }

        /// <summary>
        /// Resets the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task ResetAsync()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelResetUriFormat, Id), 
                UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends reset channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendResetOperation()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelResetUriFormat, Id), 
                UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends reset operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendResetOperationAsync()
        {
            return Task.Factory.StartNew(() => SendResetOperation());
        }

        /// <summary>
        /// Stops the channel.
        /// </summary>
        public void Stop()
        {
            AsyncHelper.Wait(StopAsync());
        }

        /// <summary>
        /// Stops the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StopAsync()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStopUriFormat, Id), 
                UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StopChannelPollInterval);
        }

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStopOperation()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStopUriFormat, Id), 
                UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends stop operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendStopOperationAsync()
        {
            return Task.Factory.StartNew(() => SendStopOperation());
        }
        #endregion

        /// <summary>
        /// Deletes the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public override Task DeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(this))
                .ContinueWith(t =>
                {
                    t.ThrowIfFaulted();

                    string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    IOperation operation = AsyncHelper.WaitOperationCompletion(
                        GetMediaContext(),
                        operationId,
                        StreamingConstants.DeleteChannelPollInterval);

                    string messageFormat = Resources.ErrorDeleteChannelFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            return;
                        case OperationState.Failed:
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed, operationId, operation.ErrorMessage);
                            throw new InvalidOperationException(message);
                        default: // can never happen unless state enum is extended
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState, operationId, operation.State);
                            throw new InvalidOperationException(message);
                    }
                });

        }

        /// <summary>
        /// Sends delete operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendDeleteOperation()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            var response = retryPolicy.ExecuteAction(() => dataContext.SaveChanges());

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            var result = new OperationData
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
        }

        /// <summary>
        /// Returns an object that can be queried to get channel telemetry data.
        /// </summary>
        /// <returns>Returns instance of <see cref="ChannelTelemetryDataProvider"/> </returns>
        public ChannelTelemetryDataProvider GetTelemetry()
        {
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

            var monitoringConfig = GetMediaContext().MonitoringConfigurations.Single();
            var notificationEndpoint = GetMediaContext().NotificationEndPoints.Where(n => n.Id == monitoringConfig.NotificationEndPointId).Single();

            var channelId = new Guid(Id.Split(':').Last());

            var telemetryDataCache = new TelemetryDataCache((start, end) => notificationEndpoint.GetMonitoringSasUris(start, end));
            return new ChannelTelemetryDataProvider(
                channelId, 
                telemetryDataCache,
                new TelemetryStorage());
        }

        /// <summary>
        /// Sends delete operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendDeleteOperationAsync()
        {
            return Task.Factory.StartNew(() => SendDeleteOperation());
        }

        /// <summary>
        /// Show a slate on the channel.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        public void ShowSlate(TimeSpan duration, string assetId)
        {
            AsyncHelper.Wait(ShowSlateAsync(duration, assetId));
        }

        /// <summary>
        /// Show a slate on the channel asynchronously.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task ShowSlateAsync(TimeSpan duration, string assetId)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelShowSlateUriFormat, Id),
                UriKind.Relative);

            var durationParameter = new BodyOperationParameter(StreamingConstants.ShowSlateDurationParameter, duration);
            var assetIdParameter = new BodyOperationParameter(StreamingConstants.ShowSlateAssetIdParameter, assetId);

            return ExecuteActionAsync(uri, StreamingConstants.ShowSlatePollInterval, durationParameter, assetIdParameter);
        }

        /// <summary>
        /// Sends show slate operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendShowSlateOperation(TimeSpan duration, string assetId)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelShowSlateUriFormat, Id),
                UriKind.Relative);

            var durationParameter = new BodyOperationParameter(StreamingConstants.ShowSlateDurationParameter, duration);
            var assetIdParameter = new BodyOperationParameter(StreamingConstants.ShowSlateAssetIdParameter, assetId);

            return SendOperation(uri, durationParameter, assetIdParameter);
        }

        /// <summary>
        /// Sends show slate operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of time to display the slate</param>
        /// <param name="assetId">Optional asset id to be used for the slate.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendShowSlateOperationAsync(TimeSpan duration, string assetId)
        {
            return Task.Factory.StartNew(() => SendShowSlateOperation(duration, assetId));
        }

        /// <summary>
        /// Hide the currently running slate if any.
        /// </summary>
        public void HideSlate()
        {
            AsyncHelper.Wait(HideSlateAsync());
        }

        /// <summary>
        /// Hide the currently running slate if any asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task HideSlateAsync()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelHideSlateUriFormat, Id),
                UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.HideSlatePollInterval);
        }

        /// <summary>
        /// Sends hide slate operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendHideSlateOperation()
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelHideSlateUriFormat, Id),
                UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends hide slate operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendHideSlateOperationAsync()
        {
            return Task.Factory.StartNew(() => SendHideSlateOperation());
        }

        /// <summary>
        /// Start an Ad marker on the channel.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        public void StartAdvertisement(TimeSpan duration, int cueId, bool showSlate = true)
        {
            AsyncHelper.Wait(StartAdvertisementAsync(duration, cueId, showSlate));
        }

        /// <summary>
        /// Start an Ad marker on the channel asynchronously.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StartAdvertisementAsync(TimeSpan duration, int cueId, bool showSlate = true)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStartAdUriFormat, Id),
                UriKind.Relative);

            var durationParameter = new BodyOperationParameter(StreamingConstants.StartAdDurationParameter, duration);
            var cueIdParameter = new BodyOperationParameter(StreamingConstants.StartAdCueIdParameter, cueId);
            var showSlateParameter = new BodyOperationParameter(StreamingConstants.StartAdShowSlateParameter, showSlate);

            return ExecuteActionAsync(uri, StreamingConstants.StartAdvertisementPollInterval, durationParameter, cueIdParameter, showSlateParameter);
        }

        /// <summary>
        /// Sends start advertisement operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartAdvertisementOperation(TimeSpan duration, int cueId, bool showSlate = true)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelStartAdUriFormat, Id),
                UriKind.Relative);

            var durationParameter = new BodyOperationParameter(StreamingConstants.StartAdDurationParameter, duration);
            var cueIdParameter = new BodyOperationParameter(StreamingConstants.StartAdCueIdParameter, cueId);
            var showSlateParameter = new BodyOperationParameter(StreamingConstants.StartAdShowSlateParameter, showSlate);

            return SendOperation(uri, durationParameter, cueIdParameter, showSlateParameter);
        }

        /// <summary>
        /// Sends start advertisement operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="duration">The duration of the ad marker.</param>
        /// <param name="cueId">optional cue id to use for the ad marker.</param>
        /// <param name="showSlate">Indicates whether to show slate for the duration of the ad.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendStartAdvertisementOperationAsync(TimeSpan duration, int cueId, bool showSlate = true)
        {
            return Task.Factory.StartNew(() => SendStartAdvertisementOperation(duration, cueId, showSlate));
        }

        /// <summary>
        /// Ends the ad marker on the channel.
        /// </summary>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        public void EndAdvertisement(int cueId)
        {
            AsyncHelper.Wait(EndAdvertisementAsync(cueId));
        }

        /// <summary>
        /// Ends the ad marker on the channel asynchronously.
        /// </summary>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task EndAdvertisementAsync(int cueId)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelEndAdUriFormat, Id),
                UriKind.Relative);

            var cueIdParameter = new BodyOperationParameter(StreamingConstants.StartAdCueIdParameter, cueId);
            return ExecuteActionAsync(uri, StreamingConstants.EndAdvertisementPollInterval, cueIdParameter);
        }

        /// <summary>
        /// Sends end advertisement operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendEndAdvertisementOperation(int cueId)
        {
            var uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, StreamingConstants.ChannelEndAdUriFormat, Id),
                UriKind.Relative);
            var cueIdParameter = new BodyOperationParameter(StreamingConstants.StartAdCueIdParameter, cueId);

            return SendOperation(uri, cueIdParameter);
        }

        /// <summary>
        /// Sends end advertisement operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="cueId">The cue id of the ad marker to end.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendEndAdvertisementOperationAsync(int cueId)
        {
            return Task.Factory.StartNew(() => SendEndAdvertisementOperation(cueId));
        }
        
        public override void SetMediaContext(MediaContextBase value)
        {
            InvalidateCollections();
            base.SetMediaContext(value);
        }

        internal override void Refresh()
        {
            _input = null;
            _preview = null;
            _encoding = null;
            base.Refresh();
        }

        /// <summary>
        /// Set array property empty array if it is null because OData does not support 
        /// empty collection 
        /// </summary>
        internal override void ValidateSettings()
        {
            if (_input != null && _input.Endpoints == null)
            {
                _input.Endpoints = new List<ChannelEndpoint>().AsReadOnly();
            }

            if (_preview != null && _preview.Endpoints == null)
            {
                _preview.Endpoints = new List<ChannelEndpoint>().AsReadOnly();
            }

            if (_encoding != null)
            {
                if (_encoding.AudioStreams == null)
                {
                    _encoding.AudioStreams = new List<AudioStream>().AsReadOnly();
                }

                if (_encoding.VideoStreams == null)
                {
                    _encoding.VideoStreams = new List<VideoStream>().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Invalidates collections to force them to be reloaded from server.
        /// </summary>
        private void InvalidateCollections()
        {
            _programCollection = null;
        }
    }
}
