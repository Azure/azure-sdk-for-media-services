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
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <returns>The created streaming endpoint.</returns>
        public IStreamingEndpoint Create(string name)
        {
            return Create(name, null, null, null, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <returns>The created streaming endpoint.</returns>
        public Task<IStreamingEndpoint> CreateAsync(string name)
        {
            return CreateAsync(name, null, null, null, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="scaleUnits">scale units.</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public IStreamingEndpoint Create(string name, int scaleUnits)
        {
            return Create(name, null, null, scaleUnits, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="scaleUnits">scale units.</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public Task<IStreamingEndpoint> CreateAsync(string name, int scaleUnits)
        {
            return CreateAsync(name, null, null, scaleUnits, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="scaleUnits">scale units.</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public IStreamingEndpoint Create(string name, string description, int scaleUnits)
        {
            return Create(name, description, null, scaleUnits, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="scaleUnits">scale units.</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public Task<IStreamingEndpoint> CreateAsync(string name, string description, int scaleUnits)
        {
            return CreateAsync(name, description, null, scaleUnits, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <returns>The created streaming endpoint (for standard account using the SDK).</returns>
        public IStreamingEndpoint Create(string name, string description, List<string> customHostNames)
        {
            return Create(name, description, customHostNames, null, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <returns>The created streaming endpoint (for standard account using the SDK).</returns>
        public Task<IStreamingEndpoint> CreateAsync(string name, string description, List<string> customHostNames)
        {
            return CreateAsync(name, description, customHostNames, null, true, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <param name="scaleUnits">scale units</param>
        /// <param name="cdnEnabled">whether the streaming endpoint integrates CDN</param>
        /// <returns>The created streaming endpoint (for premium account using the SDK).</returns>
        public IStreamingEndpoint Create(
            string name, 
            string description, 
            List<string> customHostNames, 
            int scaleUnits,
            bool cdnEnabled)
        {
            return Create(name, description, customHostNames, scaleUnits, cdnEnabled, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint asynchronously.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <param name="scaleUnits">scale units</param>
        /// <param name="cdnEnabled">whether the streaming endpoint integrates CDN</param>
        /// <returns>The created streaming endpoint (for premium account using the SDK).</returns>
        public Task<IStreamingEndpoint> CreateAsync(
            string name,
            string description,
            List<string> customHostNames,
            int scaleUnits,
            bool cdnEnabled)
        {
            return CreateAsync(name, description, customHostNames, scaleUnits, cdnEnabled, null, null, null);
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">description of the streaming endpoint</param>
        /// <param name="customHostNames">customer host names</param>
        /// <param name="scaleUnits">the scale units, can only be set for legacy and premium accounts</param>
        /// <param name="cdnEnabled">if the streaming endpoint has integrated CDN, can only be set for premium accounts</param>
        /// <param name="accessPolicies">cross site access policies</param>
        /// <param name="accessControl">access control</param>
        /// <param name="cacheControl">cache control</param>
        /// <returns>The created streaming endpoint.</returns>
        public IStreamingEndpoint Create(
            string name,
            string description,
            List<string> customHostNames,
            int? scaleUnits,
            bool cdnEnabled,
            CrossSiteAccessPolicies accessPolicies,
            StreamingEndpointAccessControl accessControl,
            StreamingEndpointCacheControl cacheControl)
        {
            return AsyncHelper.Wait(CreateAsync(
                name,
                description,
                customHostNames,
                scaleUnits,
                cdnEnabled,
                accessPolicies,
                accessControl,
                cacheControl));
        }

        /// <summary>
        /// Creates a new streaming endpoint.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">description of the streaming endpoint</param>
        /// <param name="customHostNames">customer host names</param>
        /// <param name="scaleUnits">the scale units, can only be set for legacy and premium accounts</param>
        /// <param name="cdnEnabled">if the streaming endpoint has integrated CDN, can only be set for premium accounts</param>
        /// <param name="accessPolicies">cross site access policies</param>
        /// <param name="accessControl">access control</param>
        /// <param name="cacheControl">cache control</param>
        /// <returns>The created streaming endpoint.</returns>
        public Task<IStreamingEndpoint> CreateAsync(
            string name,
            string description,
            List<string> customHostNames,
            int? scaleUnits,
            bool cdnEnabled,
            CrossSiteAccessPolicies accessPolicies,
            StreamingEndpointAccessControl accessControl,
            StreamingEndpointCacheControl cacheControl)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyStreamingEndpointName);
            }
            if (accessControl == null)
            {
                accessControl = new StreamingEndpointAccessControl();
            }
            if (accessControl.AkamaiSignatureHeaderAuthenticationKeyList == null)
            {
                accessControl.AkamaiSignatureHeaderAuthenticationKeyList = new List<AkamaiSignatureHeaderAuthenticationKey>();
            }
            if (accessControl.IPAllowList == null)
            {
                accessControl.IPAllowList = new List<IPAddress>();
            }

            var streamingEndpoint = new StreamingEndpointData
            {
                Name = name,
                Description = description,
                CustomHostNames = customHostNames ?? new List<string>(),
                ScaleUnits = scaleUnits,
                CdnEnabled = cdnEnabled,
                CrossSiteAccessPolicies = accessPolicies
            };

            ((IStreamingEndpoint) streamingEndpoint).AccessControl = accessControl;
            ((IStreamingEndpoint) streamingEndpoint).CacheControl = cacheControl;

            streamingEndpoint.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(StreamingEndpointSet, streamingEndpoint);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(streamingEndpoint))
                .ContinueWith<IStreamingEndpoint>(t =>
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
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <returns>The created streaming endpoint.</returns>
        public IOperation SendCreateOperation(string name)
        {
            return SendCreateOperation(name, null, null, null, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <returns>The created streaming endpoint.</returns>
        public Task<IOperation> SendCreateOperationAync(string name)
        {
            return SendCreateOperationAsync(name, null, null, null, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="scaleUnit">Scale units</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public IOperation SendCreateOperation(string name, int scaleUnit)
        {
            return SendCreateOperation(name, null, null, scaleUnit, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="scaleUnit">Scale units</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public Task<IOperation> SendCreateOperationAync(string name, int scaleUnit)
        {
            return SendCreateOperationAsync(name, null, null, scaleUnit, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="scaleUnit">Scale units</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public IOperation SendCreateOperation(string name, string description, int scaleUnit)
        {
            return SendCreateOperation(name, description, null, scaleUnit, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="scaleUnit">Scale units</param>
        /// <returns>The created streaming endpoint (for legacy account using the SDK).</returns>
        public Task<IOperation> SendCreateOperationAync(string name, string description, int scaleUnit)
        {
            return SendCreateOperationAsync(name, description, null, scaleUnit, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <returns>The created streaming endpoint (for standard account using the SDK).</returns>
        public IOperation SendCreateOperation(string name, string description, List<string> customHostNames)
        {
            return SendCreateOperation(name, description, customHostNames, null, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <returns>The created streaming endpoint (for standard account using the SDK).</returns>
        public Task<IOperation> SendCreateOperationAync(string name, string description, List<string> customHostNames)
        {
            return SendCreateOperationAsync(name, description, customHostNames, null, true, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <param name="scaleUnits">scale units</param>
        /// <param name="cdnEnabled">whether the streaming endpoint integrates CDN</param>
        /// <returns>The created streaming endpoint (for premium account using the SDK).</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            List<string> customHostNames,
            int scaleUnits,
            bool cdnEnabled)
        {
            return SendCreateOperation(name, description, customHostNames, scaleUnits, cdnEnabled, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">Description of the streaming endpoint.</param>
        /// <param name="customHostNames">a list of custom host names.</param>
        /// <param name="scaleUnits">scale units</param>
        /// <param name="cdnEnabled">whether the streaming endpoint integrates CDN</param>
        /// <returns>The created streaming endpoint (for premium account using the SDK).</returns>
        public Task<IOperation> SendCreateOperationAync(
            string name,
            string description,
            List<string> customHostNames,
            int scaleUnits,
            bool cdnEnabled)
        {
            return SendCreateOperationAsync(name, description, customHostNames, scaleUnits, cdnEnabled, null, null, null);
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">description of the streaming endpoint</param>
        /// <param name="customHostNames">customer host names</param>
        /// <param name="scaleUnits">the scale units, can only be set for legacy and premium accounts</param>
        /// <param name="cdnEnabled">if the streaming endpoint has integrated CDN, can only be set for premium accounts</param>
        /// <param name="accessPolicies">cross site access policies</param>
        /// <param name="accessControl">access control</param>
        /// <param name="cacheControl">cache control</param>
        /// <returns>The created streaming endpoint.</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            List<string> customHostNames,
            int? scaleUnits,
            bool cdnEnabled,
            CrossSiteAccessPolicies accessPolicies,
            StreamingEndpointAccessControl accessControl,
            StreamingEndpointCacheControl cacheControl)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyStreamingEndpointName);
            }

            var streamingEndpoint = new StreamingEndpointData
            {
                Name = name,
                Description = description,
                CustomHostNames = customHostNames ?? new List<string>(),
                ScaleUnits = scaleUnits,
                CdnEnabled = cdnEnabled,
                CrossSiteAccessPolicies = accessPolicies
            };

            ((IStreamingEndpoint)streamingEndpoint).AccessControl = accessControl;
            ((IStreamingEndpoint)streamingEndpoint).CacheControl = cacheControl;
            
            streamingEndpoint.SetMediaContext(MediaContext);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(StreamingEndpointSet, streamingEndpoint);

            MediaRetryPolicy retryPolicy = MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            var response = retryPolicy.ExecuteAction(() => dataContext.SaveChanges());

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            IOperation result = new OperationData
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
        }

        /// <summary>
        /// Sends create streaming endpoint operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the streaming endpoint.</param>
        /// <param name="description">description of the streaming endpoint</param>
        /// <param name="customHostNames">customer host names</param>
        /// <param name="scaleUnits">the scale units, can only be set for legacy and premium accounts</param>
        /// <param name="cdnEnabled">if the streaming endpoint has integrated CDN, can only be set for premium accounts</param>
        /// <param name="accessPolicies">cross site access policies</param>
        /// <param name="accessControl">access control</param>
        /// <param name="cacheControl">cache control</param>
        /// <returns>The created streaming endpoint.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            string description,
            List<string> customHostNames,
            int? scaleUnits,
            bool cdnEnabled,
            CrossSiteAccessPolicies accessPolicies,
            StreamingEndpointAccessControl accessControl,
            StreamingEndpointCacheControl cacheControl)
        {
            return Task.Factory.StartNew(() =>
                SendCreateOperation(
                    name,
                    description,
                    customHostNames,
                    scaleUnits,
                    cdnEnabled,
                    accessPolicies,
                    accessControl,
                    cacheControl));
        }
    }
}
