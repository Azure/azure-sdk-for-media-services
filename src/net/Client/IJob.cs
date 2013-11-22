//-----------------------------------------------------------------------
// <copyright file="IJob.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a job in the system.
    /// </summary>
    public partial interface IJob
    {
        /// <summary>
        /// Occurs when a file download progresses.
        /// </summary>
        event EventHandler<JobStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets a collection of Asset Identifiers that are inputs to the Job.
        /// </summary>
        /// <value>A collection of Asset Identifiers.</value>
        ReadOnlyCollection<IAsset> InputMediaAssets { get; }

        /// <summary>
        /// Gets a collection of Asset Identifiers that are outputs of the Job.
        /// </summary>
        /// <value>A collection of Asset Identifiers.</value>
        ReadOnlyCollection<IAsset> OutputMediaAssets { get; }

        /// <summary>
        /// Gets a collection of Tasks that compose the Job.
        /// </summary>
        /// <value>A Enumerable of Tasks.</value>
        TaskCollection Tasks { get; }

        /// <summary>
        /// Asynchronously sends request to cancel a job.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task CancelAsync();

        /// <summary>
        /// Sends request to cancel a job.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Asynchronously deletes this job instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes this job instance.
        /// </summary>
        void Delete();


        /// <summary>
        /// Asynchronously updates this job instance.
        /// </summary>
        /// <returns></returns>
        Task<IJob> UpdateAsync();

        /// <summary>
        /// Updates this job instance.
        /// </summary>
        void Update();


        /// <summary>
        /// Asynchronously submits this job instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task<IJob> SubmitAsync();

        /// <summary>
        /// Submits this job instance.
        /// </summary>
        void Submit();

        /// <summary>
        /// Returns a new <see cref="System.Threading.Tasks.Task"/> to monitor the job state. The <see cref="System.Threading.Tasks.Task"/> finishes when the job finishes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> that monitors the job state.</returns>
        Task GetExecutionProgressTask(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a collection of job notification subscription.
        /// </summary>
        JobNotificationSubscriptionCollection JobNotificationSubscriptions { get; }
        
        /// <summary>
        /// Saves this job instance as a job template.
        /// </summary>
        /// <param name="templateName">The job template name.</param>
        /// <returns>An <see cref="IJobTemplate"/>.</returns>
        IJobTemplate SaveAsTemplate(string templateName);

        /// <summary>
        /// Asynchronously saves this job instance as a job template.
        /// </summary>
        /// <param name="templateName">The job template name.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IJobTemplate&gt;.</returns>
        Task<IJobTemplate> SaveAsTemplateAsync(string templateName);
        
        /// <summary>
        ///Force entity and underlying properties to be refreshed  
        /// </summary>
        void Refresh();
    }
}
