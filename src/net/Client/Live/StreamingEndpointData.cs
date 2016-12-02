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
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class StreamingEndpointData : RestEntity<StreamingEndpointData>, IStreamingEndpoint
    {
        private StreamingEndpointAccessControl _accessControl;
        private StreamingEndpointCacheControl _cacheControl;
        private string _cdnProvider;

        protected override string EntitySetName { get { return StreamingEndpointBaseCollection.StreamingEndpointSet; } }

        /// <summary>
        /// Gets or sets name of the streaming endpoint.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the streaming endpoint.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets host name of the streaming endpoint.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets custom host names
        /// </summary>
        public IList<string> CustomHostNames { get; set; }

        /// <summary>
        /// Gets streaming endpoint creation date.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint last modification date.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the number of scale Units of the streaming endpoint.
        /// </summary>
        public int? ScaleUnits { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint state.
        /// </summary>
        public string State { get; set; }
        
        /// <summary>
        /// Gets or sets if CDN to be enabled on this Streaming Endpoint.
        /// </summary>
        public bool CdnEnabled { get; set; }

        /// <summary>
        /// Gets or sets Cdn provider
        /// </summary>
        public string CdnProvider
        {
            get
            {
                return _cdnProvider;
            }
            set
            {
                Live.CdnProviderType cdnProvider;
                if (value != null && !Enum.TryParse(value, true, out cdnProvider))
                {
                    throw new ArgumentException("Not a valid CDN provider");
                }

                _cdnProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets Cdn Profile
        /// </summary>
        public string CdnProfile { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint version.
        /// </summary>
        public string StreamingEndpointVersion { get; set; }

        /// <summary>
        /// Gets the free trial end time.
        /// </summary>
        public DateTime FreeTrialEndTime { get; }

        /// <summary>
        /// Gets streaming endpoint state.
        /// </summary>
        StreamingEndpointState IStreamingEndpoint.State
        {
            get { return (StreamingEndpointState) Enum.Parse(typeof (StreamingEndpointState), State, true); }
        }

        /// <summary>
        /// Gets or sets cross site access policies.
        /// </summary>
        public CrossSiteAccessPolicies CrossSiteAccessPolicies { get; set; }

        /// <summary>
        /// Gets or sets streaming endpoint access control
        /// </summary>
        public StreamingEndpointAccessControlData AccessControl
        {
            get { return _accessControl == null ? null : new StreamingEndpointAccessControlData(_accessControl); }
            set { _accessControl = (StreamingEndpointAccessControl) value; }
        }

        /// <summary>
        /// Gets or sets streaming endpoint access control
        /// </summary>
        StreamingEndpointAccessControl IStreamingEndpoint.AccessControl
        {
            get { return _accessControl; }
            set { _accessControl = value; }
        }

        /// <summary>
        /// Cache control
        /// </summary>
        public StreamingEndpointCacheControlData CacheControl
        {
            get { return _cacheControl == null ? null : new StreamingEndpointCacheControlData(_cacheControl); }
            set { _cacheControl = (StreamingEndpointCacheControl) value; }
        }

        /// <summary>
        /// Gets or sets streaming endpoint Cache control
        /// </summary>
        StreamingEndpointCacheControl IStreamingEndpoint.CacheControl
        {
            get { return _cacheControl; }
            set { _cacheControl = value; }
        }

        /// <summary>
        /// Default constructor for serailization
        /// </summary>
        public StreamingEndpointData()
        {
        }

        /// <summary>
        /// Create streaming endpoint data from the creation options.
        /// </summary>
        /// <param name="options">Streaming endpoint creation options.</param>
        /// <returns></returns>
        internal StreamingEndpointData(StreamingEndpointCreationOptions options)
        {

            Name = options.Name;
            Description = options.Description;
            ScaleUnits = options.ScaleUnits;
            CdnEnabled = options.CdnEnabled;
            CdnProfile = options.CdnProfile;
            CdnProvider = options.CdnProvider.ToString();
            StreamingEndpointVersion = options.StreamingEndpointVersion == null
                ? StreamingEndpointCreationOptions.DefaultVersion.ToString()
                : options.StreamingEndpointVersion.ToString();
            CrossSiteAccessPolicies = options.CrossSiteAccessPolicies;

            if (options.CustomHostNames != null)
            {
                CustomHostNames = (options.CustomHostNames as IList<string>) ??
                                  options.CustomHostNames.ToList();
            }

            _accessControl = options.AccessControl;
            _cacheControl = options.CacheControl;

            ValidateSettings();
        }

        /// <summary>
        /// Starts the streaming endpoint.
        /// </summary>
        public void Start()
        {
            AsyncHelper.Wait(StartAsync());
        }

        /// <summary>
        /// Starts the streaming endpoint asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StartAsync()
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Start", Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartStreamingEndpointPollInterval);
        }

        /// <summary>
        /// Sends start operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartOperation()
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Start", Id), UriKind.Relative);

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
        /// Stops the streaming endpoint.
        /// </summary>
        public void Stop()
        {
            AsyncHelper.Wait(StopAsync());
        }

        /// <summary>
        /// Stops the streaming endpoint asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StopAsync()
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Stop", Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StopStreamingEndpointPollInterval);
        }

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStopOperation()
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Stop", Id), UriKind.Relative);

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

        /// <summary>
        /// Deletes the streaming endpoint asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public override Task DeleteAsync()
        {
            return ((Task<IMediaDataServiceResponse>) base.DeleteAsync())
                .ContinueWith(t =>
                {
                    t.ThrowIfFaulted();

                    var operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    var operation = AsyncHelper.WaitOperationCompletion(
                        GetMediaContext(),
                        operationId,
                        StreamingConstants.DeleteStreamingEndpointPollInterval);

                    var messageFormat = Resources.ErrorDeleteStreamingEndpointFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            return;
                        case OperationState.Failed:
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed,
                                operationId, operation.ErrorMessage);
                            throw new InvalidOperationException(message);
                        default: // can never happen unless state enum is extended
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState,
                                operationId, operation.State);
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
            return AsyncHelper.Wait(SendDeleteOperationAsync());
        }

        /// <summary>
        /// Sends delete operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendDeleteOperationAsync()
        {
            return ((Task<IMediaDataServiceResponse>) base.DeleteAsync())
                .ContinueWith<IOperation>(t =>
                {
                    var operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    return new OperationData
                    {
                        ErrorCode = null,
                        ErrorMessage = null,
                        Id = operationId,
                        State = OperationState.InProgress.ToString(),
                    };
                });
        }

        /// <summary>
        /// Scales the streaming endpoint.
        /// </summary>
        /// <param name="scaleUnits">New scale units.</param>
        public void Scale(int scaleUnits)
        {
            AsyncHelper.Wait(ScaleAsync(scaleUnits));
        }

        /// <summary>
        /// Scales the streaming endpoint.
        /// </summary>
        /// <param name="scaleUnits">New scale units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task ScaleAsync(int scaleUnits)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Scale", Id), UriKind.Relative);

            var ruParameter = new BodyOperationParameter("scaleUnits", scaleUnits);

            return ExecuteActionAsync(uri, StreamingConstants.ScaleStreamingEndpointPollInterval, ruParameter);
        }

        /// <summary>
        /// Sends scale operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendScaleOperation(int scaleUnits)
        {
            var uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/StreamingEndpoints('{0}')/Scale", Id), UriKind.Relative);

            var ruParameter = new BodyOperationParameter("scaleUnits", scaleUnits);

            return SendOperation(uri, ruParameter);
        }

        /// <summary>
        /// Sends scale operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendScaleOperationAsync(int scaleUnits)
        {
            return Task.Factory.StartNew(() => SendScaleOperation(scaleUnits));
        }

        internal override void Refresh()
        {
            _accessControl = null;
            _cacheControl = null;
            base.Refresh();
        }

        /// <summary>
        /// Set array property empty array if it is null because OData does not support 
        /// empty collection 
        /// </summary>
        internal override sealed void ValidateSettings()
        {
            if (CustomHostNames == null)
            {
                CustomHostNames = new List<string>();
            }
        }
    }
}
