//-----------------------------------------------------------------------
// <copyright file="JobNotificationSubscription.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

    internal class JobNotificationSubscription : IJobNotificationSubscription
    {
        private int _targetJobState;
        private INotificationEndPoint _notificationEndPoint;

        public JobNotificationSubscription()
        {
            _targetJobState = 0;
            _notificationEndPoint = null;
        }

        public JobNotificationSubscription(NotificationJobState targetJobState, INotificationEndPoint notificationEndPoint)
        {
            TargetJobState = (int)targetJobState;

            if (notificationEndPoint == null)
            {
                throw new ArgumentNullException("notificationEndPoint");
            }

            _notificationEndPoint = notificationEndPoint;
            NotificationEndPointId = _notificationEndPoint.Id;
        }

        public int TargetJobState
        {
            get { return _targetJobState; }

            set
            {
                int targetJobStateValue = value;
                if (targetJobStateValue != (int)NotificationJobState.FinalStatesOnly &&
                    targetJobStateValue != (int)NotificationJobState.All)
                {
                    throw new InvalidEnumArgumentException("value", targetJobStateValue, typeof(NotificationEndPointType));
                }

                _targetJobState = value;
            }
        }

        public string NotificationEndPointId { get; set; }

        #region IJobNotificationSubscription Members

        NotificationJobState IJobNotificationSubscription.TargetJobState
        {
            get { return (NotificationJobState)_targetJobState; }
        }

        INotificationEndPoint IJobNotificationSubscription.NotificationEndPoint
        {
            get
            {
                if (_notificationEndPoint == null && MediaContext != null)
                {
                    if (!string.IsNullOrWhiteSpace(NotificationEndPointId))
                    {
                        IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                        var notificationEndPoint = dataContext.CreateQuery<NotificationEndPoint>(NotificationEndPointCollection.NotificationEndPoints).Where(n => n.Id == NotificationEndPointId).Single();
                        if (notificationEndPoint != null)
                        {
                            _notificationEndPoint = notificationEndPoint;
                        }
                    }
                }

                return _notificationEndPoint;
            }
        }

        #endregion


       public MediaContextBase MediaContext { get; set; }
    }
}
