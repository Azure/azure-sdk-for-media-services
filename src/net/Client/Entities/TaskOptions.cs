//-----------------------------------------------------------------------
// <copyright file="TaskOptions.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies the options for creating Tasks.
    /// </summary>
    [Flags]
    public enum TaskOptions
    {
        /// <summary>
        /// Specifies no creation options.
        /// </summary>
        /// <remarks>This is the default value.</remarks>
        None = 0x0,

        /// <summary>
        /// Specifies that the Task's configuration should be encrypted.
        /// </summary>
        /// <seealso cref="ITask.EncryptionKeyId"/>
        ProtectedConfiguration = 0x1,
                
        /// <summary>
        /// Specifies that the output asset will not be deleted even if the task failed
        /// </summary>
        DoNotDeleteOutputAssetOnFailure = 0x2,

        /// <summary>
        /// Specifies that the task will not be cancelled even if the corresponding job failed
        /// </summary>
        DoNotCancelOnJobFailure = 0x4
    }
}
