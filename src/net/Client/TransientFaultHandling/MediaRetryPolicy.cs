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
using System.Threading.Tasks;
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    public class MediaRetryPolicy : RetryPolicy
    {
        public IRetryPolicyAdapter RetryPolicyAdapter { get; set; } 

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

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff, IRetryPolicyAdapter adapter = null)
            : this(
                errorDetectionStrategy, (RetryStrategy)new ExponentialBackoff(retryCount, minBackoff, maxBackoff, deltaBackoff))
        {
        }

        public MediaRetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, int retryCount, TimeSpan initialInterval, TimeSpan increment)
            : this(errorDetectionStrategy, (RetryStrategy)new Incremental(retryCount, initialInterval, increment))
        {
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns>TResult.</returns>
        /// <exception cref="System.ArgumentNullException">func</exception>
        public override TResult ExecuteAction<TResult>(Func<TResult> func)
        {

            //Converting func,if  RetryPolicyAdapter defined 
            var adaptedFunction = RetryPolicyAdapter != null ? RetryPolicyAdapter.AdaptExecuteAction(func) : func;

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
                        return adaptedFunction();
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

        /// <summary>
        /// Repeatedly executes the specified asynchronous task while it satisfies the
        //     current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="taskFunc">A function that returns a started task (also refered as "hot" task).</param>
        /// <returns>Task&lt;TResult&gt;.Returns a task that will run to completion if the original task completes
        ///     successfully (either the first time or after retrying transient failures).
       ///      If the task fails with a non-transient error or the retry limit is reached,
       ///     the returned task will become faulted and the exception must be observed.</returns>
        public new Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            //If adapter defined, we are converting taskFunc before executing it 
            return base.ExecuteAsync(RetryPolicyAdapter != null ? RetryPolicyAdapter.AdaptExecuteAsync(taskFunc) : taskFunc);
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task (also refered as "hot" task).</param>
        /// <returns>Returns a task that will run to completion if the original task completes successfully (either the
        /// first time or after retrying transient failures). If the task fails with a non-transient error or
        /// the retry limit is reached, the returned task will become faulted and the exception must be observed.</returns>
        public new Task ExecuteAsync(Func<Task> taskAction)
        {
            //If adapter defined, we are converting taskAction before executing it 
            return base.ExecuteAsync(RetryPolicyAdapter != null ? RetryPolicyAdapter.AdaptExecuteAsync(taskAction) : taskAction);
        }

        /// <summary>
        ///  Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="taskFunc">A function that returns a started task (also refered as "hot" task).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns a task that will run to completion if the original task completes
        ///     successfully (either the first time or after retrying transient failures).
        ///     If the task fails with a non-transient error or the retry limit is reached,
        ///     the returned task will become faulted and the exception must be observed.</returns>
        public new Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            return base.ExecuteAsync<TResult>(RetryPolicyAdapter != null ? RetryPolicyAdapter.AdaptExecuteAsync(taskFunc) : taskFunc, cancellationToken);
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task (also refered as "hot" task).</param>
        /// <param name="cancellationToken">To cancel the retry operation, but not operations that are already in flight or that already completed successfully.</param>
        /// <returns>Returns a task that will run to completion if the original task completes successfully (either the
        /// first time or after retrying transient failures). If the task fails with a non-transient error or
        /// the retry limit is reached, the returned task will become faulted and the exception must be observed.</returns>
        public new Task ExecuteAsync(Func<Task> taskAction, CancellationToken cancellationToken)
        {
            return base.ExecuteAsync(RetryPolicyAdapter != null ? RetryPolicyAdapter.AdaptExecuteAsync(taskAction) : taskAction, cancellationToken);
        }
    }

}
