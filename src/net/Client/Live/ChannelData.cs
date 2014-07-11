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
using System.Data.Services.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a Channel and executes actions on it.
    /// </summary>
    [DataServiceKey("Id")]
    internal class ChannelData : RestEntity<ChannelData>, IChannel
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
        /// Gets the preview Url.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Uri PreviewUrl
        {
            get
            {
                IChannelPreview preview = Preview;
                if (preview == null || preview.Endpoints == null) return null;

                var endpoint = preview.Endpoints.FirstOrDefault();

                return endpoint == null ? null : endpoint.Url;
            }
        }

        /// <summary>
        /// Gets the ingest Url.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Uri IngestUrl
        {
            get
            {
                IChannelInput input = Input;
                if (input == null || input.Endpoints == null) return null;

                var endpoint = input.Endpoints.FirstOrDefault();

                return endpoint == null ? null : endpoint.Url;
            }
        }

        /// <summary>
        /// Gets or sets state of the channel.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
        /// Gets or sets the cross site access policies for the channel.
        /// </summary>
        public CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        public ChannelInput Input { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        IChannelInput IChannel.Input
        {
            get { return Input; }
            set { Input = new ChannelInput(value); }
        }

        /// <summary>
        /// Gets or sets the channel preview properties.
        /// </summary>
        public ChannelPreview Preview { get; set; }

        /// <summary>
        /// Gets or sets the channel input properties.
        /// </summary>
        IChannelPreview IChannel.Preview
        {
            get { return Preview; }
            set { Preview = new ChannelPreview(value); }
        }

        /// <summary>
        /// Gets or sets the channel output properties.
        /// </summary>
        public ChannelOutput Output { get; set; }

        /// <summary>
        /// Gets or sets the channel output properties.
        /// </summary>
        IChannelOutput IChannel.Output
        {
            get { return Output; }
            set { Output = new ChannelOutput(value); }
        }

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Start", Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends start channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Start", Id), UriKind.Relative);

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Reset", Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartChannelPollInterval);
        }

        /// <summary>
        /// Sends reset channel operation.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendResetOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Reset", Id), UriKind.Relative);

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
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Stop", Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StopChannelPollInterval);
        }

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStopOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')/Stop", Id), UriKind.Relative);

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

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

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

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

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
        /// Sends delete operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendDeleteOperationAsync()
        {
            return Task.Factory.StartNew(() => SendDeleteOperation());
        }
        
        public override void SetMediaContext(MediaContextBase value)
        {
            InvalidateCollections();
            base.SetMediaContext(value);
        }

        /// <summary>
        /// Invalidates collections to force them to be reloaded from server.
        /// </summary>
        private void InvalidateCollections()
        {
            _programCollection = null;
        }

        protected override string EntitySetName { get { return ChannelBaseCollection.ChannelSet; } }

        private ProgramBaseCollection _programCollection;
    }
}
