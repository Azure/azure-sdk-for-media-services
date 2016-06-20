//-----------------------------------------------------------------------
// <copyright file="JobData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes a job in the system.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class JobData :BaseEntity<IJob>, IJob
    {
        private const int JobInterval = 2500;
        private const string TaskTemplatesPropertyName = "TaskTemplates";
        private const string InputMediaAssetsPropertyName = "InputMediaAssets";
        private const string OutputMediaAssetsPropertyName = "OutputMediaAssets";
        private const string TasksPropertyName = "Tasks";
        private const string TaskBodyNodeName = "taskBody";
        private const string InputAssetNodeName = "inputAsset";
        private const string OutputAssetNodeName = "outputAsset";
        private const string AssetCreationOptionsAttributeName = "assetCreationOptions";
        private const string AssetFormatOptionAttributeName = "assetFormatOption";
        private const string OutputAssetNameAttributeName = "assetName";
        private const string TaskTemplateIdAttributeName = "taskTemplateId";
        private const string StorageAttributeName = "storageAccountName";


       
        private ReadOnlyCollection<IAsset> _inputMediaAssets;
        private ReadOnlyCollection<IAsset> _outputMediaAssets;
        private readonly JobNotificationSubscriptionCollection _jobNotificationSubscriptions;
        private TaskCollection _tasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobData"/> class.
        /// </summary>
        public JobData()
        {
            this.Tasks = new List<TaskData>();
            this.InputMediaAssets = new List<AssetData>();
            this.OutputMediaAssets = new List<AssetData>();
            this._jobNotificationSubscriptions = new JobNotificationSubscriptionCollection();
        }

        /// <summary>
        /// Occurs when state is changed.
        /// </summary>
        public event EventHandler<JobStateChangedEventArgs> StateChanged;

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
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public List<TaskData> Tasks { get; set; }

        /// <summary>
        /// Gets or sets the job notification subscriptions.
        /// </summary>
        public List<JobNotificationSubscription> JobNotificationSubscriptions
        {
            get { return _jobNotificationSubscriptions.JobNotificationSubscriptionList; }
            set { _jobNotificationSubscriptions.JobNotificationSubscriptionList = value; }
        }

        #region IJob Members

        /// <summary>
        /// Gets the input media assets.
        /// </summary>
        ReadOnlyCollection<IAsset> IJob.InputMediaAssets
        {
            get
            {
                if (this._inputMediaAssets == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(JobBaseCollection.JobSet, this);
                        LoadProperty(dataContext, InputMediaAssetsPropertyName);
                    }

                    this._inputMediaAssets = this.InputMediaAssets.ToList<IAsset>().AsReadOnly();
                }

                return this._inputMediaAssets;
            }
        }

        /// <summary>
        /// Gets the output media assets.
        /// </summary>
        ReadOnlyCollection<IAsset> IJob.OutputMediaAssets
        {
            get
            {
                if (this._outputMediaAssets == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(JobBaseCollection.JobSet, this);
                        LoadProperty(dataContext, OutputMediaAssetsPropertyName);
                    }

                    this._outputMediaAssets = this.OutputMediaAssets.ToList<IAsset>().AsReadOnly();
                }

                return this._outputMediaAssets;
            }
        }

        /// <summary>
        /// Gets the tasks.
        /// </summary>
        TaskCollection IJob.Tasks
        {
            get
            {
                if (this._tasks == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(JobBaseCollection.JobSet, this);
                        LoadProperty(dataContext, TasksPropertyName);
                    }

                    this._tasks = new TaskCollection(this, this.Tasks, this.GetMediaContext());
                }

                return this._tasks;
            }
        }

        /// <summary>
        /// Cancels the async.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task CancelAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job was not submitted yet.
                throw new InvalidOperationException(StringTable.InvalidOperationCancelForNotSubmittedJob);
            }

            Uri uri = new Uri(
                string.Format(CultureInfo.InvariantCulture, "/CancelJob?jobid='{0}'", HttpUtility.UrlEncode(Id)),
                UriKind.Relative);

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.IgnoreResourceNotFoundException = false;
            dataContext.AttachTo(JobBaseCollection.JobSet, this);

            return dataContext
                .ExecuteAsync<string>(uri, this)
                .ContinueWith(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        JobData data = (JobData)t.AsyncState;
                        data.Refresh();
                    });
        }

        /// <summary>
        /// Sends request to cancel a job. After job has been submitted to cancelation system trys to cancel it.
        /// </summary>
        public void Cancel()
        {
            try
            {
                this.CancelAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Deletes asynchronously.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job was not submitted yet.
                throw new InvalidOperationException(StringTable.InvalidOperationDeleteForNotSubmittedJob);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(JobBaseCollection.JobSet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            try
            {
                this.DeleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }


        /// <summary>
        /// Asynchronously updates this job instance.
        /// </summary>
        /// <returns></returns>
        public Task<IJob> UpdateAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job has not been submitted yet
                throw new InvalidOperationException(StringTable.InvalidOperationUpdateForNotSubmittedJob);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(JobBaseCollection.JobSet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                .ContinueWith<IJob>(
                   t =>
                   {
                       t.ThrowIfFaulted();
                       JobData data = (JobData)t.Result.AsyncState;
                       return data;
                   },TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Updates this job instance.
        /// </summary>
        public void Update()
        {
            try
            {
                IJob job = this.UpdateAsync().Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Submits asynchronously.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IJob> SubmitAsync()
        {
            if (!string.IsNullOrWhiteSpace(this.Id))
            {
                // The job was already submitted.
                throw new InvalidOperationException(StringTable.InvalidOperationSubmitForSubmittedJob);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

           
            this.InnerSubmit(dataContext);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(SaveChangesOptions.Batch, this))
                .ContinueWith(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        JobData data = (JobData)t.Result.AsyncState;
                        data.Refresh();
                        return (IJob)data;
                    },TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Submits this instance.
        /// </summary>
        public void Submit()
        {
            try
            {
                IJob job = this.SubmitAsync().Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Gets the execution progress of the task.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task GetExecutionProgressTask(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job was not submitted yet.
                throw new InvalidOperationException(StringTable.InvalidOperationGetExecutionProgressTaskForNotSubmittedJob);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(JobBaseCollection.JobSet, this);

            return Task.Factory.StartNew(
                    t =>
                    {
                        JobData data = (JobData)t;
                        while (!IsFinalJobState(GetExposedState(data.State)))
                        {
                            Thread.Sleep(JobInterval);

                            cancellationToken.ThrowIfCancellationRequested();

                            JobState previousState = GetExposedState(data.State);
                            data.Refresh();

                            if (previousState != GetExposedState(data.State))
                            {
                                this.OnStateChanged(new JobStateChangedEventArgs(previousState, GetExposedState(data.State)));
                            }
                        }
                    },
                    this,
                    cancellationToken);
        }

        /// <summary>
        /// Gets a collection of job notification subscription.
        /// </summary>
        JobNotificationSubscriptionCollection IJob.JobNotificationSubscriptions
        {
            get
            {
                if (GetMediaContext() != null)
                {
                    _jobNotificationSubscriptions.MediaContext = GetMediaContext();
                }

                return _jobNotificationSubscriptions;
            }
        }

        /// <summary>
        /// Saves as template asynchronously.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IJobTemplate> SaveAsTemplateAsync(string templateName)
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job was not submitted yet, so it needs to be verified.
                Verify(this);
            }

            IEnumerable<ITask> tasks = ((IJob)this).Tasks;
            IList<ITaskTemplate> taskTemplates = new List<ITaskTemplate>();

            foreach (ITask task in tasks)
            {
                taskTemplates.Add(this.CreateTaskTemplate(task));
            }

            return this.CreateJobTemplate(templateName, JobTemplateType.AccountLevel, taskTemplates.ToArray());
        }

        /// <summary>
        /// Saves this instance as a job template.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>The job template.</returns>
        public IJobTemplate SaveAsTemplate(string templateName)
        {
            try
            {
                Task<IJobTemplate> task = this.SaveAsTemplateAsync(templateName);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        #endregion

       

        private static JobState GetExposedState(int state)
        {
            return (JobState)state;
        }

        private static TimeSpan GetExposedRunningDuration(double runningDuration)
        {
            return TimeSpan.FromMilliseconds((int)runningDuration);
        }

        private static string CreateTaskBody(AssetNamingSchemeResolver<AssetData, OutputAsset> assetNamingSchemeResolver, IEnumerable<IAsset> inputs, IEnumerable<IAsset> outputs)
        {
            using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlWriterSettings outputSettings = new XmlWriterSettings();
                outputSettings.Encoding = Encoding.UTF8;
                outputSettings.Indent = true;
                XmlWriter taskBody = XmlWriter.Create(stringWriter, outputSettings);
                taskBody.WriteStartDocument();
                taskBody.WriteStartElement(TaskBodyNodeName);

                foreach (IAsset input in inputs)
                {
                    taskBody.WriteStartElement(InputAssetNodeName);
                    taskBody.WriteString(assetNamingSchemeResolver.GetAssetId(input));
                    taskBody.WriteEndElement();
                }

                foreach (IAsset output in outputs)
                {
                    taskBody.WriteStartElement(OutputAssetNodeName);
                    var outputAsset = (OutputAsset)output;

                    if (!assetNamingSchemeResolver.IsExistingOutputAsset(outputAsset))
                    {
                        int options = (int) outputAsset.Options;
                        int formatOption = (int) outputAsset.FormatOption;
                        taskBody.WriteAttributeString(AssetCreationOptionsAttributeName,
                            options.ToString(CultureInfo.InvariantCulture));
                        taskBody.WriteAttributeString(AssetFormatOptionAttributeName,
                            formatOption.ToString(CultureInfo.InvariantCulture));
                        if (!string.IsNullOrEmpty(outputAsset.Name)) // Ignore empty string for the name
                        {
                            taskBody.WriteAttributeString(OutputAssetNameAttributeName, outputAsset.Name);
                        }
                        if (!string.IsNullOrEmpty(outputAsset.StorageAccountName))
                            // Ignore empty string for the storage account
                        {
                            taskBody.WriteAttributeString(StorageAttributeName, outputAsset.StorageAccountName);
                        }
                    }
                    taskBody.WriteString(assetNamingSchemeResolver.GetAssetId(output));
                    taskBody.WriteEndElement();                        

                }

                taskBody.WriteEndDocument();
                taskBody.Flush();

                return stringWriter.ToString();
            }
        }

       

        private static void Verify(IJob job)
        {
            if (job.Tasks.Count == 0)
            {
                throw new ArgumentException(StringTable.EmptyTaskArray);
            }

            bool hasOutputAsset = false;
            foreach (ITask task in job.Tasks)
            {
                if (task.InputAssets.Count == 0)
                {
                    throw new ArgumentException(StringTable.EmptyInputArray);
                }

                if (task.OutputAssets.Count == 0)
                {
                    throw new ArgumentException(StringTable.EmptyOutputArray);
                }

                if (!hasOutputAsset)
                {
                    IAsset output = task.OutputAssets.FirstOrDefault();
                    if (output != null)
                    {
                        hasOutputAsset = true;
                    }
                }
            }

            if (!hasOutputAsset)
            {
                throw new ArgumentException(StringTable.NoPermanentOutputs);
            }
        }

        private static void Verify(ITask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            if (!(task is TaskData))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidTaskType);
            }
        }

        private static void Verify(ITaskTemplate taskTemplate)
        {
            if (taskTemplate == null)
            {
                throw new ArgumentNullException("taskTemplate");
            }

            if (!(taskTemplate is TaskTemplateData))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidTaskType);
            }
        }

        private static string CreateJobTemplateBody(AssetNamingSchemeResolver<AssetData, OutputAsset> assetMap, ITaskTemplate[] taskTemplates)
        {
            using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlWriterSettings outputSettings =
                    new XmlWriterSettings
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true
                    };

                XmlWriter jobTemplateBodyWriter = XmlWriter.Create(stringWriter, outputSettings);
                jobTemplateBodyWriter.WriteStartDocument();

                jobTemplateBodyWriter.WriteStartElement("jobTemplate");

                foreach (ITaskTemplate taskTemplate in taskTemplates)
                {
                    TaskTemplateData taskTemplateData = (TaskTemplateData)taskTemplate;
                    taskTemplateData.NumberofInputAssets = taskTemplateData.TaskInputs.Length;
                    taskTemplateData.NumberofOutputAssets = taskTemplateData.TaskOutputs.Length;

                    string taskTemplateId = string.Empty;
                    string taskTemplateBody = string.Empty;

                    if (!string.IsNullOrWhiteSpace(taskTemplateData.TaskTemplateBody))
                    {
                        // The task template was created using an already submitted job.
                        StringReader stringReader = null;
                        try
                        {
                            stringReader = new StringReader(taskTemplateData.TaskTemplateBody);
                            using (XmlReader taskTemplateBodyReader = XmlReader.Create(stringReader))
                            {
                                stringReader = null;
                                taskTemplateBodyReader.ReadToNextSibling(TaskBodyNodeName);

                                taskTemplateId = taskTemplateBodyReader.GetAttribute(TaskTemplateIdAttributeName);
                                taskTemplateBody = taskTemplateBodyReader.ReadInnerXml();
                            }
                        }
                        finally
                        {
                            if (stringReader != null)
                            {
                                stringReader.Dispose();
                            }
                        }
                    }

                    taskTemplateData.Id = !string.IsNullOrWhiteSpace(taskTemplateId)
                        ? taskTemplateId
                        : string.Concat("nb:ttid:UUID:", Guid.NewGuid());

                    jobTemplateBodyWriter.WriteStartElement(TaskBodyNodeName);
                    jobTemplateBodyWriter.WriteAttributeString(TaskTemplateIdAttributeName, taskTemplateData.Id);

                    if (!string.IsNullOrWhiteSpace(taskTemplateBody))
                    {
                        // The task template was created using an already submitted job.
                        jobTemplateBodyWriter.WriteRaw(taskTemplateBody);
                    }
                    else
                    {
                        foreach (IAsset input in taskTemplateData.TaskInputs)
                        {
                            jobTemplateBodyWriter.WriteStartElement(InputAssetNodeName);
                            jobTemplateBodyWriter.WriteString(assetMap.GetAssetId(input));
                            jobTemplateBodyWriter.WriteEndElement();
                        }

                        foreach (IAsset output in taskTemplateData.TaskOutputs)
                        {
                            jobTemplateBodyWriter.WriteStartElement(OutputAssetNodeName);

                            int options = (int)output.Options;
                            jobTemplateBodyWriter.WriteAttributeString(AssetCreationOptionsAttributeName, options.ToString(CultureInfo.InvariantCulture));

                            if (!String.IsNullOrEmpty(output.StorageAccountName))
                            {
                                jobTemplateBodyWriter.WriteAttributeString(StorageAttributeName, output.StorageAccountName);
                            }

                            jobTemplateBodyWriter.WriteString(assetMap.GetAssetId(output));
                            jobTemplateBodyWriter.WriteEndElement();
                        }
                    }

                    jobTemplateBodyWriter.WriteEndElement();
                }

                jobTemplateBodyWriter.WriteEndDocument();
                jobTemplateBodyWriter.Flush();

                return stringWriter.ToString();
            }
        }

        private static bool IsFinalJobState(JobState jobState)
        {
            return (jobState == JobState.Canceled) || (jobState == JobState.Error) || (jobState == JobState.Finished);
        }

        private void ProtectTaskConfiguration(TaskData task, ref X509Certificate2 certToUse, IMediaDataServiceContext dataContext)
        {
            using (ConfigurationEncryption configEncryption = new ConfigurationEncryption())
            {
                // Update the task with the required data.
                task.Configuration = configEncryption.Encrypt(task.Configuration);
                task.EncryptionKeyId = configEncryption.GetKeyIdentifierAsString();
                task.EncryptionScheme = ConfigurationEncryption.SchemeName;
                task.EncryptionVersion = ConfigurationEncryption.SchemeVersion;
                task.InitializationVector = configEncryption.GetInitializationVectorAsString();

                if (certToUse == null)
                {
                    // Get the certificate to use to encrypt the configuration encryption key.
                    certToUse = ContentKeyBaseCollection.GetCertificateToEncryptContentKey(GetMediaContext(), ContentKeyType.ConfigurationEncryption);
                }

                // Create a content key object to hold the encryption key.
                ContentKeyData contentKeyData = ContentKeyBaseCollection.InitializeConfigurationContentKey(configEncryption, certToUse);
                dataContext.AddObject(ContentKeyBaseCollection.ContentKeySet, contentKeyData);
            }
        }

        private void ProtectTaskConfiguration(TaskTemplateData taskTemplate, ref X509Certificate2 certToUse, IMediaDataServiceContext dataContext)
        {
            using (ConfigurationEncryption configEncryption = new ConfigurationEncryption())
            {
                // Update the task template with the required data.
                taskTemplate.Configuration = configEncryption.Encrypt(taskTemplate.Configuration);
                taskTemplate.EncryptionKeyId = configEncryption.GetKeyIdentifierAsString();
                taskTemplate.EncryptionScheme = ConfigurationEncryption.SchemeName;
                taskTemplate.EncryptionVersion = ConfigurationEncryption.SchemeVersion;
                taskTemplate.InitializationVector = configEncryption.GetInitializationVectorAsString();

                if (certToUse == null)
                {
                    // Get the certificate to use to encrypt the configuration encryption key.
                    certToUse = ContentKeyBaseCollection.GetCertificateToEncryptContentKey(GetMediaContext(), ContentKeyType.ConfigurationEncryption);
                }

                // Create a content key object to hold the encryption key.
                ContentKeyData contentKeyData = ContentKeyBaseCollection.InitializeConfigurationContentKey(configEncryption, certToUse);
                dataContext.AddObject(ContentKeyBaseCollection.ContentKeySet, contentKeyData);
            }
        }

        private void InnerSubmit(IMediaDataServiceContext dataContext)
        {
            if (!string.IsNullOrWhiteSpace(this.TemplateId))
            {

                dataContext.AddObject(JobBaseCollection.JobSet, this);

                foreach (IAsset asset in this.InputMediaAssets)
                {
                    AssetData target = asset as AssetData;
                    if (target == null)
                    {
                        throw new ArgumentException(StringTable.ErrorInputTypeNotSupported);
                    }

                    dataContext.AttachTo(AssetCollection.AssetSet, asset);
                    dataContext.AddLink(this, InputMediaAssetsPropertyName, target);
                }
            }
            else
            {
                X509Certificate2 certToUse = null;
                Verify(this);

                dataContext.AddObject(JobBaseCollection.JobSet, this);

                List<AssetData> inputAssets = new List<AssetData>();
                AssetNamingSchemeResolver<AssetData, OutputAsset> assetNamingSchemeResolver = new AssetNamingSchemeResolver<AssetData, OutputAsset>(inputAssets);

                foreach (ITask task in ((IJob)this).Tasks)
                {
                    Verify(task);
                    TaskData taskData = (TaskData)task;

                    if (task.Options.HasFlag(TaskOptions.ProtectedConfiguration))
                    {
                        ProtectTaskConfiguration(taskData, ref certToUse, dataContext);
                    }

                    taskData.TaskBody = CreateTaskBody(assetNamingSchemeResolver, task.InputAssets.ToArray(), task.OutputAssets.ToArray());
                    taskData.InputMediaAssets.AddRange(task.InputAssets.OfType<AssetData>().ToArray());
                    taskData.OutputMediaAssets.AddRange(
                        task.OutputAssets
                            .OfType<OutputAsset>()
                            .Select(
                                c =>
                                {
                                    AssetData assetData = new AssetData { Name = c.Name, Options = (int)c.Options, AlternateId = c.AlternateId };
                                    assetData.SetMediaContext(this.GetMediaContext());

                                    return assetData;
                                })
                            .ToArray());
                    dataContext.AddRelatedObject(this, TasksPropertyName, taskData);
                }

                foreach (IAsset asset in inputAssets)
                {
                    dataContext.AttachTo(AssetCollection.AssetSet, asset);
                    dataContext.AddLink(this, InputMediaAssetsPropertyName, asset);
                }
            }
        }

        private Task<IJobTemplate> CreateJobTemplate(string templateName, JobTemplateType templateType, params ITaskTemplate[] taskTemplates)
        {
            X509Certificate2 certToUse = null;
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            JobTemplateData jobTemplateData = new JobTemplateData { Name = templateName, TemplateType = (int)templateType };

            jobTemplateData.SetMediaContext(this.GetMediaContext());

            dataContext.AddObject(JobTemplateBaseCollection.JobTemplateSet, jobTemplateData);

            foreach (ITaskTemplate taskTemplate in taskTemplates)
            {
                Verify(taskTemplate);

                dataContext.AddRelatedObject(jobTemplateData, TaskTemplatesPropertyName, taskTemplate);

                if (taskTemplate.Options.HasFlag(TaskOptions.ProtectedConfiguration) && string.IsNullOrWhiteSpace(this.Id))
                {
                    ProtectTaskConfiguration((TaskTemplateData)taskTemplate, ref certToUse, dataContext);
                }
            }

            AssetNamingSchemeResolver<AssetData, OutputAsset> assetIdMap = new AssetNamingSchemeResolver<AssetData, OutputAsset>();

            jobTemplateData.JobTemplateBody = CreateJobTemplateBody(assetIdMap, taskTemplates);

            jobTemplateData.NumberofInputAssets = string.IsNullOrWhiteSpace(this.Id)
                ? assetIdMap.Inputs.Count
                : ((IJob)this).InputMediaAssets.Count;

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(SaveChangesOptions.Batch, jobTemplateData))
                .ContinueWith<IJobTemplate>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        JobTemplateData data = (JobTemplateData)t.Result.AsyncState;
                        IJobTemplate jobTemplateToReturn = this.GetMediaContext().JobTemplates.Where(c => c.Id == data.Id).First();

                        return jobTemplateToReturn;
                    });
        }

        private ITaskTemplate CreateTaskTemplate(ITask task)
        {
            TaskTemplateData taskTemplate = new TaskTemplateData
                {
                    Name = task.Name,
                    MediaProcessorId = task.MediaProcessorId,
                    Configuration = task.Configuration,
                    EncryptionKeyId = task.EncryptionKeyId,
                    EncryptionScheme = task.EncryptionScheme,
                    EncryptionVersion = task.EncryptionVersion,
                    InitializationVector = task.InitializationVector,
                    TaskInputs = task.InputAssets.ToArray(),
                    TaskOutputs = task.OutputAssets.ToArray(),
                    NumberofInputAssets = task.InputAssets.Count,
                    NumberofOutputAssets = task.OutputAssets.Count,
                    Options = (int)task.Options,
                    TaskTemplateBody = task.TaskBody
                };

            taskTemplate.SetMediaContext(this.GetMediaContext());

            return taskTemplate;
        }

        private void OnStateChanged(JobStateChangedEventArgs jobStateChangedEventArgs)
        {
            EventHandler<JobStateChangedEventArgs> stateChangedEvent = this.StateChanged;
            if (stateChangedEvent != null)
            {
                stateChangedEvent(this, jobStateChangedEventArgs);
            }
        }

        public void Refresh()
        {            
            InvalidateCollections();

            var refreshed = (JobData)GetMediaContext().Jobs.Where(c => c.Id == this.Id).FirstOrDefault();
            
            //it is possible that job has been cancelled and deleted while we are refreshing
            if (refreshed != null)
            {
                this.Created = refreshed.Created;
                this.EndTime= refreshed.EndTime;
                this.LastModified = refreshed.LastModified;
                this.Name = refreshed.Name;
                this.Priority = refreshed.Priority;
                this.RunningDuration = refreshed.RunningDuration;
                this.StartTime = refreshed.StartTime;
                this.State = refreshed.State;
                this.TemplateId = refreshed.TemplateId;
                this.JobNotificationSubscriptions = refreshed.JobNotificationSubscriptions;
            }
        }

        //Invalidate collections to force them to be reloaded from server
        private void InvalidateCollections()
        {
            this._tasks = null;
            this._inputMediaAssets = null;
            this._outputMediaAssets = null;
            this.Tasks.Clear();
            this.InputMediaAssets.Clear();
            this.OutputMediaAssets.Clear();
        }
    }
}
