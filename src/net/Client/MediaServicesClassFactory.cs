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

using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public abstract class MediaServicesClassFactory
    {
        /// <summary>
        /// Creates a data service context.
        /// </summary>
        /// <returns>The new DataServiceContext instance.</returns>
        public abstract IMediaDataServiceContext CreateDataServiceContext();

        /// <summary>
        /// Creates retry policy used for working with Azure blob storage.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetBlobStorageClientRetryPolicy();

        /// <summary>
        /// Creates retry policy for saving changes in Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetSaveChangesRetryPolicy();
 
        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// </summary>
        /// <returns>Retry policy.</returns>
        public abstract MediaRetryPolicy GetQueryRetryPolicy();
   }
}
