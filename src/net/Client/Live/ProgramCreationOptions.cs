// Copyright 2014 Microsoft Corporation
// 
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

using System;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// The options to create a channel program, which contains
    /// all parameters and settings to configure the program.
    /// </summary>
    public class ProgramCreationOptions
    {
        /// <summary>
        /// Name of the program.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the program.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The archive window length.
        /// </summary>
        public TimeSpan ArchiveWindowLength { get; set; }

        /// <summary>
        /// Id of the asset for storing channel content.
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Gets the streaming manifest name. 
        /// </summary>
        public string ManifestName { get; set; }

        /// <summary>
        /// Create a ProgramCreationOptions object
        /// </summary>
        public ProgramCreationOptions() { }

        /// <summary>
        /// Create a ProgramCreationOptions object
        /// </summary>
        /// <param name="name">name of the program</param>
        /// <param name="archiveWindowLength">archive window length</param>
        /// <param name="assetId">Id of the asset for storing channel content</param>
        internal ProgramCreationOptions(string name, TimeSpan archiveWindowLength, string assetId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (archiveWindowLength <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("archiveWindowLength");
            }

            if (string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException("assetId");
            }

            Name = name;
            ArchiveWindowLength = archiveWindowLength;
            AssetId = assetId;
        }
    }
}
