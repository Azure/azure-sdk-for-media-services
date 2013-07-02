// Copyright 2012 Microsoft Corporation
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
using System.Data.Services.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [DataServiceKey("Id")]
    internal class OperationData : IOperation
    {
        /// <summary>
        /// Gets unique identifier of the Operation.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets operation state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets operation error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets description of the error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets operation state.
        /// </summary>
        OperationState IOperation.State
        {
            get
            {
                return (OperationState)Enum.Parse(typeof(OperationState), State, true);
            }
        }
    }

    internal class Operation<T> : OperationData, IOperation<T>
    {
        #region IOperation<T> Members

        public T Target
        {
            get;
            internal set;
        }

        #endregion
    }
}
