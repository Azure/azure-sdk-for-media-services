//-----------------------------------------------------------------------
// <copyright file="IStorageAccount.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    ///     Represents Azure storage account in a system
    /// </summary>
    public interface IStorageAccount
    {
        /// <summary>
        ///     Name of a storage account
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        bool IsDefault { get; set; }
    }
}