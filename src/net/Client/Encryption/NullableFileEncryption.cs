//-----------------------------------------------------------------------
// <copyright file="NullableFileEncryption.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Provides a no-op file encryption.
    /// </summary>
    internal sealed class NullableFileEncryption : IDisposable
    {
        private FileEncryption _fileEncryption;

        /// <summary>
        /// Gets the file encryption.
        /// </summary>
        public FileEncryption FileEncryption
        {
            get { return this._fileEncryption; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has file encryption.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has file encryption; otherwise, <c>false</c>.
        /// </value>
        public bool HasFileEncryption
        {
            get { return this._fileEncryption != null; }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Init()
        {
            if (!this.HasFileEncryption)
            {
                this._fileEncryption = new FileEncryption();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._fileEncryption != null)
            {
                this._fileEncryption.Dispose();
            }
        }
    }
}
