//-----------------------------------------------------------------------
// <copyright file="AzureMediaServicesDataServiceContextFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
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
// </license>

using System;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Net;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.WindowsAzure.MediaServices.Client.Versioning;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A factory for creating the DataServiceContext connected to Windows Azure Media Services.
    /// </summary>
    public class AzureMediaServicesClassFactory : MediaServicesClassFactory
    {
        private readonly Uri _azureMediaServicesEndpoint;
        private readonly OAuthDataServiceAdapter _dataServiceAdapter;
        private readonly ServiceVersionAdapter _serviceVersionAdapter;
        private readonly MediaContextBase _mediaContext;
        
        private const int ConnectionRetryMaxAttempts = 4;
        private const int ConnectionRetrySleepQuantum = 100;

        private static Cache<Uri>  _endpointCache = new Cache<Uri>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaServicesClassFactory"/> class.
        /// </summary>
        public AzureMediaServicesClassFactory() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaServicesClassFactory"/> class.
        /// </summary>
        /// <param name="azureMediaServicesEndpoint">The Windows Azure Media Services endpoint to use.</param>
        /// <param name="dataServiceAdapter">The data service adapter.</param>
        /// <param name="serviceVersionAdapter">The service version adapter.</param>
        /// <param name="mediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        public AzureMediaServicesClassFactory(Uri azureMediaServicesEndpoint, OAuthDataServiceAdapter dataServiceAdapter, ServiceVersionAdapter serviceVersionAdapter, MediaContextBase mediaContext)
        {
            this._dataServiceAdapter = dataServiceAdapter;
            this._serviceVersionAdapter = serviceVersionAdapter;
            this._mediaContext = mediaContext;

            this._azureMediaServicesEndpoint = _endpointCache.GetOrAdd(
                azureMediaServicesEndpoint.ToString(),
                () => GetAccountApiEndpoint(this._dataServiceAdapter, this._serviceVersionAdapter, azureMediaServicesEndpoint),
                () => mediaContext.Credentials.TokenExpiration);
        }

        /// <summary>
        /// Creates a data service context.
        /// </summary>
        /// <returns>The new DataServiceContext instance.</returns>
        public override IMediaDataServiceContext CreateDataServiceContext()
        {
            DataServiceContext dataContext = new DataServiceContext(_azureMediaServicesEndpoint, DataServiceProtocolVersion.V3)
            {
                IgnoreMissingProperties = true,
                IgnoreResourceNotFoundException = true,
                MergeOption = MergeOption.PreserveChanges,
            };

            this._dataServiceAdapter.Adapt(dataContext);
            this._serviceVersionAdapter.Adapt(dataContext);

            dataContext.ReadingEntity += this.OnReadingEntity;

			MediaRetryPolicy queryRetryPolicy = GetQueryRetryPolicy();

            return new MediaDataServiceContext(dataContext, queryRetryPolicy);
        }

        /// <summary>
        /// Creates retry policy for working with Azure blob storage.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetBlobStorageClientRetryPolicy()
        {
            var retryPolicy = new MediaRetryPolicy(
                GetStorageTransientErrorDetectionStrategy(),
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));

            return retryPolicy;
        }

        /// <summary>
        /// Creates retry policy for saving changes in Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetSaveChangesRetryPolicy()
        {
            var retryPolicy = new MediaRetryPolicy(
                GetSaveChangesErrorDetectionStrategy(),
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));

            return retryPolicy;
        }

        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetQueryRetryPolicy()
        {
            var retryPolicy = new MediaRetryPolicy(
                GetQueryErrorDetectionStrategy(),
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));

            return retryPolicy;
        }

        private Uri GetAccountApiEndpoint(OAuthDataServiceAdapter dataServiceAdapter, ServiceVersionAdapter versionAdapter, Uri apiServer)
        {
            RetryPolicy retryPolicy = new RetryPolicy(
                GetWebRequestTransientErrorDetectionStrategy(),
                RetryStrategyFactory.DefaultStrategy());

            Uri apiEndpoint = null;
            retryPolicy.ExecuteAction(
                    () =>
                        {
                            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(apiServer);
                            request.AllowAutoRedirect = false;
                            dataServiceAdapter.AddAccessTokenToRequest(request);
                            versionAdapter.AddVersionToRequest(request);

                            using (WebResponse response = request.GetResponse())
                            {
                                apiEndpoint = GetAccountApiEndpointFromResponse(response);
                            }
                        }
                );

            return apiEndpoint;
        }

        private static Uri GetAccountApiEndpointFromResponse(WebResponse webResponse)
        {
            HttpWebResponse httpWebResponse = (HttpWebResponse)webResponse;

            if (httpWebResponse.StatusCode == HttpStatusCode.MovedPermanently)
            {
                return new Uri(httpWebResponse.Headers[HttpResponseHeader.Location]);
            }

            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return httpWebResponse.ResponseUri;
            }

            throw new InvalidOperationException("Unexpected response code.");
        }
        
        private void OnReadingEntity(object sender, ReadingWritingEntityEventArgs args)
        {
            IMediaContextContainer mediaContextContainer = args.Entity as IMediaContextContainer;
            if (mediaContextContainer != null)
            {
                mediaContextContainer.SetMediaContext(this._mediaContext);
            }
        }
    }
}