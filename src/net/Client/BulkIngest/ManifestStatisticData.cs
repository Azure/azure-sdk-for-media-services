//-----------------------------------------------------------------------
// <copyright file="IngestIngestManifestStatistics.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represent static information about manifest
    /// </summary>
    public class IngestIngestManifestStatistics:IIngestManifestStatistics
    {

        /// <summary>
        /// Amount of pending files which has not been uploaded and processed by a system
        /// </summary>
        /// <value>
        /// The pending files count.
        /// </value>
        public int PendingFilesCount { get; set; }

        /// <summary>
        /// Amount of uploaded and processed files
        /// </summary>
        /// <value>
        /// The finished files count.
        /// </value>
        public int FinishedFilesCount { get; set; }

        /// <summary>
        /// Amount of files where error has been detected
        /// </summary>
        /// <value>
        /// The error files count.
        /// </value>
        public int ErrorFilesCount { get; set; }

        /// <summary>
        /// Contains error string associated with files which has not be uploaded and processed successfully
        /// </summary>
        /// <value>
        /// The error files details.
        /// </value>
        public string ErrorFilesDetails { get; set; }
    }
}