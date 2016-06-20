//-----------------------------------------------------------------------
// <copyright file="IJobNotificationSubscription.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Defines the notification endpoint task state changes and whether to include task progress for which to be notified.
    /// </summary>
    public partial interface ITaskNotificationSubscription
    {
        /// <summary>
        /// The state changes for which the subscriber is interested in receiving notifications.
        /// </summary>
        NotificationJobState TargetTaskState { get; }

        /// <summary>
        /// Endpoint that subscriber receives notification of job state.
        /// </summary>
        INotificationEndPoint NotificationEndPoint { get; }

        /// <summary>
        /// Boolean to control if including the task progress or not
        /// </summary>
        bool IncludeTaskProgress { get; }
    }
}
