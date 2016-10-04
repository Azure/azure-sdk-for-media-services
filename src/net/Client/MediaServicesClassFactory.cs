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
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public abstract class MediaServicesClassFactory
    {
        /// <summary>
        /// Creates instance of <see cref="IMediaDataServiceContext"/>.Deafault list of <see cref="IDataServiceContextAdapter"/> applied .
        /// </summary>
        /// <returns>The new  <see cref="IMediaDataServiceContext"/> instance.</returns>
        public abstract IMediaDataServiceContext CreateDataServiceContext();

        /// <summary>
        /// Creates instance of <see cref="IMediaDataServiceContext"/> with contains additional applyed <see cref="IDataServiceContextAdapter"/> adapters 
        /// </summary>
        /// <param name="adapters">list of <see cref="IDataServiceContextAdapter"/> which will be applied additionally on top of default adapters</param>
        /// <returns><see cref="IMediaDataServiceContext"/></returns>
        public abstract IMediaDataServiceContext CreateDataServiceContext(IEnumerable<IDataServiceContextAdapter> adapters);

        /// <summary>
        /// Returns list of <see cref="IDataServiceContextAdapter"/> which applied by default for each request  
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<IDataServiceContextAdapter> GetDefaultDataContextAdapters();

        /// <summary>
        /// Creates a ClientRequestIdAdapter
        /// </summary>
        /// <returns>The new ClientRequestIdAdapter instance.</returns>
        public abstract IWebRequestAdapter CreateClientRequestIdAdapter();
        /// <summary>
        /// Creates retry policy used for working with Azure blob storage.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetBlobStorageClientRetryPolicy();

        [Obsolete]
        /// <summary>
        /// Creates retry policy for saving changes in Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetSaveChangesRetryPolicy();

        [Obsolete]
        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetQueryRetryPolicy();

        /// <summary>
        /// Creates error detection strategy that can be used for detecting transient errors in web request related operations.
        /// </summary>
        /// <returns>Error detection strategy.</returns>
        public virtual MediaErrorDetectionStrategy GetWebRequestTransientErrorDetectionStrategy()
        {
            return new WebRequestTransientErrorDetectionStrategy();
        }

        /// <summary>
        /// Creates error detection strategy that can be used for detecting transient errors in OData queries.
        /// </summary>
        /// <returns>Error detection strategy.</returns>
        public virtual MediaErrorDetectionStrategy GetQueryErrorDetectionStrategy()
        {
            return new QueryErrorDetectionStrategy();
        }

        /// <summary>
        /// Creates error detection strategy that can be used for detecting transient errors when SaveChanges() is invoked.
        /// </summary>
        /// <returns>Error detection strategy.</returns>
        public virtual MediaErrorDetectionStrategy GetSaveChangesErrorDetectionStrategy()
        {
            return new SaveChangesErrorDetectionStrategy();
        }

        /// <summary>
        /// Creates error detection strategy that can be used for detecting transient errors in Azure storage related operations.
        /// </summary>
        /// <returns>Error detection strategy.</returns>
        public virtual MediaErrorDetectionStrategy GetStorageTransientErrorDetectionStrategy()
        {
            return new StorageTransientErrorDetectionStrategy();
        }


        public virtual BlobTransferClient GetBlobTransferClient()
        {
            return new BlobTransferClient();
        }

        public abstract MediaRetryPolicy GetSaveChangesRetryPolicy(IRetryPolicyAdapter adapter);
        public abstract MediaRetryPolicy GetQueryRetryPolicy(IRetryPolicyAdapter adapter);
    }
}