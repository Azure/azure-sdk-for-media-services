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
    public class JobNotificationSubscriptionCollection : IEnumerable<IJobNotificationSubscription>, ICloudMediaContextInit
    {
        private List<JobNotificationSubscription> _jobNotificationSubscriptionList; 
        private CloudMediaContext _cloudMediaContext;

        public JobNotificationSubscriptionCollection()
        {
            _jobNotificationSubscriptionList = new List<JobNotificationSubscription>();
        }

        public int Count
        {
            get { return _jobNotificationSubscriptionList.Count; }
        }

        public void AddNew(NotificationJobState targetJobState, INotificationEndPoint notificationEndPoint)
        {
            JobNotificationSubscription subscription = new JobNotificationSubscription(targetJobState, notificationEndPoint);

            if (_cloudMediaContext != null)
            {
                subscription.InitCloudMediaContext(_cloudMediaContext);
            }

            _jobNotificationSubscriptionList.Add(subscription);
        }

        /// <summary>
        /// This job notification subscription list is for ODataContext serialization.
        /// So it is invisible to external callers.
        /// </summary>
        internal List<JobNotificationSubscription> JobNotificationSubscriptionList
        {
            get { return _jobNotificationSubscriptionList; }

            set
            {
                if (value == null)
                {
                    _jobNotificationSubscriptionList.Clear();
                }
                else
                {
                    _jobNotificationSubscriptionList = value;
                }
            }
        }

        internal void Clear()
        {
            _jobNotificationSubscriptionList.Clear();
        }

        #region ICloudMediaContextInit Members

        /// <summary>
        /// Inits the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            _cloudMediaContext = context;
        }

        #endregion

        #region IEnumerable<IJobNotificationSubscription> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IJobNotificationSubscription> GetEnumerator()
        {
            if (_cloudMediaContext != null)
            {
                foreach (var jobNotificationSubscription in _jobNotificationSubscriptionList)
                {
                    jobNotificationSubscription.InitCloudMediaContext(_cloudMediaContext);
                }
            }

            return _jobNotificationSubscriptionList.ToList<IJobNotificationSubscription>().GetEnumerator();
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
    }
}
