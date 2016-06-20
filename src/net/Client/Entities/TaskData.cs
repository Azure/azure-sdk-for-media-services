//-----------------------------------------------------------------------
// <copyright file="TaskData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Common;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a task within a job in the system.
    /// </summary>
    /// <seealso cref="IJob"/>
    [DataServiceKey("Id")]
    internal partial class TaskData : BaseEntity<ITask>, ITask
    {
        /// <summary>
        /// The set name for tasks.
        /// </summary>
        internal const string TaskSet = "Tasks";

        private const string InputMediaAssetsPropertyName = "InputMediaAssets";
        private const string OutputMediaAssetsPropertyName = "OutputMediaAssets";
       
        private InputAssetCollection<IAsset> _inputMediaAssets;
        private OutputAssetCollection _outputMediaAssets;

        private IJob _parentJob;
        private readonly TaskNotificationSubscriptionCollection _taskNotificationSubscriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskData"/> class.
        /// </summary>
        public TaskData()
        {
            this.Id = string.Empty;
            this.InputMediaAssets = new List<AssetData>();
            this.OutputMediaAssets = new List<AssetData>();
            this.ErrorDetails = new List<ErrorDetail>();
            this.HistoricalEvents = new List<TaskHistoricalEvent>();
            this._taskNotificationSubscriptions = new TaskNotificationSubscriptionCollection();
        }

        /// <summary>
        /// Gets or sets the task inputs.
        /// </summary>
        /// <value>
        /// The task inputs.
        /// </value>
        public IAsset[] TaskInputs { get; set; }

        /// <summary>
        /// Gets or sets the task outputs.
        /// </summary>
        /// <value>
        /// The task outputs.
        /// </value>
        public IAsset[] TaskOutputs { get; set; }

        /// <summary>
        /// Gets or sets the input media assets.
        /// </summary>
        /// <value>
        /// The input media assets.
        /// </value>
        public List<AssetData> InputMediaAssets { get; set; }

        /// <summary>
        /// Gets or sets the output media assets.
        /// </summary>
        /// <value>
        /// The output media assets.
        /// </value>
        public List<AssetData> OutputMediaAssets { get; set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        /// <value>
        /// The error details.
        /// </value>
        public List<ErrorDetail> ErrorDetails { get; set; }

        /// <summary>
        /// Gets or sets the error details.
        /// </summary>
        /// <value>
        /// The error details.
        /// </value>
        public List<TaskHistoricalEvent> HistoricalEvents { get; set; }
            
        /// <summary>
        /// Gets a collection of <see cref="TaskHistoricalEvent"/> objects decribing events associated with task execution.
        /// </summary>
        /// <value>
        /// The historical events.
        /// </value>
        ReadOnlyCollection<TaskHistoricalEvent> ITask.HistoricalEvents
        {
            get
            {
                return this.HistoricalEvents.AsReadOnly();
            }

        }

        #region ITask Members

        /// <summary>
        /// Gets or sets the percentage of completion of the task.
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Gets a collection of <see cref="ErrorDetail"/> objects describing the errors encountered during task execution.
        /// </summary>
        ReadOnlyCollection<ErrorDetail> ITask.ErrorDetails
        {
            get { return this.ErrorDetails.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the collection of input assets for the task.
        /// </summary>
        InputAssetCollection<IAsset> ITask.InputAssets
        {
            get
            {
                if (this._inputMediaAssets == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(TaskSet, this);
                        LoadProperty(dataContext, InputMediaAssetsPropertyName);
                    }

                    this._inputMediaAssets = new InputAssetCollection<IAsset>(this, this.InputMediaAssets);
                }

                return this._inputMediaAssets;
            }
        }

        /// <summary>
        /// Gets the collection of output assets for the task.
        /// </summary>
        OutputAssetCollection ITask.OutputAssets
        {
            get
            {
                if (this._outputMediaAssets == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(TaskSet, this);
                        LoadProperty(dataContext, OutputMediaAssetsPropertyName);
                    }

                    this._outputMediaAssets = new OutputAssetCollection(this, this.OutputMediaAssets, this.GetMediaContext());
                }

                return this._outputMediaAssets;
            }
        }

        /// <summary>
        /// Gets the decrypted configuration data.
        /// </summary>
        /// <returns>A string containing the configuration data. If the data was encrypted, the configuration returned is decrypted.</returns>
        public string GetClearConfiguration()
        {
            TaskOptions options = (TaskOptions)this.Options;

            if (options.HasFlag(TaskOptions.ProtectedConfiguration) && (!string.IsNullOrEmpty(this.EncryptionKeyId)) && (this.GetMediaContext() != null))
            {
                return ConfigurationEncryptionHelper.DecryptConfigurationString(this.GetMediaContext(), this.EncryptionKeyId, this.InitializationVector, this.Configuration);
            }

            return this.Configuration;
        }

        /// <summary>
        /// Gets a collection of Task notification subscription.
        /// </summary>
        TaskNotificationSubscriptionCollection ITask.TaskNotificationSubscriptions
        {
            get
            {
                if (GetMediaContext() != null)
                {
                    _taskNotificationSubscriptions.MediaContext = GetMediaContext();
                }

                return _taskNotificationSubscriptions;
            }
        }

        #endregion

        /// <summary>
        /// sets the job which the task belongs to
        /// </summary>
        public IJob ParentJob
        {
            set { _parentJob = value; }
        }

        /// <summary>
        /// Get the parent Job
        /// </summary>
        /// <returns>The parent job associated with the task</returns>
        public IJob GetParentJob()
        {
            return _parentJob;
        }

        private static JobState GetExposedState(int state)
        {
            return (JobState)state;
        }

        private static TimeSpan GetExposedRunningDuration(double runningDuration)
        {
            return TimeSpan.FromMilliseconds((int)runningDuration);
        }

        private static TaskOptions GetExposedOptions(int options)
        {
            return (TaskOptions)options;
        }

        public List<TaskNotificationSubscription> TaskNotificationSubscriptions
        {
            get { return _taskNotificationSubscriptions.TaskNotificationSubscriptionList; }
            set { _taskNotificationSubscriptions.TaskNotificationSubscriptionList = value; }
        }
    }
}
