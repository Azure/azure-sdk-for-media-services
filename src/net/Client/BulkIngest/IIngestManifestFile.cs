//-----------------------------------------------------------------------
// <copyright file="IIngestManifestFile.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents manifest file
    /// </summary>
    public partial interface IIngestManifestFile
    {
        /// <summary>
        /// Deletes the manifest asset file asynchronously.
        /// </summary>
        Task DeleteAsync();

        /// <summary>
        /// Deletes manifest asset fils synchronously.
        /// </summary>
        void Delete();
    }
}