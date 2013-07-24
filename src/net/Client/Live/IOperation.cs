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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes an Operation.
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// Gets unique identifier of the Operation.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Id of the entity associated with this operation.
        /// </summary>
        string TargetEntityId { get; set; }

        /// <summary>
        /// Gets operation error code.
        /// </summary>
        string ErrorCode { get; }

        /// <summary>
        /// Gets description of the error.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets operation state.
        /// </summary>
        OperationState State { get; }
    }
}
