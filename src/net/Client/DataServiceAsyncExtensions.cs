//-----------------------------------------------------------------------
// <copyright file="DataServiceAsyncExtensions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Data.Services.Client;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Extension methods to DataService classes.
    /// </summary>
    public static class DataServiceAsyncExtensions
    {
        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of element.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The state transferable state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceQuery<T> query, object state)
        {
            return Task.Factory.FromAsync<IEnumerable<T>>(query.BeginExecute, query.EndExecute, state);
        }

    }
}
