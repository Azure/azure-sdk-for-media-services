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
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using System.Net;
using Microsoft.WindowsAzure.MediaServices.Client.Rest;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a Channel and executes actions on it.
    /// </summary>
    [DataServiceKey("Id")]
    internal class ChannelData : RestEntity<ChannelData>, IChannel, ICloudMediaContextInit
    {
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets channel last modification date.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets Url of the preview.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string PreviewUrl { get; set; }

        /// <summary>
        /// Gets or sets ingest Url.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string IngestUrl { get; set; }

        /// <summary>
        /// Gets or sets state of the channel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets size of the channel.
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets channel settings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Settings 
        {
            get
            {
                return Serializer.Serialize(new ChannelServiceSettings(_settings));
            }
            set
            {
                _settings = (ChannelSettings)Serializer.Deserialize<ChannelServiceSettings>(value);
            }
        }

        #region ICloudMediaContextInit Members
        /// <summary>
        /// Initializes the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            InvalidateCollections();
            this._cloudMediaContext = (CloudMediaContext)context;
        }

        #endregion

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
        /// Gets size of the channel.
        /// </summary>
        ChannelSize IChannel.Size
        { 
            get 
            {
                return (ChannelSize)Enum.Parse(typeof(ChannelSize), Size, true);
            }

            set
            {
                Size = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets channel settings.
        /// </summary>
        ChannelSettings IChannel.Settings
        {
            get
            {
                return _settings;
            }

            set
            {
                _settings = value;
            }
        }

        /// <summary>
        /// Collection of programs associated with the channel.
        /// </summary>
        ProgramBaseCollection IChannel.Programs
        {
            get
            {
                if (_programCollection == null && _cloudMediaContext != null)
                {
                    this._programCollection = new ProgramBaseCollection(_cloudMediaContext, this);
                }

                return _programCollection;
            }
        }

        /// <summary>
        /// Adds or removes channel metrics recevied event handler
        /// </summary>
        event EventHandler<MetricsEventArgs<IChannelMetric>> IChannel.MetricsReceived
        {
            add
            {
                _cloudMediaContext.ChannelMetrics.Monitor.Subscribe(Id, value);
            }
            remove
            {
                _cloudMediaContext.ChannelMetrics.Monitor.Unsubscribe(Id, value);
            }
        }

        /// <summary>
        /// Gets Url of the preview.
        /// </summary>
        Uri IChannel.PreviewUrl
        {
            get
            {
                return new Uri(PreviewUrl);
            }
        }

        /// <summary>
        /// Gets ingest Url.
        /// </summary>
        Uri IChannel.IngestUrl
        {
            get
            {
                return new Uri(IngestUrl);
            }
        }

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Start", this.Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends start channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Start", this.Id), UriKind.Relative);

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Reset", this.Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends reset channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendResetOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Reset", this.Id), UriKind.Relative);

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Stop", this.Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StopChannelPollInterval);
        }

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStopOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Stop", this.Id), UriKind.Relative);

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
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this).ContinueWith(t =>
            {
                t.ThrowIfFaulted();

                string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                IOperation operation = AsyncHelper.WaitOperationCompletion(
                    this._cloudMediaContext,
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
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            var response = dataContext.SaveChanges();

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            var result = new OperationData()
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
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
        /// Get the latest channel metric.
        /// </summary>
        /// <returns>The latest ChannelMetrics entity of this channel service</returns>
        public IChannelMetric GetMetric()
        {
            var uri = new Uri(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "/{0}('{1}')/{2}",
                    ChannelBaseCollection.ChannelSet,
                    Id,
                    Metric.MetricProperty
                    ),
                UriKind.Relative);

            var dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var metric = dataContext.Execute<ChannelMetricData>(uri).SingleOrDefault();

            return metric;
        }

        protected override string EntitySetName { get { return ChannelBaseCollection.ChannelSet; } }

        /// <summary>
        /// Invalidates collections to force them to be reloaded from server.
        /// </summary>
        private void InvalidateCollections()
        {
            this._programCollection = null;
        }

        private ProgramBaseCollection _programCollection;

        private ChannelSettings _settings;
    }
}
