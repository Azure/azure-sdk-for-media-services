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
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{

    internal class TaskNotificationSubscription : BaseEntity<ITaskNotificationSubscription>, ITaskNotificationSubscription
    {
        private int _targetTaskState;
        private INotificationEndPoint _notificationEndPoint;

        public TaskNotificationSubscription()
        {
            _targetTaskState = 0;
            _notificationEndPoint = null;
            IncludeTaskProgress = false;
        }

        public TaskNotificationSubscription(NotificationJobState targetTaskState, INotificationEndPoint notificationEndPoint, bool includeTaskProgress)
        {
            TargetTaskState = (int)targetTaskState;

            if (notificationEndPoint == null)
            {
                throw new ArgumentNullException("notificationEndPoint");
            }

            _notificationEndPoint = notificationEndPoint;
            NotificationEndPointId = _notificationEndPoint.Id;
            IncludeTaskProgress = includeTaskProgress;
        }

        public int TargetTaskState
        {
            get { return _targetTaskState; }

            set
            {
                int targetTaskStateValue = value;
                if (targetTaskStateValue != (int)NotificationJobState.FinalStatesOnly &&
                    targetTaskStateValue != (int)NotificationJobState.All)
                {
                    throw new InvalidEnumArgumentException("value", targetTaskStateValue, typeof(NotificationJobState));
                }

                _targetTaskState = value;
            }
        }

        public string NotificationEndPointId { get; set; }

        public bool IncludeTaskProgress { get; set; }

        #region IJobNotificationSubscription Members

        NotificationJobState ITaskNotificationSubscription.TargetTaskState
        {
            get { return (NotificationJobState)_targetTaskState; }
        }

        INotificationEndPoint ITaskNotificationSubscription.NotificationEndPoint
        {
            get
            {
                if (_notificationEndPoint == null && GetMediaContext() != null)
                {
                    if (!string.IsNullOrWhiteSpace(NotificationEndPointId))
                    {
                        var notificationEndPoint = GetMediaContext().NotificationEndPoints.Where(n => n.Id == NotificationEndPointId).SingleOrDefault();
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

    }
}
