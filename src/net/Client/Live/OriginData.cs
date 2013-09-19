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
using System.Data.Services.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Properties;
using Microsoft.WindowsAzure.MediaServices.Client.Rest;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class OriginData : RestEntity<OriginData>, IOrigin, ICloudMediaContextInit
    {
        /// <summary>
        /// Gets or sets the name of the origin.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the origin.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets host name of the origin.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets origin creation date.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets origin last modification date.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets state of the origin.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the number of Reserved Units of the origin.
        /// </summary>
        public int ReservedUnits { get; set; }

        /// <summary>
        /// Gets or sets origin settings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Settings 
        {
            get
            {
                return Serializer.Serialize(new OriginServiceSettings(_settings));
            }
            set
            {
                _settings = (OriginSettings)Serializer.Deserialize<OriginServiceSettings>(value);
            }
        }

        #region ICloudMediaContextInit Members
        /// <summary>
        /// Initializes the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            this._cloudMediaContext = (CloudMediaContext)context;
        }

        #endregion

        /// <summary>
        /// Gets state of the origin.
        /// </summary>
        OriginState IOrigin.State 
        { 
            get 
            {
                return (OriginState)Enum.Parse(typeof(OriginState), State, true);
            } 
        }

        public SingleOriginMetricsMonitor MetricsMonitor { get; set; }

        /// <summary>
        /// Gets or sets origin settings.
        /// </summary>
        OriginSettings IOrigin.Settings 
        { 
            get 
            {
                return _settings; 
            }

            set
            {
                _settings = value; 
            } 
        }

        /// <summary>
        /// Starts the origin.
        /// </summary>
        public void Start()
        {
            AsyncHelper.Wait(StartAsync());
        }

        /// <summary>
        /// Starts the origin asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StartAsync()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Start", this.Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StartOriginPollInterval);
        }

        /// <summary>
        /// Sends start operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStartOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Start", this.Id), UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends start operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendStartOperationAsync()
        {
            return Task.Factory.StartNew(() => SendStartOperation());
        }

        /// <summary>
        /// Stops the origin.
        /// </summary>
        public void Stop()
        {
            AsyncHelper.Wait(StopAsync());
        }

        /// <summary>
        /// Stops the origin asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task StopAsync()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Stop", this.Id), UriKind.Relative);

            return ExecuteActionAsync(uri, StreamingConstants.StopOriginPollInterval);
        }

        /// <summary>
        /// Sends stop operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendStopOperation()
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Stop", this.Id), UriKind.Relative);

            return SendOperation(uri);
        }

        /// <summary>
        /// Sends stop operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendStopOperationAsync()
        {
            return Task.Factory.StartNew(() => SendStopOperation());
        }

        /// <summary>
        /// Scales the origin.
        /// </summary>
        /// <param name="reservedUnits">New reserved units.</param>
        public void Scale(int reservedUnits)
        {
            AsyncHelper.Wait(ScaleAsync(reservedUnits));
        }

        /// <summary>
        /// Sends scale operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendScaleOperation(int reservedUnits)
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Scale", this.Id), UriKind.Relative);

            BodyOperationParameter ruParameter = new BodyOperationParameter("ReservedUnits", reservedUnits);

            return SendOperation(uri, ruParameter);
        }

        /// <summary>
        /// Sends scale operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendScaleOperationAsync(int reservedUnits)
        {
            return Task.Factory.StartNew(() => SendScaleOperation(reservedUnits));
        }

        /// <summary>
        /// Scales the origin asynchronously.
        /// </summary>
        /// <param name="reservedUnits">New reserved units.</param>
        /// <returns>Task to wait on for operation completion.</returns>
        public Task ScaleAsync(int reservedUnits)
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/Origins('{0}')/Scale", this.Id), UriKind.Relative);

            BodyOperationParameter ruParameter = new BodyOperationParameter("ReservedUnits", reservedUnits);

            return ExecuteActionAsync(uri, StreamingConstants.ScaleOriginPollInterval, ruParameter);
        }

        /// <summary>
        /// Deletes the channel asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        public override Task DeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this).ContinueWith(t =>
            {
                t.ThrowIfFaulted();

                string operationId = t.Result.Single().Headers[StreamingConstants.OperationIdHeader];

                IOperation operation = AsyncHelper.WaitOperationCompletion(
                    this._cloudMediaContext,
                    operationId,
                    StreamingConstants.DeleteOriginPollInterval); 

                string messageFormat = Resources.ErrorDeleteOriginFailedFormat;
                string message;

                switch (operation.State)
                {
                    case OperationState.Succeeded:
                        return;
                    case OperationState.Failed:
                        message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.Failed, operationId, operation.ErrorMessage);
                        throw new InvalidOperationException(message);
                    default: // can never happen unless state enum is extended
                        message = string.Format(CultureInfo.CurrentCulture, messageFormat, Resources.InInvalidState, operationId, operation.State);
                        throw new InvalidOperationException(message);
                }
            });
        }

        /// <summary>
        /// Sends delete operation to the service and returns. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Operation info that can be used to track the operation.</returns>
        public IOperation SendDeleteOperation()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                throw new InvalidOperationException(Resources.ErrorEntityWithoutId);
            }

            DataServiceContext dataContext = this._cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(EntitySetName, this);
            dataContext.DeleteObject(this);

            var response = dataContext.SaveChanges();

            string operationId = response.Single().Headers[StreamingConstants.OperationIdHeader];

            var result = new OperationData()
            {
                ErrorCode = null,
                ErrorMessage = null,
                Id = operationId,
                State = OperationState.InProgress.ToString(),
            };

            return result;
        }

        /// <summary>
        /// Sends delete operation to the service asynchronously. Use Operations collection to get operation's status.
        /// </summary>
        /// <returns>Task to wait on for operation sending completion.</returns>
        public Task<IOperation> SendDeleteOperationAsync()
        {
            return Task.Factory.StartNew(() => SendDeleteOperation());
        }

        public IOriginMetric GetMetric()
        {
            var uri = new Uri(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "/{0}('{1}')/{2}",
                    OriginMetricBaseCollection.OriginMetricSet,
                    Id,
                    Metric.MetricSet
                    ),
                UriKind.Relative);

            var dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            var metric = dataContext.Execute<OriginMetricData>(uri).SingleOrDefault();

            return metric;
        }

        protected override string EntitySetName { get { return OriginBaseCollection.OriginSet; } }

        private OriginSettings _settings;
    }
}
