//-----------------------------------------------------------------------
// <copyright file="NotificationEndPointCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public sealed class NotificationEndPointCollection : CloudBaseCollection<INotificationEndPoint>
    {
        /// <summary>
        /// The entity set name for NotificationEndPoints.
        /// </summary>
        internal const string NotificationEndPoints = "NotificationEndPoints";

        private readonly CloudMediaContext _cloudMediaContext;

        internal NotificationEndPointCollection(CloudMediaContext cloudMediaContext)
        {
            _cloudMediaContext = cloudMediaContext;
            DataContextFactory = _cloudMediaContext.DataContextFactory;
            Queryable = DataContextFactory.CreateDataServiceContext().CreateQuery<NotificationEndPoint>(NotificationEndPoints);
        }

        /// <summary>
        /// Create a notification endpoint object in asynchronous mode.
        /// </summary>
        /// <param name="name">Name of notification endpoint</param>
        /// <param name="endPointType">Notification endpoint type</param>
        /// <param name="endPointAddress">Notification endpoint address</param>
        /// <returns>Task of creating notification endpoint.</returns>
        public Task<INotificationEndPoint> CreateAsync(string name, NotificationEndPointType endPointType,
                                                       string endPointAddress)
        {
            NotificationEndPoint notificationEndPoint = new NotificationEndPoint
            {
                Name = name,
                EndPointType = (int)endPointType,
                EndPointAddress = endPointAddress
            };

            notificationEndPoint.InitCloudMediaContext(_cloudMediaContext);
            DataServiceContext dataContext = DataContextFactory.CreateDataServiceContext();
            dataContext.AddObject(NotificationEndPoints, notificationEndPoint);

            return dataContext
                .SaveChangesAsync(notificationEndPoint)
                .ContinueWith<INotificationEndPoint>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (NotificationEndPoint)t.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Create a notification endpoint object.
        /// </summary>
        /// <param name="name">Name of notification endpoint</param>
        /// <param name="endPointType">Notification endpoint type</param>
        /// <param name="endPointAddress">Notification endpoint address</param>
        /// <returns>Notification endpoint object</returns>
        public INotificationEndPoint Create(string name, NotificationEndPointType endPointType, string endPointAddress)
        {
            try
            {
                Task<INotificationEndPoint> task = CreateAsync(name, endPointType, endPointAddress);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }
    }
}
