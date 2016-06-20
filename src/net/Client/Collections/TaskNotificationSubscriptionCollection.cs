//-----------------------------------------------------------------------
// <copyright file="JobNotificationSubscriptionCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class TaskNotificationSubscriptionCollection : IEnumerable<ITaskNotificationSubscription>
    {
        private List<TaskNotificationSubscription> _taskNotificationSubscriptionList;
       
        public TaskNotificationSubscriptionCollection()
        {
            _taskNotificationSubscriptionList = new List<TaskNotificationSubscription>();
        }

        public int Count
        {
            get { return _taskNotificationSubscriptionList.Count; }
        }

        public void AddNew(NotificationJobState targetJobState, INotificationEndPoint notificationEndPoint, bool includeTaskProgress)
        {
            TaskNotificationSubscription subscription = new TaskNotificationSubscription(targetJobState, notificationEndPoint, includeTaskProgress);

            if (MediaContext != null)
            {
                subscription.SetMediaContext(MediaContext);
            }

            _taskNotificationSubscriptionList.Add(subscription);
        }

        /// <summary>
        /// This task notification subscription list is for ODataContext serialization.
        /// So it is invisible to external callers.
        /// </summary>
        internal List<TaskNotificationSubscription> TaskNotificationSubscriptionList
        {
            get { return _taskNotificationSubscriptionList; }

            set
            {
                if (value == null)
                {
                    _taskNotificationSubscriptionList.Clear();
                }
                else
                {
                    _taskNotificationSubscriptionList = value;
                }
            }
        }

        internal void Clear()
        {
            _taskNotificationSubscriptionList.Clear();
        }

       

        #region IEnumerable<ITaskNotificationSubscription> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ITaskNotificationSubscription> GetEnumerator()
        {
            if (MediaContext != null)
            {
                foreach (var taskNotificationSubscription in _taskNotificationSubscriptionList)
                {
                    taskNotificationSubscription.SetMediaContext(MediaContext);
                }
            }

            return _taskNotificationSubscriptionList.ToList<ITaskNotificationSubscription>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
       
        public MediaContextBase MediaContext { get; set; }
    }
}
