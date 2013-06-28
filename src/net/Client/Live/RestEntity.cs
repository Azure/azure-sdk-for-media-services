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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal abstract class RestEntity<T>
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
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this);
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
            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(this.EntitySetName, this);
            dataContext.UpdateObject(this);

            return dataContext.SaveChangesAsync(this);
        }

        protected Task ExecuteActionAsync(Uri uri, TimeSpan pollInterval, params OperationParameter[] operationParameters)
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                if (this._cloudMediaContext != null)
                {
                    DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();

                    var response = dataContext.Execute(uri, "POST", operationParameters);

                    if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    {
                        throw new InvalidOperationException("Entity not found");
                    }
                    else if (response.StatusCode >= 400) 
                    {
                        var code = (HttpStatusCode)response.StatusCode;
                        throw new InvalidOperationException(code.ToString());
                    }
                    else if (response.StatusCode != (int)HttpStatusCode.PartialContent) // synchronous complete
                    {
                        Refresh();
                        return;
                    }

                    string operationId = response.Headers[StreamingConstants.OperationIdHeader];

                    var operation = AsyncHelper.WaitOperationCompletion(
                        this._cloudMediaContext,
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
            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();

            var response = dataContext.Execute(uri, "POST", operationParameters);

            if (response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Entity not found");
            }
            else if (response.StatusCode >= 400)
            {
                var code = (HttpStatusCode)response.StatusCode;
                throw new InvalidOperationException(code.ToString());
            }
            else if (response.StatusCode != (int)HttpStatusCode.PartialContent) // synchronous complete
            {
                Refresh();
                return new OperationData()
                {
                    ErrorCode = null,
                    ErrorMessage = null,
                    State = OperationState.Succeeded.ToString(),
                    Id = null
                };
            }

            string operationId = response.Headers[StreamingConstants.OperationIdHeader];

            return new OperationData()
            {
                ErrorCode = null,
                ErrorMessage = null,
                State = OperationState.InProgress.ToString(),
                Id = operationId
            };
        }

        internal void Refresh()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", EntitySetName, Id), UriKind.Relative);
            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this, Guid.NewGuid().ToString());
            dataContext.Execute<T>(uri).Single();
        }

        protected abstract string EntitySetName { get; }

        protected CloudMediaContext _cloudMediaContext;
    }
}
