//-----------------------------------------------------------------------
// <copyright file="JobTemplateData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a JobTemplate that can be used to create Jobs.
    /// </summary>
    /// <seealso cref="IJob.SaveAsTemplate(string)"/>
    [DataServiceKey("Id")]
    internal partial class JobTemplateData : BaseEntity<IJobTemplate>, IJobTemplate
    {
        private const string TaskTemplatesPropertyName = "TaskTemplates";

       
        private ReadOnlyCollection<ITaskTemplate> _taskTemplates;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobTemplateData"/> class.
        /// </summary>
        public JobTemplateData()
        {
            this.TaskTemplates = new List<TaskTemplateData>();
        }

        /// <summary>
        /// Gets or sets the task templates.
        /// </summary>
        /// <value>
        /// The task templates.
        /// </value>
        public List<TaskTemplateData> TaskTemplates { get; set; }

        /// <summary>
        /// Gets a collection of TaskTemplates that compose this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <value>A collection of TaskTemplates composing this <see cref="IJobTemplate"/>.</value>
        ReadOnlyCollection<ITaskTemplate> IJobTemplate.TaskTemplates
        {
            get
            {
                if (this._taskTemplates == null)
                {
                    if (!string.IsNullOrWhiteSpace(this.Id))
                    {
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(JobTemplateBaseCollection.JobTemplateSet, this);
                        dataContext.LoadProperty(this, TaskTemplatesPropertyName);

                        this.ResolveTaskTemplateInputsAndOuputs();
                    }

                    this._taskTemplates = new ReadOnlyCollection<ITaskTemplate>(new List<ITaskTemplate>(this.TaskTemplates));
                }

                return this._taskTemplates;
            }
        }

        /// <summary>
        /// Gets or sets the job template body copied.
        /// </summary>
        /// <value>
        /// The job template body copied.
        /// </value>
        internal string JobTemplateBodyCopied { get; set; }

        /// <summary>
        /// Creates an in-memory copy of this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A copy of this <see cref="IJobTemplate"/>.</returns>
        public IJobTemplate Copy()
        {
            JobTemplateData jobTemplateCopy = new JobTemplateData
            {
                Name = this.Name,
                TemplateType = this.TemplateType,
                NumberofInputAssets = this.NumberofInputAssets,
                JobTemplateBodyCopied = this.JobTemplateBody
            };

            jobTemplateCopy.SetMediaContext(this.GetMediaContext());

            foreach (TaskTemplateData taskTemplate in ((IJobTemplate)this).TaskTemplates.OfType<TaskTemplateData>())
            {
                jobTemplateCopy.TaskTemplates.Add(taskTemplate.Copy());
            }

            return jobTemplateCopy;
        }

        /// <summary>
        /// Asynchronously saves this <see cref="IJobTemplate"/> when created from a copy of an existing <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task SaveAsync()
        {
            if (!string.IsNullOrWhiteSpace(this.Id))
            {
                // The job template was already saved, and there is no current support to update it.
                throw new InvalidOperationException(StringTable.InvalidOperationSaveForSavedJobTemplate);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

            this.InnerSave(dataContext);

            return dataContext
                .SaveChangesAsync(SaveChangesOptions.Batch, this)
                .ContinueWith(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        JobTemplateData data = (JobTemplateData)t.Result.AsyncState;

                        dataContext.CreateQuery<JobTemplateData>(JobTemplateBaseCollection.JobTemplateSet).Where(jt => jt.Id == data.Id).First();
                        dataContext.LoadProperty(data, TaskTemplatesPropertyName);
                    });
        }

        /// <summary>
        /// Saves this <see cref="IJobTemplate"/> when created from a copy of an existing <see cref="IJobTemplate"/>.
        /// </summary>
        public void Save()
        {
            try
            {
                this.SaveAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Asynchronously deletes this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Id))
            {
                // The job template was not saved.
                throw new InvalidOperationException(StringTable.InvalidOperationDeleteForNotSavedJobTemplate);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(JobTemplateBaseCollection.JobTemplateSet, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this);
        }

        /// <summary>
        /// Deletes this <see cref="IJobTemplate"/>.
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

        private static JobTemplateType GetExposedTemplateType(int type)
        {
            return (JobTemplateType)type;
        }

        private static int GetInternalTemplateType(JobTemplateType type)
        {
            return (int)type;
        }

        private static void ProtectTaskConfiguration(TaskTemplateData taskTemplate, ref X509Certificate2 certToUse, IMediaDataServiceContext dataContext)
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
                    certToUse = ContentKeyBaseCollection.GetCertificateToEncryptContentKey(dataContext, ContentKeyType.ConfigurationEncryption);
                }

                // Create a content key object to hold the encryption key.
                ContentKeyData contentKeyData = ContentKeyBaseCollection.CreateConfigurationContentKey(configEncryption, certToUse);
                dataContext.AddObject(ContentKeyCollection.ContentKeySet, contentKeyData);
            }
        }

        private void InnerSave(IMediaDataServiceContext dataContext)
        {
            X509Certificate2 certToUse = null;

            dataContext.AddObject(JobTemplateBaseCollection.JobTemplateSet, this);

            foreach (TaskTemplateData taskTemplate in this.TaskTemplates)
            {
                dataContext.AddRelatedObject(this, TaskTemplatesPropertyName, taskTemplate);

                if (((ITaskTemplate)taskTemplate).Options.HasFlag(TaskOptions.ProtectedConfiguration) && (taskTemplate.Configuration != taskTemplate.ConfigurationCopied))
                {
                    ProtectTaskConfiguration((TaskTemplateData)taskTemplate, ref certToUse, dataContext);
                }
            }

            MatchCollection matches = Regex.Matches(this.JobTemplateBodyCopied, @"taskTemplateId=""nb:ttid:UUID:([a-zA-Z0-9\-]+)""");
            this.JobTemplateBody = this.JobTemplateBodyCopied;
            for (int i = 0; i < matches.Count; i++)
            {
                string taskTemplateId = Guid.NewGuid().ToString();

                this.TaskTemplates[i].Id = string.Concat("nb:ttid:UUID:", taskTemplateId);
                this.JobTemplateBody = this.JobTemplateBody.Replace(matches[i].Groups[1].Value, taskTemplateId);
            }
        }

        private void ResolveTaskTemplateInputsAndOuputs()
        {
            AssetPlaceholderToInstanceResolver assetPlaceholderToInstanceResolver = new AssetPlaceholderToInstanceResolver();

            using (StringReader stringReader = new StringReader(this.JobTemplateBody))
            {
                XElement root = XElement.Load(stringReader);

                foreach (XElement taskBody in root.Elements("taskBody"))
                {
                    List<IAsset> taskTemplateInputs = new List<IAsset>();
                    List<IAsset> taskTemplateOutputs = new List<IAsset>();

                    string taskTemplateId = (string)taskBody.Attribute("taskTemplateId");

                    TaskTemplateData taskTemplate = this.TaskTemplates.Where(t => t.Id == taskTemplateId).Single();

                    foreach (XElement input in taskBody.Elements("inputAsset"))
                    {
                        string inputName = (string)input.Value;
                        IAsset inputAsset = assetPlaceholderToInstanceResolver.CreateOrGetInputAsset(inputName);
                        if (inputAsset != null)
                        {
                            taskTemplateInputs.Add(inputAsset);
                        }
                    }

                    foreach (XElement output in taskBody.Elements("outputAsset"))
                    {
                        string outputName = (string)output.Value;
                        taskTemplateOutputs.Add(assetPlaceholderToInstanceResolver.CreateOrGetOutputAsset(outputName));
                    }

                    taskTemplate.TaskInputs = taskTemplateInputs.ToArray();
                    taskTemplate.TaskOutputs = taskTemplateOutputs.ToArray();
                }
            }
        }
    }
}
