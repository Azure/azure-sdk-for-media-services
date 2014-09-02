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
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal abstract class RestEntity<T> : BaseEntity<T>
    {
        public string Id { get; set; }

        /// <summary>
        /// Deletes this instance.
        /// </summary>        
        public void Delete()
        {
            AsyncHelper.Wait(DeleteAsync());
        }

        /// <summary>
        /// Deletes this instance asynchronously.
        /// </summary>        
        public virtual Task DeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Sends update request to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendUpdateOperation()
        {
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.UpdateObject(this);
            
            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            var response = retryPolicy.ExecuteAction(() => dataContext.SaveChanges()).Single();

            if (response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Entity not found");
            }
            
            if (response.StatusCode >= 400)
            {
                var code = (HttpStatusCode)response.StatusCode;
                throw new InvalidOperationException(code.ToString());
            }
            
            if (response.StatusCode != (int)HttpStatusCode.Accepted) // synchronous complete
            {
                Refresh();
                return new OperationData
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    State = OperationState.Succeeded.ToString(),
                    Id = null
                };
            }

            string operationId = response.Headers[StreamingConstants.OperationIdHeader];

            return new OperationData
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };
        }


        /// <summary>
        /// Sends update request to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendUpdateOperationAsync()
        {
            return Task.Factory.StartNew(() => SendUpdateOperation());
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>        
        public void Update()
        {
            AsyncHelper.Wait(UpdateAsync());
        }

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task UpdateAsync()
        {
            return SendUpdateOperationAsync()
                .ContinueWith(t =>
                {
                    t.ThrowIfFaulted();

                    IOperation operation = t.Result;

                    while (operation.State == OperationState.InProgress)
                    {
                        operation = AsyncHelper.WaitOperationCompletion(
                            GetMediaContext(),
                            operation.Id,
                            StreamingConstants.CreateChannelPollInterval);
                    }

                    string messageFormat = Resources.ErrorUpdateFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            return;
                        case OperationState.Failed:
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed, operation.Id, operation.ErrorMessage);
                            throw new InvalidOperationException(message);
                        default: // can never happen unless state enum is extended
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState, operation.Id, operation.State);
                            throw new InvalidOperationException(message);
                    }
                });
        }

        protected Task ExecuteActionAsync(Uri uri, TimeSpan pollInterval, params OperationParameter[] operationParameters)
        {
            return Task.Factory.StartNew(() =>
            {
                if (GetMediaContext() != null)
                {
                    IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

                    MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

                    var response = retryPolicy.ExecuteAction(() => dataContext.Execute(uri, "POST", operationParameters));

                    if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    {
                        throw new InvalidOperationException("Entity not found");
                    }
                    
                    if (response.StatusCode >= 400) 
                    {
                        var code = (HttpStatusCode)response.StatusCode;
                        throw new InvalidOperationException(code.ToString());
                    }
                    
                    if (response.StatusCode != (int)HttpStatusCode.Accepted) // synchronous complete
                    {
                        Refresh();
                        return;
                    }

                    string operationId = response.Headers[StreamingConstants.OperationIdHeader];

                    var operation = AsyncHelper.WaitOperationCompletion(
                        GetMediaContext(),
                        operationId,
                        pollInterval);

                    Refresh();

                    string messageFormat = Resources.ErrorOperationFailedFormat;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            return;
                        case OperationState.Failed:
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, messageFormat, uri.OriginalString, Resources.Failed, operationId));
                        default:
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.CurrentCulture, messageFormat, uri.OriginalString, operation.State, operationId));
                    }
                }
            });
        }

        protected IOperation SendOperation(Uri uri, params OperationParameter[] operationParameters)
        {
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            var response = retryPolicy.ExecuteAction(() => dataContext.Execute(uri, "POST", operationParameters));

            if (response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Entity not found");
            }

            if (response.StatusCode >= 400)
            {
                var code = (HttpStatusCode)response.StatusCode;
                throw new InvalidOperationException(code.ToString());
            }

            if (response.StatusCode != (int)HttpStatusCode.Accepted) // synchronous complete
            {
                Refresh();
                return new OperationData
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    State = OperationState.Succeeded.ToString(),
                    Id = null
                };
            }

            string operationId = response.Headers[StreamingConstants.OperationIdHeader];

            return new OperationData
            {
                ErrorCode = null,
                ErrorMessage = null,
                State = OperationState.InProgress.ToString(),
                Id = operationId
            };
        }

        internal virtual void Refresh()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", EntitySetName, Id), UriKind.Relative);
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this, Guid.NewGuid().ToString());

            MediaRetryPolicy retryPolicy = GetMediaContext().MediaServicesClassFactory.GetQueryRetryPolicy();

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            retryPolicy.ExecuteAction(() => dataContext.Execute<T>(uri)).Single();
        }

        protected abstract string EntitySetName { get; }
    }
}
