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
    public class ChannelBaseCollection : CloudBaseCollection<IChannel>
    {
        internal const string ChannelSet = "Channels";

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal ChannelBaseCollection(CloudMediaContext cloudMediaContext)
            : base(cloudMediaContext)
        {
            this.Queryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<ChannelData>(ChannelSet);
        }

        /// <summary>
        /// Creates new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>The created channel.</returns>
        public IChannel Create(string name, string description, ChannelSize size, ChannelSettings settings)
        {
            return AsyncHelper.Wait(CreateAsync(name, description, size, settings));
        }

        /// <summary>
        /// Creates new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>The created channel.</returns>
        public IChannel Create(string name, ChannelSize size, ChannelSettings settings)
        {
            return Create(name, null, size, settings);
        }

        /// <summary>
        /// Asynchronously creates new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>The created channel.</returns>
        public Task<IChannel> CreateAsync(string name, string description, ChannelSize size, ChannelSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyChannelName);
            }

            var channel = new ChannelData
            {
                Description = description,
                Name = name,
                Size = size.ToString(),
            };

            ((IChannel)channel).Settings = settings;

            channel.InitCloudMediaContext(this.MediaContext);

            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(ChannelSet, channel);


            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(channel))
                .ContinueWith<IChannel>(t =>
                {
                    t.ThrowIfFaulted();
                    string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    IOperation operation = AsyncHelper.WaitOperationCompletion(
                        this.MediaContext,
                        operationId,
                        StreamingConstants.CreateChannelPollInterval);

                    string messageFormat = Resources.ErrorCreateChannelFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            channel = (ChannelData)t.Result.AsyncState;
                            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Channels('{0}')", channel.Id), UriKind.Relative);
                            return (ChannelData)dataContext.Execute<ChannelData>(uri).SingleOrDefault();;
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
        /// Asynchronously creates new channel.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>The created channel.</returns>
        public Task<IChannel> CreateAsync(string name, ChannelSize size, ChannelSettings settings)
        {
            return CreateAsync(name, null, size, settings);
        }

        #region Non-polling asyncs

        /// <summary>
        /// Sends create channel operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            ChannelSize size,
            ChannelSettings settings)
        {
            return SendCreateOperation(name, null, size, settings);
        }

        /// <summary>
        /// Sends create channel operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            ChannelSize size, 
            ChannelSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyChannelName);
            }

            var channel = new ChannelData
            {
                Description = description,
                Name = name,
                Size = size.ToString(),
            };

            ((IChannel)channel).Settings = settings;

            channel.InitCloudMediaContext(this.MediaContext);

            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(ChannelSet, channel);
            var response = dataContext.SaveChanges();

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            IOperation result = new OperationData()
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            ChannelSize size,
            ChannelSettings settings)
        {
            return SendCreateOperationAsync(name, null, size, settings);
        }

        /// <summary>
        /// Sends create channel operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Unique name of the channel.</param>
        /// <param name="description">Description of the channel.</param>
        /// <param name="size">Size of the channel.</param>
        /// <param name="settings">Channel settings.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            string description,
            ChannelSize size,
            ChannelSettings settings)
        {
            return Task.Factory.StartNew(() => SendCreateOperation(name, description, size, settings));
        }

        #endregion Non-polling asyncs
    }
}
