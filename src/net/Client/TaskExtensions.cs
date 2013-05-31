//-----------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Extension methods for a task.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Throws an exception if the task faulted.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="faultedCallback">The faulted callback.</param>
        public static void ThrowIfFaulted(this Task task, Action faultedCallback)
        {
            if (task.IsFaulted)
            {
                if (faultedCallback != null)
                {
                    faultedCallback();
                }

                AggregateException exception = task.Exception.Flatten();

                if (exception.InnerExceptions.Count == 1)
                {
                    throw exception.InnerException;
                }

                throw exception;
            }
        }

        /// <summary>
        /// Throws an exception if the task is faulted.
        /// </summary>
        /// <param name="task">The task.</param>
        public static void ThrowIfFaulted(this Task task)
        {
            task.ThrowIfFaulted(null);
        }

        /// <summary>
        /// Throws an exception if cancellation was requested.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="cancelledCallback">The cancelled callback.</param>
        public static void ThrowIfCancellationRequested(this CancellationToken cancellationToken, Action cancelledCallback)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (cancelledCallback != null)
                {
                    cancelledCallback();
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
