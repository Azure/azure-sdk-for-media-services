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
        /// <param name="inputProtocol">Channel input streaming protocol</param>
        /// <param name="inputIPAllowList">Channel input IP allow list</param>
        /// <returns>The created channel.</returns>
        public IChannel Create(string name, StreamingProtocol inputProtocol, IEnumerable<IPRange> inputIPAllowList)
        {
            return Create(new ChannelCreationOptions(name, inputProtocol, inputIPAllowList));
        }

        /// <summary>
        /// Asynchronously create a new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="inputProtocol">Channel input streaming protocol</param>
        /// <param name="inputIPAllowList">Channel input IP allow list</param>
        /// <returns>The channel creation task.</returns>
        public Task<IChannel> CreateAsync(string name, StreamingProtocol inputProtocol, IEnumerable<IPRange> inputIPAllowList)
        {
            return CreateAsync(new ChannelCreationOptions(name, inputProtocol, inputIPAllowList));
        }

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="options"> Channel creation options </param>
        /// <returns>The created channel.</returns>
        public IChannel Create(ChannelCreationOptions options)
        {
            return AsyncHelper.Wait(CreateAsync(options));
        }

        /// <summary>
        /// Asynchronously create a new channel.
        /// </summary>
        /// <param name="options"> Channel creation options </param>
        /// <returns>The channel creation task.</returns>
        public Task<IChannel> CreateAsync(ChannelCreationOptions options)
        {
            var response = CreateChannelAsync(options);

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
        /// <param name="inputProtocol">Channel input streaming protocol</param>
        /// <param name="inputIPAllowList">Channel input IP allow list</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(string name, StreamingProtocol inputProtocol, IEnumerable<IPRange> inputIPAllowList)
        {
            return SendCreateOperation(new ChannelCreationOptions(name, inputProtocol, inputIPAllowList));
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="inputProtocol">Channel input streaming protocol</param>
        /// <param name="inputIPAllowList">Channel input IP allow list</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(string name, StreamingProtocol inputProtocol, IEnumerable<IPRange> inputIPAllowList)
        {
            return SendCreateOperationAsync(new ChannelCreationOptions(name, inputProtocol, inputIPAllowList));
        }

        /// <summary>
        /// Sends create channel operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="options"> Channel creation options </param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(ChannelCreationOptions options)
        {
            return AsyncHelper.Wait(SendCreateOperationAsync(options));
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="options"> Channel creation options </param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(ChannelCreationOptions options)
        {
            var response = CreateChannelAsync(options);

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

        private Task<IMediaDataServiceResponse> CreateChannelAsync(ChannelCreationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                throw new ArgumentException(Resources.ErrorEmptyChannelName);
            }

            if (options.Input == null ||
                options.Input.AccessControl == null ||
                options.Input.AccessControl.IPAllowList == null)
            {
                throw new ArgumentException(Resources.ErrorEmptyChannelInputIPAllowList);
            }

            var channelData = new ChannelData
            {
                Name = options.Name,
                Description = options.Description,
                CrossSiteAccessPolicies = options.CrossSiteAccessPolicies,
                Slate = options.Slate,
            };

            // setting the state of the channel
            ChannelState channelState = options.State == ChannelState.Running
                ? options.State
                : ChannelState.Stopped;

            channelData.State = channelState.ToString();

            //setting vanityUrl flag
            channelData.VanityUrl = options.VanityUrl;

            IChannel channel = channelData;

            channel.Input = options.Input;
            channel.Preview = options.Preview;
            channel.Output = options.Output;
            channel.EncodingType = options.EncodingType;
            channel.Encoding = options.Encoding;
            
            channelData.ValidateSettings();

            channelData.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(ChannelSet, channel);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(channel));
        }
    }
}
