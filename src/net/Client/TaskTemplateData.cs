//-----------------------------------------------------------------------
// <copyright file="TaskTemplateData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.ObjectModel;
using System.Data.Services.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a TaskTemplate.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class TaskTemplateData : ITaskTemplate, ICloudMediaContextInit
    {
        private CloudMediaContext _cloudMediaContext;

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
        /// Gets a collection of input assets to the templated Task.
        /// </summary>
        /// <value>A collection of Task input Assets.</value>
        ReadOnlyCollection<IAsset> ITaskTemplate.TaskInputs
        {
            get
            {
                return new ReadOnlyCollection<IAsset>(this.TaskInputs);
            }
        }

        /// <summary>
        /// Gets a collection of output assets to the templated Task.
        /// </summary>
        /// <value></value>
        ReadOnlyCollection<IAsset> ITaskTemplate.TaskOutputs
        {
            get
            {
                return new ReadOnlyCollection<IAsset>(this.TaskOutputs);
            }
        }

        /// <summary>
        /// Gets or sets the task template body.
        /// </summary>
        /// <value>
        /// The task template body.
        /// </value>
        internal string TaskTemplateBody { get; set; }

        /// <summary>
        /// Gets or sets the configuration copied.
        /// </summary>
        /// <value>
        /// The configuration copied.
        /// </value>
        internal string ConfigurationCopied { get; set; }

        /// <summary>
        /// Initializes the cloud media context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            this._cloudMediaContext = context;
        }

        /// <summary>
        /// Decrypts an encrypted task configuration.
        /// </summary>
        /// <returns>The decrypted task configuration if it was encrypted; otherwise the <see cref="Configuration"/>.</returns>
        /// <seealso cref="ITaskTemplate.EncryptionKeyId"/>
        public string GetClearConfiguration()
        {
            TaskOptions options = (TaskOptions)Options;

            if (options.HasFlag(TaskOptions.ProtectedConfiguration) && (!string.IsNullOrEmpty(this.EncryptionKeyId)) && (this._cloudMediaContext != null))
            {
                return ConfigurationEncryptionHelper.DecryptConfigurationString(this._cloudMediaContext, this.EncryptionKeyId, this.InitializationVector, this.Configuration);
            }

            return this.Configuration;
        }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>The task template copy.</returns>
        internal TaskTemplateData Copy()
        {
            TaskTemplateData templateData = new TaskTemplateData
            {
                Name = this.Name,
                MediaProcessorId = this.MediaProcessorId,
                Configuration = this.Configuration,
                ConfigurationCopied = this.Configuration,
                EncryptionKeyId = this.EncryptionKeyId,
                EncryptionScheme = this.EncryptionScheme,
                EncryptionVersion = this.EncryptionVersion,
                InitializationVector = this.InitializationVector,
                TaskInputs = this.TaskInputs,
                TaskOutputs = this.TaskOutputs,
                NumberofInputAssets = this.NumberofInputAssets,
                NumberofOutputAssets = this.NumberofOutputAssets,
                Options = (int)this.Options,
                TaskTemplateBody = this.TaskTemplateBody
            };

            templateData.InitCloudMediaContext(this._cloudMediaContext);

            return templateData;
        }

        private static TaskOptions GetExposedOptions(int options)
        {
            return (TaskOptions)options;
        }
    }
}
