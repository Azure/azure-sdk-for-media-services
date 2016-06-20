//-----------------------------------------------------------------------
// <copyright file="TaskCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// A collection of tasks.
    /// </summary>
    public class TaskCollection : IEnumerable<ITask>
    {
        private readonly IList<ITask> _tasks;
        private readonly IJob _job;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCollection"/> class.
        /// </summary>
        public TaskCollection()
        {
            this._tasks = new List<ITask>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCollection"/> class.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="tasks">The tasks.</param>
        /// <param name="mediaContext">The <seealso cref="MediaContextBase"/> instance.</param>
        internal TaskCollection(IJob job, IEnumerable<ITask> tasks, MediaContextBase mediaContext)
        {
            this._tasks = new List<ITask>(tasks);
            this._job = job;
            this.MediaContext = mediaContext;
        }

        /// <summary>
        /// Gets the count of elements in collection.
        /// </summary>
        public int Count
        {
            get { return this._tasks.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return !string.IsNullOrWhiteSpace(this._job.Id); }
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.WindowsAzure.MediaServices.Client.ITask"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The task.</returns>
        public ITask this[int index]
        {
            get { return this._tasks[index]; }
        }

        #region IEnumerable<ITask> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ITask> GetEnumerator()
        {
            return this._tasks.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Adds a new task.
        /// </summary>
        /// <param name="taskName">The task name.</param>
        /// <param name="mediaProcessor">The media processor id.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        /// <returns>The new task.</returns>
        public ITask AddNew(string taskName, IMediaProcessor mediaProcessor, string configuration, TaskOptions options)
        {
            if (taskName == null)
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "taskName"));
            }

            if (taskName.Length == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNullOrEmpty, "taskName"));
            }

            if (mediaProcessor == null)
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNull, "mediaProcessor"));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorArgCannotBeNull, "configuration"));
            }

            this.CheckIfJobIsPersistedAndThrowNotSupported();

            var task = new TaskData
                           {
                               Name = taskName,
                               Configuration = configuration,
                               MediaProcessorId = mediaProcessor.Id,
                               Options = (int)options,
                               ParentJob = _job
                           };

            task.SetMediaContext(MediaContext);

            this._tasks.Add(task);

            return task;
        }

        /// <summary>
        /// Adds a new task.
        /// </summary>
        /// <param name="taskName">The task name.</param>
        /// <param name="mediaProcessor">The media processor.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="options">The options.</param>
        /// <param name="parentTask">The parent task.</param>
        /// <returns>The new task.</returns>
        public ITask AddNew(string taskName, IMediaProcessor mediaProcessor, string configuration, TaskOptions options, ITask parentTask)
        {
            if (parentTask == null)
            {
                throw new ArgumentNullException("parentTask");
            }

            var task = this.AddNew(taskName, mediaProcessor, configuration, options);

            foreach (IAsset outputAsset in parentTask.OutputAssets)
            {
                task.InputAssets.Add(outputAsset);
            }

            return task;
        }

        /// <summary>
        /// Checks if job is persisted and throw not supported exception.
        /// </summary>
        private void CheckIfJobIsPersistedAndThrowNotSupported()
        {
            if (this.IsReadOnly)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, StringTable.ErrorReadOnlyCollectionToSubmittedTask, "Tasks"));
            }
        }

        public MediaContextBase MediaContext { get; set; }
    }
}
