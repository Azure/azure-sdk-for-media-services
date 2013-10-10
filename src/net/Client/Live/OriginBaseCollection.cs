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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IOrigin"/>.
    /// </summary>
    public class OriginBaseCollection : CloudBaseCollection<IOrigin>
    {
        internal const string OriginSet = "Origins";

        /// <summary>
        /// Initializes a new instance of the <see cref="OriginBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal OriginBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            this.MediaContext = cloudMediaContext;
            this.Queryable = this.MediaContext.DataContextFactory.CreateDataServiceContext().CreateQuery<OriginData>(OriginSet);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>The created Origin.</returns>
        public IOrigin Create(
                   string name,
                   int reservedUnits)
        {
            return Create(
                name,
                null,
                reservedUnits,
                null);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>The created Origin.</returns>
        public IOrigin Create(
                   string name,
                   string description,
                   int reservedUnits)
        {
            return Create(
                name,
                description,
                reservedUnits,
                null);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>The created Origin.</returns>
        public IOrigin Create(
                    string name,
                    int reservedUnits,
                    OriginSettings settings)
        {
            return Create(
                name,
                null,
                reservedUnits,
                settings);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>The created Origin.</returns>
        public IOrigin Create(
                    string name,
                    string description,
                    int reservedUnits,
                    OriginSettings settings)
        {
            return AsyncHelper.Wait(this.CreateAsync(
                name,
                description,
                reservedUnits,
                settings));
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task<IOrigin> CreateAsync(
            string name,
            string description,
            int reservedUnits)
        {
            return CreateAsync(name, description, reservedUnits, null);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task<IOrigin> CreateAsync(
                    string name,
                    int reservedUnits,
                    OriginSettings settings)
        {
            return CreateAsync(name, null, reservedUnits, settings);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task<IOrigin> CreateAsync(
            string name,
            int reservedUnits)
        {
            return CreateAsync(name, null, reservedUnits, null);
        }

        /// <summary>
        /// Creates new Origin.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task<IOrigin> CreateAsync(
            string name,
            string description,
            int reservedUnits,
            OriginSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyOriginName);
            }

            var origin = new OriginData
            {
                Description = description,
                Name = name,
                ReservedUnits = reservedUnits,
            };

            ((IOrigin)origin).Settings = settings;

            origin.MediaContext =this.MediaContext;

            DataServiceContext dataContext = this.MediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AddObject(OriginSet, origin);

            return dataContext
                .SaveChangesAsync(origin)
                .ContinueWith<IOrigin>(t =>
                {
                    t.ThrowIfFaulted();

                    string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                    IOperation operation = AsyncHelper.WaitOperationCompletion(
                        this.MediaContext,
                        operationId,
                        StreamingConstants.CreateOriginPollInterval);

                    string messageFormat = Resources.ErrorCreateOriginFailedFormat;
                    string message;

                    switch (operation.State)
                    {
                        case OperationState.Succeeded:
                            OriginData result = (OriginData)t.AsyncState;
                            result.Refresh();
                            return result;
                        case OperationState.Failed:
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed, operationId, operation.ErrorMessage);
                            throw new InvalidOperationException(message);
                        default: // can never happen unless state enum is extended
                            message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState, operationId, operation.State);
                            throw new InvalidOperationException(message);
                    }
                });        
        }

        #region Non-polling asyncs

        /// <summary>
        /// Sends create origin operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            int reservedUnits)
        {
            return SendCreateOperation(name, null, reservedUnits, null);
        }

        /// <summary>
        /// Sends create origin operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            int reservedUnits)
        {
            return SendCreateOperation(name, description, reservedUnits, null);
        }

        /// <summary>
        /// Sends create origin operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            int reservedUnits,
            OriginSettings settings)
        {
            return SendCreateOperation(name, null, reservedUnits, settings);
        }

        /// <summary>
        /// Sends create origin operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendCreateOperation(
            string name,
            string description,
            int reservedUnits,
            OriginSettings settings)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ErrorEmptyOriginName);
            }

            var origin = new OriginData
            {
                Description = description,
                Name = name,
                ReservedUnits = reservedUnits,
            };

            ((IOrigin)origin).Settings = settings;

            origin.MediaContext = this.MediaContext;

            DataServiceContext dataContext = this.MediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AddObject(OriginSet, origin);
            var response = dataContext.SaveChanges();

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            IOperation result = new OperationData()
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
        }

        /// <summary>
        /// Sends create origin operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            int reservedUnits)
        {
            return SendCreateOperationAsync(name, null, reservedUnits, null);
        }

        /// <summary>
        /// Sends create origin operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            string description,
            int reservedUnits)
        {
            return SendCreateOperationAsync(name, description, reservedUnits, null);
        }

        /// <summary>
        /// Sends create origin operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            int reservedUnits,
            OriginSettings settings)
        {
            return SendCreateOperationAsync(name, null, reservedUnits, settings);
        }

        /// <summary>
        /// Sends create origin operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <param name="name">Name of the origin.</param>
        /// <param name="description">Description of the Origin.</param>
        /// <param name="reservedUnits">Reserved units.</param>
        /// <param name="settings">Origin settings.</param>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendCreateOperationAsync(
            string name,
            string description,
            int reservedUnits,
            OriginSettings settings)
        {
            return Task.Factory.StartNew(() => SendCreateOperation(name, description, reservedUnits, settings));
        }

        #endregion Non-polling asyncs
    }
}
