//-----------------------------------------------------------------------
// <copyright file="RetryPolicyExtensions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client.AzureStorageClientTransientFaultHandling
{
    /// <summary>
    /// Extends the RetryPolicy to allow using the retry strategies from the Transient Fault Handling Application Block with Windows Azure Store.
    /// </summary>
    public static class RetryPolicyExtensions
    {
        /// <summary>
        /// Wrap a Transient Fault Handling Application Block retry policy into a Microsoft.WindowsAzure.StorageClient.RetryPolicy.
        /// </summary>
        /// <param name="retryPolicy">The Transient Fault Handling Application Block retry strategy to wrap.</param>
        /// <returns>Returns a wrapped Transient Fault Handling Application Block retry strategy into a Microsoft.WindowsAzure.StorageClient.RetryPolicy.</returns>
        public static IRetryPolicy AsAzureStorageClientRetryPolicy(
            this RetryPolicy retryPolicy)
        {
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            return new ShouldRetryWrapper(retryPolicy);
        }

        private class ShouldRetryWrapper : IRetryPolicy
        {
            private readonly RetryPolicy _retryPolicy;
            private readonly ShouldRetry _shouldRetry;

            public ShouldRetryWrapper(RetryPolicy retryPolicy)
            {
                _retryPolicy = retryPolicy;
                _shouldRetry = _retryPolicy.RetryStrategy.GetShouldRetry();
            }

            public IRetryPolicy CreateInstance()
            {
                return this;
            }

            public bool ShouldRetry(
                int currentRetryCount, 
                int statusCode, 
                Exception lastException, 
                out TimeSpan retryInterval, 
                WindowsAzure.Storage.OperationContext operationContext)
            {
                retryInterval = TimeSpan.FromMilliseconds(100);

                return _retryPolicy.ErrorDetectionStrategy.IsTransient(lastException) && _shouldRetry(currentRetryCount, lastException, out retryInterval);
            }
        }
    }
}
