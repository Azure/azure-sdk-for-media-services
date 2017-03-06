//-----------------------------------------------------------------------
// <copyright file="MediaDataServiceContext.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Wraps System.Data.Services.Client.DataServiceContext.
    /// </summary>
    public class MediaDataServiceContext : IMediaDataServiceContext,IRetryPolicyAdapter
    {
        private readonly DataServiceContext _dataContext;
        private readonly MediaRetryPolicy _queryRetryPolicy;

       
        private readonly ClientRequestIdAdapter _clientRequestIdAdapter;

        public MediaDataServiceContext(DataServiceContext dataContext, MediaRetryPolicy queryRetryPolicy, ClientRequestIdAdapter clientRequestAdapter)
        {
            _dataContext = dataContext;
            _queryRetryPolicy = queryRetryPolicy;
            _clientRequestIdAdapter = clientRequestAdapter;

        }

        /// <summary>
        /// Gets or sets whether an exception is raised when a 404 error (resource not found) is returned by the data service.
        /// </summary>
        public bool IgnoreResourceNotFoundException
        {
            get
            {
                return _dataContext.IgnoreResourceNotFoundException;
            }
            set
            {
                _dataContext.IgnoreResourceNotFoundException = value;
            }
        }

        /// <summary>
        /// Creates a data service query for data of a specified generic type.
        /// create a query based on (BaseUri + relativeUri)
        /// </summary>
        /// <typeparam name="TIinterface">The exposed interface type of elements returned by the query.</typeparam>
        /// <typeparam name="TData">The type used by the query internaly.</typeparam>
        /// <param name="entitySetName">A string that resolves to a URI.</param>
        /// <returns>A new System.Data.Services.Client.DataServiceQuery<TElement> instance that represents a data service query.</returns>
        public IQueryable<TIinterface> CreateQuery<TIinterface, TData>(string entitySetName)
        {
            _clientRequestIdAdapter.ChangeCurrentRequestId();
            IQueryable<TIinterface> inner = (IQueryable<TIinterface>)_dataContext.CreateQuery<TData>(entitySetName);
            var result = new MediaQueryable<TIinterface, TData>(inner, _queryRetryPolicy);
            return result;
        }

        /// <summary>
        /// Notifies the System.Data.Services.Client.DataServiceContext to start tracking
        /// the specified resource and supplies the location of the resource in the specified
        /// resource set.
        /// Remarks:
        //     It does not follow the object graph and attach related objects.
        /// </summary>
        /// <param name="entitySetName">The string value that contains the name of the entity set to which to the entity is attached.</param>
        /// <param name="entity">The entity to add.</param>
        public void AttachTo(string entitySetName, object entity)
        {
            _dataContext.AttachTo(entitySetName, entity);
        }

        /// <summary>
        /// Notifies the System.Data.Services.Client.DataServiceContext to start tracking
        /// the specified resource and supplies the location of the resource in the specified
        /// resource set.
        /// Remarks:
        //     It does not follow the object graph and attach related objects.
        /// </summary>
        /// <param name="entitySetName">The string value that contains the name of the entity set to which to the entity is attached.</param>
        /// <param name="entity">The entity to add.</param>
        /// <param name="etag">An etag value that represents the state of the entity the last time it was
        /// retrieved from the data service. This value is treated as an opaque string;
        /// no validation is performed on it by the client library.</param>
        public void AttachTo(string entitySetName, object entity, string etag)
        {
            _dataContext.AttachTo(entitySetName, entity, etag);
        }
        
        /// <summary>
        /// Changes the state of the specified object to be deleted in the System.Data.Services.Client.DataServiceContext.
        /// Remarks:
        ///     Existing objects in the Added state become detached.
        /// </summary>
        /// <param name="entity">The tracked entity to be changed to the deleted state.</param>
        public void DeleteObject(object entity)
        {
            _dataContext.DeleteObject(entity);
        }

        /// <summary>
        /// Sends a request to the data service to execute a specific URI.Not supported
        /// by the WCF Data Services 5.0 client for Silverlight.
        /// </summary>
        /// <typeparam name="TElement">The type that the query returns.</typeparam>
        /// <param name="requestUri">The URI to which the query request will be sent. The URI may be any valid data service URI. Can contain $ query parameters.</param>
        /// <returns>The results of the query operation.</returns>
        public IEnumerable<TElement> Execute<TElement>(Uri requestUri)
        {
            return _dataContext.Execute<TElement>(requestUri);
        }

        /// <summary>
        /// Sends a request to the data service to execute a specific URI by using a
        /// specific HTTP method.Not supported by the WCF Data Services 5.0 client for
        /// Silverlight.
        /// This overload expects the requestUri to end with a ServiceOperation or ServiceAction that returns void.
        /// </summary>
        /// <param name="requestUri">The URI to which the query request will be sent. The URI may be any valid data service URI. Can contain $ query parameters.</param>
        /// <param name="httpMethod">The HTTP data transfer method used by the client.</param>
        /// <param name="operationParameters">The operation parameters used.</param>
        /// <returns>The response of the operation.</returns>
        public OperationResponse Execute(Uri requestUri, string httpMethod, params OperationParameter[] operationParameters)
        {
            return _dataContext.Execute(requestUri, httpMethod, operationParameters);
        }

        /// <summary>
        /// Adds the specified object to the set of objects that the System.Data.Services.Client.DataServiceContext is tracking.
        // Remarks:
        ///     It does not follow the object graph and add related objects.  Any leading
        ///     or trailing forward slashes will automatically be trimmed from entitySetName.
        /// </summary>
        /// <param name="entitySetName">The name of the entity set to which the resource will be added.</param>
        /// <param name="entity">The object to be tracked by the System.Data.Services.Client.DataServiceContext.</param>
        public void AddObject(string entitySetName, object entity)
        {
            _dataContext.AddObject(entitySetName, entity);
        }

        /// <summary>
        /// Loads deferred content for a specified property from the data service.Not
        ///  supported by the WCF Data Services 5.0 client for Silverlight.
        /// Remarks:
        ///     If entity is in in detached or added state, this method will throw an InvalidOperationException
        ///     since there is nothing it can load from the server.  If entity is in unchanged
        ///     or modified state, this method will load its collection or reference elements
        ///     as unchanged with unchanged bindings.  If entity is in deleted state, this
        ///     method will load the entities linked to by its collection or reference property
        ///     in the unchanged state with bindings in the deleted state.
        /// </summary>
        /// <param name="entity">The entity that contains the property to load.</param>
        /// <param name="propertyName">The name of the property of the specified entity to load.</param>
        /// <returns>The response to the load operation.</returns>
        public QueryOperationResponse LoadProperty(object entity, string propertyName)
        {
            return _dataContext.LoadProperty(entity, propertyName);
        }
        
        /// <summary>
        /// Changes the state of the specified object in the System.Data.Services.Client.DataServiceContext to System.Data.Services.Client.EntityStates.Modified.
        /// </summary>
        /// <param name="entity">The tracked entity to be assigned to the System.Data.Services.Client.EntityStates.Modified state.</param>
        public void UpdateObject(object entity)
        {
            _dataContext.UpdateObject(entity);
        }

        /// <summary>
        /// Notifies the System.Data.Services.Client.DataServiceContext that a new link
        /// exists between the objects specified and that the link is represented by
        /// the property specified by the sourceProperty parameter.
        /// Remarks:
        ///     Notifies the context that a modified link exists between the source and target
        ///     objects and that the link is represented via the source.sourceProperty which
        ///     is a reference.  The context adds this link to the set of modified created
        ///     links to be sent to the data service on the next call to SaveChanges(). 
        ///     Links are one way relationships. If a back pointer exists (ie. two way association),
        ///     this method should be called a second time to notify the context object of
        ///     the second link. 
        /// </summary>
        /// <param name="source">The source object for the new link.</param>
        /// <param name="sourceProperty">The property on the source object that identifies the target object of the new link.</param>
        /// <param name="target">The child object involved in the new link that is to be initialized by calling
        /// this method. The target object must be a subtype of the type identified by
        /// the sourceProperty parameter. If target is set to null, the call represents
        /// a delete link operation.
        /// </param>
        public void SetLink(object source, string sourceProperty, object target)
        {
            _dataContext.SetLink(source, sourceProperty, target);
        }

        /// <summary>
        /// Adds the specified link to the set of objects the System.Data.Services.Client.DataServiceContext
        // is tracking.
        /// Remarks:
        ///     Notifies the context that a new link exists between the source and target
        ///     objects and that the link is represented via the source.sourceProperty which
        ///     is a collection.  The context adds this link to the set of newly created
        ///     links to be sent to the data service on the next call to SaveChanges(). 
        ///     Links are one way relationships. If a back pointer exists (ie. two way association),
        ///     this method should be called a second time to notify the context object of
        ///     the second link. 
        /// </summary>
        /// <param name="source">The source object for the new link.</param>
        /// <param name="sourceProperty">The name of the navigation property on the source object that returns the related object.</param>
        /// <param name="target">The object related to the source object by the new link.</param>
        public void AddLink(object source, string sourceProperty, object target)
        {
            _dataContext.AddLink(source, sourceProperty, target);
        }

        /// <summary>
        /// Saves the changes that the System.Data.Services.Client.DataServiceContext
        /// is tracking to storage.Not supported by the WCF Data Services 5.0 client
        /// for Silverlight. 
        /// </summary>
        /// <returns>A System.Data.Services.Client.DataServiceResponse that contains status, headers,
        /// and errors that result from the call to System.Data.Services.Client.DataServiceContext.SaveChanges.Remarks.</returns>
        public IMediaDataServiceResponse SaveChanges()
        {
            return new MediaDataServiceResponse(_dataContext.SaveChanges());
        }

        /// <summary>
        /// Changes the state of the link to deleted in the list of links being tracked
        /// by the System.Data.Services.Client.DataServiceContext.
        /// Remarks:
        ///     Notifies the context that a link exists between the source and target object
        ///     and that the link is represented via the source.sourceProperty which is a
        ///     collection.  The context adds this link to the set of deleted links to be
        ///     sent to the data service on the next call to SaveChanges().  If the specified
        ///     link exists in the "Added" state, then the link is detached (see DetachLink
        ///     method) instead. 
        /// </summary>
        /// <param name="source">The source object in the link to be marked for deletion.</param>
        /// <param name="sourceProperty">The name of the navigation property on the source object that is used to access the target object.</param>
        /// <param name="target">The target object involved in the link that is bound to the source object.
        /// The target object must be of the type identified by the source property or
        /// a subtype.</param>
        public void DeleteLink(object source, string sourceProperty, object target)
        {
            _dataContext.DeleteLink(source, sourceProperty, target);
        }

        /// <summary>
        /// Adds a related object to the context and creates the link that defines the relationship between the two objects in a single request.
        /// </summary>
        /// <param name="source">The parent object that is being tracked by the context.</param>
        /// <param name="sourceProperty">The name of the navigation property that returns the related object based on an association between the two entities.</param>
        /// <param name="target">The related object that is being added.</param>
        public void AddRelatedObject(object source, string sourceProperty, object target)
        {
            _dataContext.AddRelatedObject(source, sourceProperty, target);
        }

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="continuation">The continuation.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IEnumerable<T>> ExecuteAsync<T>(DataServiceQueryContinuation<T> continuation, object state)
        {
            return Task.Factory.FromAsync<DataServiceQueryContinuation<T>, IEnumerable<T>>(_dataContext.BeginExecute<T>, _dataContext.EndExecute<T>, continuation, state);
        }

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IEnumerable<T>> ExecuteAsync<T>(Uri requestUri, object state)
        {
            return Task.Factory.FromAsync<Uri, IEnumerable<T>>(_dataContext.BeginExecute<T>, _dataContext.EndExecute<T>, requestUri, state);
        }

        /// <summary>
        /// Executes the asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="state">The state.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<OperationResponse> ExecuteAsync(Uri requestUri, object state, string httpMethod)
        {
            return Task.Factory.FromAsync<Uri, string, OperationResponse>(
                (u, m, ac, s) => _dataContext.BeginExecute(u, ac, s, m),
                (ar) => _dataContext.EndExecute(ar),
                requestUri,
                httpMethod,
                state);
        }
        /// <summary>
        /// Executes the url method asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="singleResult">Whether a single result is expected or not.</param>
        /// <param name="parameters">OperationParameters to be sent with the Execute request.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IEnumerable<T>> ExecuteAsync<T>(Uri requestUri, string httpMethod, bool singleResult, params OperationParameter[] parameters)
        {
            object state = null; // unused

            return Task.Factory.FromAsync<Uri, string, IEnumerable<T>>(
                (url, method, asyncCallback, stateObj) => _dataContext.BeginExecute<T>(url, asyncCallback, stateObj, method, singleResult, parameters),
                (asyncResult) => _dataContext.EndExecute<T>(asyncResult),
                requestUri,
                httpMethod,
                state);
        }
        /// <summary>
        /// Executes the batch operation asynchronously.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queries">The queries.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<DataServiceResponse> ExecuteBatchAsync(object state, params DataServiceRequest[] queries)
        {
            return Task.Factory.FromAsync<DataServiceResponse>(_dataContext.BeginExecuteBatch(null, state, queries), _dataContext.EndExecuteBatch);
        }

        /// <summary>
        /// Gets the read stream asynchronously.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="args">The args.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<DataServiceStreamResponse> GetReadStreamAsync(object entity, DataServiceRequestArgs args, object state)
        {
            return Task.Factory.FromAsync<object, DataServiceRequestArgs, DataServiceStreamResponse>(
                _dataContext.BeginGetReadStream, _dataContext.EndGetReadStream, entity, args, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, object state)
        {
            return Task.Factory.FromAsync<object, string, QueryOperationResponse>(
                _dataContext.BeginLoadProperty, _dataContext.EndLoadProperty, entity, propertyName, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="continuation">The continuation.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, DataServiceQueryContinuation continuation, object state)
        {
            return Task.Factory.FromAsync<object, string, DataServiceQueryContinuation, QueryOperationResponse>(
                _dataContext.BeginLoadProperty, _dataContext.EndLoadProperty, entity, propertyName, continuation, state);
        }

        /// <summary>
        /// Loads the property asynchronously.
        /// </summary>
       /// <param name="entity">The entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="nextLinkUri">The next link URI.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<QueryOperationResponse> LoadPropertyAsync(object entity, string propertyName, Uri nextLinkUri, object state)
        {
            return Task.Factory.FromAsync<object, string, Uri, QueryOperationResponse>(
                _dataContext.BeginLoadProperty, _dataContext.EndLoadProperty, entity, propertyName, nextLinkUri, state);
        }

        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IMediaDataServiceResponse> SaveChangesAsync(object state)
        {
            return Task.Factory.FromAsync<DataServiceResponse>(_dataContext.BeginSaveChanges, _dataContext.EndSaveChanges, state)
                .ContinueWith<IMediaDataServiceResponse>(t => WrapTask(t));
        }
        
        /// <summary>
        /// Saves the changes asynchronously.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="state">The state.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IMediaDataServiceResponse> SaveChangesAsync(SaveChangesOptions options, object state)
        {
            Task<DataServiceResponse> task = Task.Factory.FromAsync<SaveChangesOptions, DataServiceResponse>(
                _dataContext.BeginSaveChanges,
                _dataContext.EndSaveChanges,
                options,
                state);
            return task.ContinueWith<IMediaDataServiceResponse>(t => WrapTask(t),TaskContinuationOptions.AttachedToParent);
        }

        public Task<IMediaDataServiceResponse> SaveChangesAsync(SaveChangesOptions options, object state, CancellationToken token)
        {
            
            Task<DataServiceResponse> task = Task.Factory.FromAsync<SaveChangesOptions, DataServiceResponse>(
               _dataContext.BeginSaveChanges,
               _dataContext.EndSaveChanges,
               options,
               state).HandleCancellation(token);

            return task.ContinueWith<IMediaDataServiceResponse>(t => WrapTask(t));
        }

        private IMediaDataServiceResponse WrapTask(Task<DataServiceResponse> task)
        {
            task.ThrowIfFaulted();
            var result = new MediaDataServiceResponse(task.Result);
            result.AsyncState = task.AsyncState;
            return result;
        }

        public Func<Task> AdaptExecuteAsync(Func<Task> taskFunc)
        {
            _clientRequestIdAdapter.ChangeCurrentRequestId();
            return taskFunc;
        }

        Func<TResult> IRetryPolicyAdapter.AdaptExecuteAction<TResult>(Func<TResult> func)
        {
            _clientRequestIdAdapter.ChangeCurrentRequestId();
            return func;
        }

        Func<Task<TResult>> IRetryPolicyAdapter.AdaptExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
        {
            _clientRequestIdAdapter.ChangeCurrentRequestId();
            return taskFunc;
        }

    }
}
