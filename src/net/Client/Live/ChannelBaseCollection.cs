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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IChannel"/>.
    /// </summary>
    public sealed class ChannelBaseCollection : CloudBaseCollection<IChannel>
    {
        internal const string ChannelSet = "Channels";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="MediaContextBase"/> instance.</param>
        internal ChannelBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
			Queryable = MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IChannel, ChannelData>(ChannelSet);
        }

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>The created channel.</returns>
        public IChannel Create(
            string name,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return Create(name, null, input, preview, output);
        }

        /// <summary>
        /// Asynchronously create a new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>The created channel.</returns>
        public Task<IChannel> CreateAsync(
            string name,  
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return CreateAsync(name, null, input, preview, output);
        }

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel or friendly name.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>The created channel.</returns>
        public IChannel Create(
            string name, 
            string description, 
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return AsyncHelper.Wait(CreateAsync(name, description, input, preview, output));
        }

        /// <summary>
        /// Asynchronously create a new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel or friendly name.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>The created channel.</returns>
        public Task<IChannel> CreateAsync(
            string name, 
            string description, 
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            var response = CreateChannelAsync(name, description, input, preview, output);

            return response.ContinueWith<IChannel>(t =>
                {
                    t.ThrowIfFaulted();
                    string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    IOperation operation = AsyncHelper.WaitOperationCompletion(
                        MediaContext,
                        operationId,
                        StreamingConstants.CreateChannelPollInterval);

                    string messageFormat = Resources.ErrorCreateChannelFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            var result = (ChannelData)t.Result.AsyncState;
                            result.Refresh();
                            return result;
                        case OperationState.Failed:
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed, operationId, operation.ErrorMessage);
                            throw new InvalidOperationException(message);
                        default: // can never happen unless state enum is extended
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState, operationId, operation.State);
                            throw new InvalidOperationException(message);
                    }
                });
        }

        #region Non-polling asyncs

        /// <summary>
        /// Sends create channel operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return SendCreateOperation(name, null, input, preview, output);
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return SendCreateOperationAsync(name, null, input, preview, output);
        }

        /// <summary>
        /// Sends create channel operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel or friendly name.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            return AsyncHelper.Wait(SendCreateOperationAsync(name, description, input, preview, output));
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel or friendly name.</param>
        /// <param name="input">The channel input endpoint properties.</param>
        /// <param name="preview">The channel preview endpoint properties.</param>
        /// <param name="output">The channel output endpoint properties.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            string description,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            var response = CreateChannelAsync(name, description, input, preview, output);

            return response.ContinueWith(t =>
            {
                t.ThrowIfFaulted();

                string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                IOperation result = new OperationData
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    Id = operationId,
                    State = OperationState.InProgress.ToString(),
                };

                return result;
            });
        }

        #endregion Non-polling asyncs

        private Task<IMediaDataServiceResponse> CreateChannelAsync(
            string name,
            string description,
            ChannelInput input,
            ChannelPreview preview,
            ChannelOutput output)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyChannelName);
            }

            var channelData = new ChannelData
            {
                Name = name,
                Description = description
            };

            IChannel channel = channelData;

            channel.Input = input;
            channel.Preview = preview;
            channel.Output = output;

            if (channel.Input == null)
            {
                channel.Input = new ChannelInput();
            }
            if (channel.Input.Endpoints == null)
            {
                channel.Input.Endpoints = new List<ChannelEndpoint>().AsReadOnly();
            }
            if (channel.Input.AccessControl == null)
            {
                channel.Input.AccessControl = new ChannelAccessControl();
            }
            if (channel.Input.AccessControl.IPAllowList == null)
            {
                channel.Input.AccessControl.IPAllowList = new List<IPAddress>();
            }

            if (channel.Preview == null)
            {
                channel.Preview = new ChannelPreview();
            }
            if (channel.Preview.Endpoints == null)
            {
                channel.Preview.Endpoints = new List<ChannelEndpoint>().AsReadOnly();
            }
            if (channel.Preview.AccessControl == null)
            {
                channel.Preview.AccessControl = new ChannelAccessControl();
            }
            if (channel.Preview.AccessControl.IPAllowList == null)
            {
                channel.Preview.AccessControl.IPAllowList = new List<IPAddress>();
            }

            channelData.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(ChannelSet, channel);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(channel));
        }
    }
}
