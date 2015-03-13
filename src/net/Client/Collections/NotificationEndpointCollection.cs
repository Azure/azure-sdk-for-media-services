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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public sealed class NotificationEndPointCollection : CloudBaseCollection<INotificationEndPoint>
    {
        /// <summary>
        /// The entity set name for NotificationEndPoints.
        /// </summary>
        internal const string NotificationEndPoints = "NotificationEndPoints";

        internal NotificationEndPointCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            Queryable = MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<INotificationEndPoint, NotificationEndPoint>(NotificationEndPoints);
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

            notificationEndPoint.SetMediaContext(MediaContext);
            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(NotificationEndPoints, notificationEndPoint);

            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(notificationEndPoint))
                .ContinueWith<INotificationEndPoint>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (NotificationEndPoint)t.Result.AsyncState;
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
