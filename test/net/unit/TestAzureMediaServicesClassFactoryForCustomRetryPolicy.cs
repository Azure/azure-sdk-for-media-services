//-----------------------------------------------------------------------
// <copyright file="TestMediaServicesClassFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using Moq;
using System.Net;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
{

    public class TestMediaServicesClassFactoryForCustomRetryPolicy : TestMediaServicesClassFactory
    {
        private IMediaDataServiceContext _dataContext;
        
        private const int ConnectionBlobMaxAttempts = 2;
        private const int ConnectionSaveRetryMaxAttempts = 3;
        private const int ConnectionQueryRetryMaxAttempts = 2;
        private const int ConnectionRetryInitialInterval = 200;
        private const int ConnectionRetrySleepQuantum = 100;
        public TestMediaServicesClassFactoryForCustomRetryPolicy(IMediaDataServiceContext dataContext)
            : base(dataContext)
        {
            _dataContext = dataContext;
        }

        public override IMediaDataServiceContext CreateDataServiceContext()
        {
            return _dataContext;
        }

        /// <summary>
        /// Creates retry policy for working with Azure blob storage.
        /// This overrides the GetBlobStorageClientRetryPolicy defined in AzureMediaServicesClassFactory
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetBlobStorageClientRetryPolicy()
        {
            //overriding the retry policy to have 2 retry attempts for working with Azure blob storage.
            //Also using SaveChangesRetry Polciy here.
            var retryPolicy = new MediaRetryPolicy(
                GetSaveChangesErrorDetectionStrategy(),
                ConnectionBlobMaxAttempts);

            return retryPolicy;
        }

        /// <summary>
        /// Creates retry policy for saving changes in Media Services REST layer.
        /// This overrides the GetSaveChangesRetryPolicy defined in AzureMediaServicesClassFactory
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetSaveChangesRetryPolicy(IRetryPolicyAdapter adapter)
        {
            //Overriding to create a retrypolicy with a different RetryInitialInterval and retrycount 
            //than the default one.Also creating a new custom retrystrategy for adding a new transient 
            //type failure in the list of transient exceptions.

            var retryPolicy = new MediaRetryPolicy(
                GetSaveChangesErrorDetectionStrategy(),
                retryCount: ConnectionSaveRetryMaxAttempts,
                initialInterval: TimeSpan.FromMilliseconds(ConnectionRetryInitialInterval),
                increment: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16)
                );
            retryPolicy.RetryPolicyAdapter = adapter;
            return retryPolicy;
        }

        /// <summary>
        /// Creates retry policy for querying Media Services REST layer.
        /// This overrides the GetQueryRetryPolicy defined in AzureMediaServicesClassFactory
        /// </summary>
        /// <returns>Retry policy.</returns>
        public override MediaRetryPolicy GetQueryRetryPolicy(IRetryPolicyAdapter adapter)
        {
            //Overriding to create a retrypolicy that has different retryattempts and retrysleepquantum than the default one.
            var retryPolicy = new MediaRetryPolicy(
                GetQueryErrorDetectionStrategy(),
                (RetryStrategy)new FixedInterval(ConnectionQueryRetryMaxAttempts, TimeSpan.FromMilliseconds((ConnectionRetrySleepQuantum))));
            retryPolicy.RetryPolicyAdapter = adapter;
            return retryPolicy;
        }

        /// <summary>
        /// Creates error detection strategy that can be used for detecting transient errors when SaveChanges() is invoked.
        /// </summary>
        /// <returns>Error detection strategy.</returns>
        public override MediaErrorDetectionStrategy GetSaveChangesErrorDetectionStrategy()
        {
            //This calls the custom retrystrategy that we created.
            return new TestSaveChangesErrorDetectionStrategy();
        }
    }
}