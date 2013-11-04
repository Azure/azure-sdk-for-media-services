//-----------------------------------------------------------------------
// <copyright file="MediaRetryPolicy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    public class MediaRetryPolicy : RetryPolicy
    {
        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
            : base(errorDetectionStrategy, retryStrategy)
        {
        }

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount)
            : this(errorDetectionStrategy, (RetryStrategy)new FixedInterval(retryCount))
        {
        }

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan retryInterval)
            : this(errorDetectionStrategy, (RetryStrategy)new FixedInterval(retryCount, retryInterval))
        {
        }

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
            : this(
                errorDetectionStrategy, (RetryStrategy)new ExponentialBackoff(retryCount, minBackoff, maxBackoff, deltaBackoff))
        {
        }

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(errorDetectionStrategy, (RetryStrategy)new Incremental(retryCount, initialInterval, increment))
        {
        }

        public override TResult ExecuteAction<TResult>(Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            int retryCount = 0;
            TimeSpan delay = TimeSpan.Zero;
            ShouldRetry shouldRetry = RetryStrategy.GetShouldRetry();

            while (true)
            {
                do
                {
                    try
                    {
                        return func();
                    }
                    catch (Exception ex)
                    {
                        if (!ErrorDetectionStrategy.IsTransient(ex))
                        {
                            throw;
                        }

                        if (shouldRetry(retryCount++, ex, out delay))
                        {
                            if (delay.TotalMilliseconds < 0.0)
                            {
                                delay = TimeSpan.Zero;
                            }   
                            OnRetrying(retryCount, ex, delay);
                        }
                        else
                        {
                            OnRetrying(retryCount, ex, delay);

                            throw;
                        }
                    }
                }
                while (retryCount <= 1 && RetryStrategy.FastFirstRetry);

                Thread.Sleep(delay);
            }
        }
    }

}
