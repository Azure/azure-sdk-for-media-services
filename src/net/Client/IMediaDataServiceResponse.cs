//-----------------------------------------------------------------------
// <copyright file="AssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections;
using System.Collections.Generic;
using System.Data.Services.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public interface IMediaDataServiceResponse : IEnumerable<OperationResponse>, IEnumerable
    {
        /// <summary>
        /// The headers from an HTTP response associated with a batch request.
        /// </summary>
        IDictionary<string, string> BatchHeaders { get; }

        /// <summary>
        /// The status code from an HTTP response associated with a batch request.
        /// </summary>
        int BatchStatusCode { get; }

        /// <summary>
        /// Gets a Boolean value that indicates whether the response contains multiple results.
        /// </summary>
        bool IsBatchResponse { get; }

        /// <summary>
        /// Preserves async state destroyed by retry mechanism.
        /// </summary>
        object AsyncState { get; set; }
    }
}
