//-----------------------------------------------------------------------
// <copyright file="StorageAccountData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Data.Services.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents Azure storage account in a system
    /// </summary>
    [DataServiceKey("Name")]
    internal class StorageAccountData : IStorageAccount
    {
        /// <summary>
        /// Name of a storage account
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault { get; set; }

        /// <summary>
        /// If Storage Account Metrics are enabled on the storage account, this returns the number of bytes used in the storage account by blob data.
        /// If Storage Account Metrics are not enabled or no data is available, then null is returned.
        /// </summary>
        public long? BytesUsed { get; set; }
    }
}