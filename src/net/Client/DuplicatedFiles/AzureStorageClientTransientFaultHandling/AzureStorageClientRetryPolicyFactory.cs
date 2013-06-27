//-----------------------------------------------------------------------
// <copyright file="AzureStorageClientRetryPolicyFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;


namespace Microsoft.WindowsAzure.MediaServices.Client.AzureStorageClientTransientFaultHandling
{
    public class AzureStorageClientRetryPolicyFactory
    {
        private const int ConnectionRetryMaxAttempts = 4;
        private const int ConnectionRetrySleepQuantum = 100;

        public static RetryPolicy DefaultPolicy
        {
            get
            {
                //exponential retry

                RetryPolicy retryPolicy =  new MediaRetryPolicy(
                    new StorageTransientErrorDetectionStrategy(),
                    retryCount: ConnectionRetryMaxAttempts,
                    minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                    maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                    deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));

                retryPolicy.Retrying +=
                    delegate(object sender, RetryingEventArgs e)
                        {
                            StringBuilder errorMessages = new StringBuilder();

                            errorMessages.AppendLine("Current RetryCount: " + e.CurrentRetryCount);
                            errorMessages.AppendLine("Delaying for (milliseconds): " + e.Delay.TotalMilliseconds);

                        };

                return retryPolicy;
            }
        }
    }
}
