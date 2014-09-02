//-----------------------------------------------------------------------
// <copyright file="TestRetryAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    /// <summary>
    /// Class TestRetryAdapter.Adapting retry policy for testing purpose to validate how adapt logic has been called
    /// </summary>
    public class TestRetryAdapter : IRetryPolicyAdapter
    {

        /// <summary>
        /// To track how many times Adapt menthods has been called
        /// </summary>
        public int NumberOfAdaptCalled = 0;
        /// <summary>
        /// To track how many time func returned by AdaptExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)  has been executed
        /// </summary>
        public int FuncExecutedCountByExecuteAsync1 = 0;
        /// <summary>
        /// To track how many time func returned by AdaptExecuteAction<TResult>(Func<TResult> func)  has been executed
        /// </summary>
        public int FuncExecutedCountByExecuteAction = 0;

        public TestRetryAdapter()
        {
            
        }

        public Func<Task<TResult>> AdaptExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            NumberOfAdaptCalled++;
            return new Func<Task<TResult>>(() => taskFunc().ContinueWith(task =>
            {
                FuncExecutedCountByExecuteAsync1++;
                return task.Result; 
            },
                TaskContinuationOptions.ExecuteSynchronously));

        }

        public Func<Task> AdaptExecuteAsync(Func<Task> taskFunc)
        {
            NumberOfAdaptCalled++;
            return taskFunc;
        }

        public Func<TResult> AdaptExecuteAction<TResult>(Func<TResult> func)
        {
            NumberOfAdaptCalled++;
            return new Func<TResult>(() =>
            {
                FuncExecutedCountByExecuteAction++;
                return func();
            });
        }
    }
}