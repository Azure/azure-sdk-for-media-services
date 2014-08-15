//-----------------------------------------------------------------------
// <copyright file="RetryStrategyFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    public static class RetryStrategyFactory
    {
        public const int ConnectionRetryMaxAttempts = 4;
        private const int ConnectionRetrySleepQuantum = 100;

        public static RetryStrategy DefaultStrategy()
        {
            return new ExponentialBackoff(
                retryCount: ConnectionRetryMaxAttempts,
                minBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum),
                maxBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum * 16),
                deltaBackoff: TimeSpan.FromMilliseconds(ConnectionRetrySleepQuantum));
        }
    }
}
