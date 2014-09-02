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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IStreamingEndpoint"/>.
    /// </summary>
    public class StreamingEndpointBaseCollection : CloudBaseCollection<IStreamingEndpoint>
    {
        internal const string StreamingEndpointSet = "StreamingEndpoints";

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingEndpointBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        internal StreamingEndpointBaseCollection(CloudMediaContext cloudMediaContext)
            : base(cloudMediaContext)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            Queryable = MediaContext
                .MediaServicesClassFactory
                .CreateDataServiceContext()
                .CreateQuery<IStreamingEndpoint, StreamingEndpointData>(StreamingEndpointSet);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">streaming endpoint service name.</param>
        /// <param name="scaleUnits">the streaming endpoint scale units, can only be set for legacy and premium accounts</param>
        /// <returns>The created streaming endpoint.</returns>
        public IStreamingEndpoint Create(string name, int scaleUnits)
        {
            return Create(new StreamingEndpointCreationOptions(name, scaleUnits));
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">streaming endpoint service name.</param>
        /// <param name="scaleUnits">the streaming endpoint scale units, can only be set for legacy and premium accounts</param>
        /// <returns>The streaming endpoint creation task.</returns>
        public Task<IStreamingEndpoint> CreateAsync(string name, int scaleUnits)
        {
            return CreateAsync(new StreamingEndpointCreationOptions(name, scaleUnits));
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="options">Streaming endpoint creation options</param>
        /// <returns>The created streaming endpoint.</returns>
        public IStreamingEndpoint Create(StreamingEndpointCreationOptions options)
        {
            return AsyncHelper.Wait(CreateAsync(options));
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="options">Streaming endpoint creation options</param>
        /// <returns>The streaming endpoint creation task.</returns>
        public Task<IStreamingEndpoint> CreateAsync(StreamingEndpointCreationOptions options)
        {
            var response = CreateStreamingEndpointAsync(options);

            return response.ContinueWith<IStreamingEndpoint>(t =>
                {
                    t.ThrowIfFaulted();

                    string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    IOperation operation = AsyncHelper.WaitOperationCompletion(
                        MediaContext,
                        operationId,
                        StreamingConstants.CreateStreamingEndpointPollInterval);

                    string messageFormat = Resources.ErrorCreateStreamingEndpointFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            var result = (StreamingEndpointData)t.Result.AsyncState;
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


        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">streaming endpoint service name.</param>
        /// <param name="scaleUnits">the streaming endpoint scale units, can only be set for legacy and premium accounts</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(string name, int scaleUnits)
        {
            return SendCreateOperation(new StreamingEndpointCreationOptions(name, scaleUnits));
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">streaming endpoint service name.</param>
        /// <param name="scaleUnits">the streaming endpoint scale units, can only be set for legacy and premium accounts</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAync(string name, int scaleUnits)
        {
            return SendCreateOperationAsync(new StreamingEndpointCreationOptions(name, scaleUnits));
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="options">Streaming endpoint creation options</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(StreamingEndpointCreationOptions options)
        {
            return AsyncHelper.Wait(SendCreateOperationAsync(options));
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="options">Streaming endpoint creation options</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(StreamingEndpointCreationOptions options)
        {
            var response = CreateStreamingEndpointAsync(options);

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

        private Task<IMediaDataServiceResponse> CreateStreamingEndpointAsync(StreamingEndpointCreationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                throw new ArgumentException(Resources.ErrorEmptyStreamingEndpointName);
            }

            if (options.CustomHostNames == null)
            {
                options.CustomHostNames = new List<string>();
            }

            var streamingEndpoint = new StreamingEndpointData
            {
                Name = options.Name,
                Description = options.Description,
                CustomHostNames = (options.CustomHostNames as IList<string>) ?? options.CustomHostNames.ToList(),
                ScaleUnits = options.ScaleUnits,
                CrossSiteAccessPolicies = options.CrossSiteAccessPolicies
            };

            ((IStreamingEndpoint) streamingEndpoint).AccessControl = options.AccessControl;
            ((IStreamingEndpoint) streamingEndpoint).CacheControl = options.CacheControl;

            streamingEndpoint.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(StreamingEndpointSet, streamingEndpoint);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(streamingEndpoint));
        }
    }
}
