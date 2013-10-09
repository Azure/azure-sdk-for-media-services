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
using System.ComponentModel;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal class NotificationEndPoint : INotificationEndPoint, ICloudMediaContextInit
    {
        private CloudMediaContext _cloudMediaContext;
        private string _id;
        private string _name;
        private NotificationEndPointType _endPointType;
        private string _endPointAddress;

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
        /// Initializes the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            _cloudMediaContext = context;
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
                    throw new ArgumentNullException("value");
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
                    throw new ArgumentNullException("value");
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
                if (endPointTypeValue != (int) (NotificationEndPointType.AzureQueue))
                {
                    throw new InvalidEnumArgumentException("value", endPointTypeValue, typeof(NotificationEndPointType));
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
                    throw new ArgumentNullException("value");
                }

                _endPointAddress = value;
            }
        }

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
            IMediaDataServiceContext dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(NotificationEndPointCollection.NotificationEndPoints, this);
            dataContext.UpdateObject(this);

            return dataContext.SaveChangesAsync(this);
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
            IMediaDataServiceContext dataContext = _cloudMediaContext.DataContextFactory.CreateDataServiceContext();
            dataContext.AttachTo(NotificationEndPointCollection.NotificationEndPoints, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this);
        }
    }
}
