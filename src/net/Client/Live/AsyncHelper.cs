// Copyright 2012 Microsoft Corporation
// 
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

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Provides methods simplifying wait on tasks.
    /// </summary>
    internal static class AsyncHelper
    {
        /// <summary>
        /// Waits on a task.
        /// </summary>
        /// <typeparam name="T">Task result type.</typeparam>
        /// <param name="task">Task to wait on.</param>
        /// <returns>Result of the task.</returns>
        public static T Wait<T>(Task<T> task)
        {
            try
            {
                task.Wait();
                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Waits on a task.
        /// </summary>
        /// <param name="task">Task to wait on.</param>
        public static void Wait(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Waits for REST Nimbus Streaming operation completion
        /// </summary>
        /// <param name="context">The <seealso cref="CloudMediaContext"/> instance.</param>
        /// <param name="operationId">Id of the operation.</param>
        /// <param name="pollInterval">Poll interval.</param>
        /// <returns>Operation.</returns>
        public static IOperation WaitOperationCompletion(MediaContextBase context, string operationId, TimeSpan pollInterval)
        {
            IOperation operation;

            do
            {
                System.Threading.Thread.Sleep(pollInterval);

                IMediaDataServiceContext dataContext = context.MediaServicesClassFactory.CreateDataServiceContext();
                Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Operations('{0}')", operationId), UriKind.Relative);

                MediaRetryPolicy retryPolicy = context.MediaServicesClassFactory.GetQueryRetryPolicy();

                try
                {
                    operation = retryPolicy.ExecuteAction<IEnumerable<OperationData>>(() => dataContext.Execute<OperationData>(uri)).SingleOrDefault();
                }
                catch (Exception e)
                {
                    e.Data.Add("OperationId", operationId);
                    throw;
                }

                if (operation == null)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.CurrentCulture, Resources.ErrorOperationNotFoundFormat, operationId));
                }
            }
            while (operation.State == OperationState.InProgress);

            return operation;
        }
    }
}
