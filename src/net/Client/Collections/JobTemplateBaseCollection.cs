//-----------------------------------------------------------------------
// <copyright file="JobTemplateCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IJobTemplate"/>.
    /// </summary>
    public class JobTemplateBaseCollection : CloudBaseCollection<IJobTemplate>
    {
        /// <summary>
        /// The name of the job template set.
        /// </summary>
        internal const string JobTemplateSet = "JobTemplates";

        /// <summary>
        /// Initializes a new instance of the <see cref="JobTemplateBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal JobTemplateBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            this.Queryable = cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IJobTemplate, JobTemplateData>(JobTemplateSet);
        }
    }
}
