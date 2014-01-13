//-----------------------------------------------------------------------
// <copyright file="JobCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IJob"/>.
    /// </summary>
    public class JobBaseCollection : CloudBaseCollection<IJob>
    {
        /// <summary>
        /// The name of the job set.
        /// </summary>
        internal const string JobSet = "Jobs";

        /// <summary>
        /// Initializes a new instance of the <see cref="JobBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal JobBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            this.Queryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IJob, JobData>(JobSet);
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="name">Job name.</param>
        /// <returns>An <see cref="IJob"/>.</returns>
        public IJob Create(string name)
        {
            return this.Create(name, 0);
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="name">The job name.</param>
        /// <param name="priority">The job priority.</param>
        /// <returns>An <see cref="IJob"/>.</returns>
        public IJob Create(string name, int priority)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            JobData job = new JobData { Name = name, Priority = priority };
            job.SetMediaContext(this.MediaContext);

            return job;
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobTemplate">The job template.</param>
        /// <param name="inputMediaAssets">The input media assets.</param>
        /// <returns>The new job.</returns>
        public IJob Create(string name, IJobTemplate jobTemplate, IEnumerable<IAsset> inputMediaAssets)
        {
            return this.Create(name, jobTemplate, inputMediaAssets, 0);
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jobTemplate">The job template.</param>
        /// <param name="inputMediaAssets">The input media assets.</param>
        /// <param name="priority">The priority.</param>
        /// <returns>The new job.</returns>
        public IJob Create(string name, IJobTemplate jobTemplate, IEnumerable<IAsset> inputMediaAssets, int priority)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (jobTemplate == null)
            {
                throw new ArgumentNullException("jobTemplate");
            }

            if (inputMediaAssets == null)
            {
                throw new ArgumentNullException("inputMediaAssets");
            }

            if (jobTemplate.NumberofInputAssets != inputMediaAssets.Count())
            {
                throw new ArgumentException(StringTable.ErrorInvalidNumberOfInputs);
            }

            List<AssetData> inputAssets = new List<AssetData>();
            foreach (IAsset asset in inputMediaAssets)
            {
                AssetData target = asset as AssetData;
                if (target == null)
                {
                    throw new ArgumentException(StringTable.ErrorInputTypeNotSupported);
                }

                inputAssets.Add(target);
            }

            JobData job = new JobData();

            job.SetMediaContext(this.MediaContext);
            job.Priority = priority;
            job.Name = name;
            job.TemplateId = jobTemplate.Id;
            job.InputMediaAssets = inputAssets;

            return job;
        }
    }
}
