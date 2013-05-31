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

using System;
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

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="continuation">The continuation.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceContext context, DataServiceQueryContinuation<T> continuation, object state)
        {
            return Task.Factory.FromAsync<DataServiceQueryContinuation<T>, IEnumerable<T>>(context.BeginExecute<T>, context.EndExecute<T>, continuation, state);
        }

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="context">The context.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<IEnumerable<T>> ExecuteAsync<T>(this DataServiceContext context, Uri requestUri, object state)
        {
            return Task.Factory.FromAsync<Uri, IEnumerable<T>>(context.BeginExecute<T>, context.EndExecute<T>, requestUri, state);
        }

        /// <summary>
        /// Executes the asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="state">The state.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<OperationResponse> ExecuteAsync(this DataServiceContext context, Uri requestUri, object state, string httpMethod)
        {
            return Task.Factory.FromAsync<Uri, string, OperationResponse>(
                (u, m, ac, s) => context.BeginExecute(u, ac, s, m),
                (ar) => context.EndExecute(ar),
                requestUri,
                httpMethod,
                state);
        }

        /// <summary>
        /// Executes the batch operation asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The state.</param>
        /// <param name="queries">The queries.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<DataServiceResponse> ExecuteBatchAsync(this DataServiceContext context, object state, params DataServiceRequest[] queries)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return Task.Factory.FromAsync<DataServiceResponse>(context.BeginExecuteBatch(null, state, queries), context.EndExecuteBatch);
        }

        /// <summary>
        /// Gets the read stream asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="args">The args.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<DataServiceStreamResponse> GetReadStreamAsync(this DataServiceContext context, object entity, DataServiceRequestArgs args, object state)
        {
            return Task.Factory.FromAsync<object, DataServiceRequestArgs, DataServiceStreamResponse>(context.BeginGetReadStream, context.EndGetReadStream, entity, args, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, object state)
        {
            return Task.Factory.FromAsync<object, string, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="continuation">The continuation.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, DataServiceQueryContinuation continuation, object state)
        {
            return Task.Factory.FromAsync<object, string, DataServiceQueryContinuation, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, continuation, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="nextLinkUri">The next link URI.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<QueryOperationResponse> LoadPropertyAsync(this DataServiceContext context, object entity, string propertyName, Uri nextLinkUri, object state)
        {
            return Task.Factory.FromAsync<object, string, Uri, QueryOperationResponse>(context.BeginLoadProperty, context.EndLoadProperty, entity, propertyName, nextLinkUri, state);
        }

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, object state)
        {
            return Task.Factory.FromAsync<DataServiceResponse>(context.BeginSaveChanges, context.EndSaveChanges, state);
        }

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="options">The options.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public static Task<DataServiceResponse> SaveChangesAsync(this DataServiceContext context, SaveChangesOptions options, object state)
        {
            return Task.Factory.FromAsync<SaveChangesOptions, DataServiceResponse>(context.BeginSaveChanges, context.EndSaveChanges, options, state);
        }
    }
}
