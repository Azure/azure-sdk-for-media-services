//-----------------------------------------------------------------------
// <copyright file="IRetryPolicyAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    /// <summary>
    /// Interface IRetryPolicyAdapter. Implement this interface to adapt retry policy methods
    /// </summary>
    public interface IRetryPolicyAdapter
    {
        /// <summary>
        /// Method can be used to override default behavior of RetryPolicy.ExecuteAsync which returns Func<Task<TResult>>
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="taskFunc">The task function.</param>
        /// <returns>Func&lt;Task&lt;TResult&gt;&gt;.</returns>
        Func<Task<TResult>> AdaptExecuteAsync<TResult>(Func<Task<TResult>> taskFunc);
        
        /// <summary>
        /// Method can be used to override default behavior of RetryPolicy.ExecuteAsync which returns Func<Task>
        /// </summary>
        /// <param name="taskFunc">The task function.</param>
        /// <returns>Func&lt;Task&gt;.</returns>
        Func<Task>  AdaptExecuteAsync(Func<Task> taskFunc);
        
        /// <summary>
        /// Method can be used to override default behavior of RetryPolicy.ExecuteAction which returns Func<TResult>
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns>Func&lt;TResult&gt;.</returns>
        Func<TResult> AdaptExecuteAction<TResult>(Func<TResult> func);
    }
}