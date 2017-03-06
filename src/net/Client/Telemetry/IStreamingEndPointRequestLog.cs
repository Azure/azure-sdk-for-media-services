//-----------------------------------------------------------------------
// <copyright file="IStreamingEndPointRequestLog.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// A Streaming EndPoint request log metric.
    /// </summary>
    public interface IStreamingEndpointRequestLog
    {
        /// <summary>
        /// Gets the partition key of the record.
        /// </summary>
        string PartitionKey { get; }

        /// <summary>
        /// Gets the row key of the record.
        /// </summary>
        string RowKey { get; }

        /// <summary>
        /// Gets the Media Services account ID.
        /// </summary>
        Guid AccountId { get; }

        /// <summary>
        /// Gets the Media Services Streaming Endpoint ID.
        /// </summary>
        Guid StreamingEndpointId { get; }

        /// <summary>
        /// Gets the observed time of the metric.
        /// </summary>
        DateTime ObservedTime { get; }

        /// <summary>
        /// Gets the Streaming Endpoint host name.
        /// </summary>
        string HostName { get; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        int StatusCode { get; }

        /// <summary>
        /// Gets the result code.
        /// </summary>
        string ResultCode { get; }

        /// <summary>
        /// Gets the request count.
        /// </summary>
        int RequestCount { get; }

        /// <summary>
        /// Gets the bytes sent.
        /// </summary>
        long BytesSent { get; }

        /// <summary>
        /// Gets the server latency.
        /// </summary>
        int ServerLatency { get; }

        /// <summary>
        /// Gets the end to end request time.
        /// </summary>
        int EndToEndLatency { get; }
    }
}
