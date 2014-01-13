//-----------------------------------------------------------------------
// <copyright file="IJobTemplate.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a JobTemplate that can be used to create Jobs.
    /// </summary>
    /// <seealso cref="IJob.SaveAsTemplate(string)"/>
    public partial interface IJobTemplate
    {
        /// <summary>
        /// Gets a collection of TaskTemplates that compose this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <value>A collection of TaskTemplates composing this <see cref="IJobTemplate"/>.</value>
        ReadOnlyCollection<ITaskTemplate> TaskTemplates { get; }

        /// <summary>
        /// Creates an in-memory copy of this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A copy of this <see cref="IJobTemplate"/>.</returns>
        IJobTemplate Copy();

        /// <summary>
        /// Asynchronously saves this <see cref="IJobTemplate"/> when created from a copy of an existing <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task SaveAsync();

        /// <summary>
        /// Saves this <see cref="IJobTemplate"/> when created from a copy of an existing <see cref="IJobTemplate"/>.
        /// </summary>
        void Save();

        /// <summary>
        /// Asynchronously deletes this <see cref="IJobTemplate"/>.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        Task DeleteAsync();

        /// <summary>
        /// Deletes this <see cref="IJobTemplate"/>.
        /// </summary>
        void Delete();
    }
}
