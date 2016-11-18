//-----------------------------------------------------------------------
// <copyright file="IStreamingFilter.cs" company="Microsoft">Copyright 2015 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Interface for account level filter
    /// </summary>
    public interface IStreamingFilter
    {
        /// <summary>
        /// Name of filter
        /// </summary>
        string Name { get; }

        /// <summary>
        /// First quality
        /// </summary>
        FirstQuality FirstQuality { get; set; }

        /// <summary>
        /// Presentation time range
        /// </summary>
        PresentationTimeRange PresentationTimeRange { get; set; }

        /// <summary>
        /// Track selection conditions
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<FilterTrackSelectStatement> Tracks { get; set; }

        /// <summary>
        /// Updates this filter instance asynchronously.
        /// </summary>
        /// <returns>Task to wait on for operation completion.</returns>
        Task<IMediaDataServiceResponse> UpdateAsync();

        /// <summary>
        /// Updates this filter instance.
        /// </summary>        
        void Update();

        /// <summary>
        /// Asynchronously revokes the specified filter
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IStreamingFilter&gt;.</returns>
        Task<IMediaDataServiceResponse> DeleteAsync();

        /// <summary>
        /// Deletes the specified filter
        /// </summary>
        void Delete();
    }
}
