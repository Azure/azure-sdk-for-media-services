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

using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net;


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
        private readonly UserAgentAdapter _userAgentAdapter;

        private const int ConnectionRetryMaxAttempts = 4;
        private const int ConnectionRetrySleepQuantum = 100;

        private static Cache<Uri> _endpointCache = new Cache<Uri>();
        private IWebRequestAdapter _clientRequestIdAdapter;
        public AzureMediaServicesClassFactory()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaServicesClassFactory" /> class.
        /// </summary>
        /// <param name="azureMediaServicesEndpoint">The Windows Azure Media Services endpoint to use.</param>
        /// <param name="mediaContext">The <seealso cref="CloudMediaContext" /> instance.</param>
        public AzureMediaServicesClassFactory(Uri azureMediaServicesEndpoint, CloudMediaContext mediaContext)
        {
            _dataServiceAdapter = new OAuthDataServiceAdapter(mediaContext.TokenProvider);
            _serviceVersionAdapter = new ServiceVersionAdapter(KnownApiVersions.Current);
            _userAgentAdapter = new UserAgentAdapter(KnownClientVersions.Current);
            _mediaContext = mediaContext;
            _azureMediaServicesEndpoint = CreateAzureMediaServicesEndPoint(azureMediaServicesEndpoint, mediaContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaServicesClassFactory" /> class.
        /// </summary>
        /// <param name="azureMediaServicesEndpoint">The Windows Azure Media Services endpoint to use.</param>
        /// <param name="dataServiceAdapter">The data service adapter.</param>
        /// <param name="serviceVersionAdapter">The service version adapter.</param>
        /// <param name="mediaContext">The <seealso cref="CloudMediaContext" /> instance.</param>
        /// <param name="userAgentAdapter">The user agent request adapter</param>
        public AzureMediaServicesClassFactory(Uri azureMediaServicesEndpoint, OAuthDataServiceAdapter dataServiceAdapter, ServiceVersionAdapter serviceVersionAdapter, MediaContextBase mediaContext, UserAgentAdapter userAgentAdapter)
        {
            _dataServiceAdapter = dataServiceAdapter;
            _serviceVersionAdapter = serviceVersionAdapter;
            _mediaContext = mediaContext;
            _userAgentAdapter = userAgentAdapter;
            _azureMediaServicesEndpoint = CreateAzureMediaServicesEndPoint(azureMediaServicesEndpoint, mediaContext);
        }

        /// <summary>
        /// Creates instance of <see cref="IMediaDataServiceContext"/>.Deafault list of <see cref="IDataServiceContextAdapter"/> applied .
        /// </summary>
        /// <returns>The new  <see cref="IMediaDataServiceContext"/> instance.</returns>
        public override IMediaDataServiceContext CreateDataServiceContext()
        {

            return CreateDataServiceContext(new List<IDataServiceContextAdapter>());

        }

        /// <summary>
        /// Creates instance of <see cref="IMediaDataServiceContext"/> with contains additional applyed <see cref="IDataServiceContextAdapter"/> adapters 
        /// </summary>
        /// <param name="adapters"></param>
        /// <returns><see cref="IMediaDataServiceContext"/></returns>
        public override IMediaDataServiceContext CreateDataServiceContext(IEnumerable<IDataServiceContextAdapter> adapters)
        {
            DataServiceContext dataContext = new DataServiceContext(_azureMediaServicesEndpoint, DataServiceProtocolVersion.V3)
            {
                IgnoreMissingProperties = true,
                IgnoreResourceNotFoundException = true,
                MergeOption = MergeOption.PreserveChanges,
            };

            List<IDataServiceContextAdapter> dataServiceContextAdapters = GetDefaultDataContextAdapters().ToList();
            dataServiceContextAdapters.AddRange(adapters.ToList());
            dataServiceContextAdapters.ForEach(c => c.Adapt(dataContext));

            ClientRequestIdAdapter clientRequestIdAdapter = dataServiceContextAdapters.FirstOrDefault(c => c is ClientRequestIdAdapter) as ClientRequestIdAdapter;
            dataContext.ReadingEntity += OnReadingEntity;
            var queryRetryPolicy = GetQueryRetryPolicy(null);
            var context = new MediaDataServiceContext(dataContext, queryRetryPolicy, clientRequestIdAdapter);
            queryRetryPolicy.RetryPolicyAdapter = context;
            return context;
        }

        /// <summary>
        /// Creates a clientRequestIdAdapter
        /// </summary>
        /// <returns>The new DataServiceContext instance.</returns>
        public override IWebRequestAdapter CreateClientRequestIdAdapter()
        {
            if (_clientRequestIdAdapter == null)
            {
                _clientRequestIdAdapter = new ClientRequestIdAdapter();
            }
            return _clientRequestIdAdapter;
        }

        /// <summary>
        /// Returns IEnumerable of type <see cref="IDataServiceContextAdapter"/> which applied by default for each request  
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IDataServiceContextAdapter> GetDefaultDataContextAdapters()
        {
            var clientRequestIdAdapter = new ClientRequestIdAdapter();
            return new IDataServiceContextAdapter[]{ _dataServiceAdapter, _serviceVersionAdapter, _userAgentAdapter, clientRequestIdAdapter };
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
        public override MediaRetryPolicy GetSaveChangesRetryPolicy(IRetryPolicyAdapter adapter)
        {
            var retryPolicy = new MediaRetryPolicy(
                GetSaveChangesErrorDetectionStrategy(),
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum)
                );
            retryPolicy.RetryPolicyAdapter = adapter;
            return retryPolicy;
        }

        [Obsolete]
        public override MediaRetryPolicy GetSaveChangesRetryPolicy()
        {
            return GetSaveChangesRetryPolicy(null);
        }


        [Obsolete]
        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetQueryRetryPolicy()
        {
            return GetQueryRetryPolicy(null);
        }
        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetQueryRetryPolicy(IRetryPolicyAdapter adapter)
        {
            var retryPolicy = new MediaRetryPolicy(
                GetQueryErrorDetectionStrategy(),
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));
            retryPolicy.RetryPolicyAdapter = adapter;
            return retryPolicy;
        }

        private Uri GetAccountApiEndpoint(OAuthDataServiceAdapter dataServiceAdapter, ServiceVersionAdapter versionAdapter, Uri apiServer, UserAgentAdapter userAgentAdapter, IWebRequestAdapter clientRequestIdAdapter)
        {
            MediaRetryPolicy retryPolicy = new MediaRetryPolicy(
                GetWebRequestTransientErrorDetectionStrategy(),
                RetryStrategyFactory.DefaultStrategy());

            Uri apiEndpoint = null;
            retryPolicy.ExecuteAction(
                    () =>
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiServer);
                        request.AllowAutoRedirect = false;
                        dataServiceAdapter.AddAccessTokenToRequest(request);
                        versionAdapter.AddVersionToRequest(request);
                        userAgentAdapter.AddUserAgentToRequest(request);
                        clientRequestIdAdapter.AddClientRequestId(request);

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
                mediaContextContainer.SetMediaContext(_mediaContext);
            }
        }

        private Uri CreateAzureMediaServicesEndPoint(Uri azureMediaServicesEndpoint, MediaContextBase mediaContext)
        {
            string cacheKey = string.Format(
                "{0},{1}",
                mediaContext.TokenProvider.MediaServicesAccountName,
                azureMediaServicesEndpoint.ToString());

            return (_endpointCache.GetOrAdd(
                cacheKey,
                () => GetAccountApiEndpoint(_dataServiceAdapter, _serviceVersionAdapter, azureMediaServicesEndpoint, _userAgentAdapter, CreateClientRequestIdAdapter()),
                () => mediaContext.TokenProvider.GetAccessToken().Item2.DateTime));
        }
    }
}