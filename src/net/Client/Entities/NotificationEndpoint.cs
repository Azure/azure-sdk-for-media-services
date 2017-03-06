//-----------------------------------------------------------------------
// <copyright file="NotificationEndPoint.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.ComponentModel;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.Telemetry;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class NotificationEndPoint : BaseEntity<INotificationEndPoint>, INotificationEndPoint
    {
        private string _id;
        private string _name;
        private string _endPointAddress;
        private NotificationEndPointType _endPointType;
        private NotificationEndPointCredentialType _endPointCredentialType;
        private ProtectionKeyType _protectionKeyType;

        /// <summary>
        /// Don't allow the customer to create a default NotificationEndPoint object.
        /// </summary>
        public NotificationEndPoint()
        {
        }

        public NotificationEndPoint(string name, NotificationEndPointType endPointType, string endPointAddress)
        {
            _id = string.Empty;
            Name = name;
            EndPointType = (int)endPointType;
            EndPointAddress = endPointAddress;
        }

        public NotificationEndPoint(string id, string name, NotificationEndPointType endPointType, string endPointAddress)
        {
            Id = id;
            Name = name;
            EndPointType = (int)endPointType;
            EndPointAddress = endPointAddress;
        }

        /// <summary>
        /// Unique identifier of notification endpoint
        /// </summary>
        public string Id
        {
            get { return _id; }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("Id");
                }

                _id = value;
            }
        }

        /// <summary>
        /// Display name of the notification endpoint.
        /// 
        /// Name can be an empty or null string.
        /// </summary>
        public string Name
        {
            get { return _name; }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Name");
                }

                _name = value;
            }
        }

        /// <summary>
        /// Type of notification endpoint.
        /// 
        /// Media service uses this type to determine how to write the notification to the endpoint. 
        /// </summary>
        NotificationEndPointType INotificationEndPoint.EndPointType
        {
            get { return _endPointType; }
        }

        public int EndPointType
        {
            get { return (int)_endPointType; }

            set
            {
                int endPointTypeValue = value;
                if (!Enum.IsDefined(typeof(NotificationEndPointType), endPointTypeValue))
                {
                    throw new InvalidEnumArgumentException("EndPointType", endPointTypeValue, typeof(NotificationEndPointType));
                }

                _endPointType = (NotificationEndPointType)endPointTypeValue;
            }
        }

        /// <summary>
        /// Address of endPoint. The constraints of this value is determined by the endpoint type.
        /// </summary>
        public string EndPointAddress
        {
            get { return _endPointAddress; }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("EndPointAddress");
                }

                _endPointAddress = value;
            }
        }

        /// <summary>
        /// Type of notification endpoint Credential.
        /// Media service uses this type to determine how to write the notification to the endpoint. 
        /// </summary>
        NotificationEndPointCredentialType INotificationEndPoint.CredentialType
        {
            get { return _endPointCredentialType; }
        }

        /// <summary>
        /// Set the Credential type for notification endpoint 
        /// </summary>
        public int CredentialType
        {
            get { return (int)_endPointCredentialType; }

            set
            {
                int endPointCredentialTypeValue = value;
                if (endPointCredentialTypeValue != (int)(NotificationEndPointCredentialType.None) && endPointCredentialTypeValue != (int)(NotificationEndPointCredentialType.SigningKey))
                {
                    throw new InvalidEnumArgumentException("CredentialType", endPointCredentialTypeValue, typeof(NotificationEndPointCredentialType));
                }

                _endPointCredentialType = (NotificationEndPointCredentialType)endPointCredentialTypeValue;
            }
        }

        /// <summary>
        /// The encrypted endPoint credential.
        /// </summary>
        public string EncryptedEndPointCredential { get; set; }

        /// <summary>
        /// The protection key type.
        /// </summary>
        public int ProtectionKeyType 
        {
            get { return (int)_protectionKeyType; }
            set
            {
                int protectionKeyTypeValue = value;
                if (protectionKeyTypeValue != (int)(Client.ProtectionKeyType.X509CertificateThumbprint) )
                {
                    throw new InvalidEnumArgumentException("ProtectionKeyType", protectionKeyTypeValue, typeof(ProtectionKeyType));
                }

                _protectionKeyType = (ProtectionKeyType)protectionKeyTypeValue;
        }
        }

        /// <summary>
        /// The Protection Key Id.
        /// </summary>
        public string ProtectionKeyId { get; set; }

        /// <summary>
        /// Update the notification endpoint object.
        /// </summary>
        public void Update()
        {
            try
            {
                UpdateAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Update the notification endpoint object in asynchronous mode.
        /// </summary>
        /// <returns>Task of updating the notification endpoint.</returns>
        public Task UpdateAsync()
        {
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(NotificationEndPointCollection.NotificationEndPoints, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Delete this instance of notification endpoint object.
        /// </summary>
        public void Delete()
        {
            try
            {
                DeleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Delete this instance of notification endpoint object in asynchronous mode.
        /// </summary>
        /// <returns>Task of deleting the notification endpoint.</returns>
        public Task DeleteAsync()
        {
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(NotificationEndPointCollection.NotificationEndPoints, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Returns monitoring data for notification endpoint.
        /// </summary>
        /// <param name="start">Requested start date in UTC.</param>
        /// <param name="end">Requested end date in UTC.</param>
        /// <returns>Returns a list of <see cref="MonitoringSasUri"/>.</returns>
        public IEnumerable<MonitoringSasUri> GetMonitoringSasUris(DateTime start, DateTime end)
        {
            try
            {
                return GetMonitoringSasUrisAsync(start, end).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }       
        }

        /// <summary>
        /// Returns monitoring data for notification endpoint in asynchronous mode.
        /// </summary>
        /// <param name="start">Requested start date in UTC.</param>
        /// <param name="end">Requested end date in UTC.</param>
        /// <returns>Task of retrieving list of <see cref="MonitoringSasUri"/> .</returns>
        public Task<IEnumerable<MonitoringSasUri>> GetMonitoringSasUrisAsync(DateTime start, DateTime end)
        {
            if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Start and end dates must be in UTC format.");
            }

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

            Uri mointoringSasRequestUri = new Uri(string.Format(CultureInfo.InvariantCulture, "/NotificationEndPoints('{0}')/GetMonitoringSasUris", Id), UriKind.Relative);

            var parameters = new OperationParameter[]
            {
                new BodyOperationParameter("monitoringStartDate", start),
                new BodyOperationParameter("monitoringEndDate", end)
            };

            return dataContext.ExecuteAsync<MonitoringSasUri>(
                requestUri: mointoringSasRequestUri,
                httpMethod: "POST",
                singleResult: false,
                parameters: parameters);
        }
    }
}
