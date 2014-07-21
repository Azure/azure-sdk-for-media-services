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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Various constants.
    /// </summary>
    public static class StreamingConstants
    {
        internal const string OperationIdHeader = "operation-id";

        internal static readonly TimeSpan CreateChannelPollInterval = TimeSpan.FromSeconds(15);

        internal static readonly TimeSpan DeleteChannelPollInterval = TimeSpan.FromSeconds(5);

        internal static readonly TimeSpan StartChannelPollInterval = TimeSpan.FromMinutes(1);

        internal static readonly TimeSpan StopChannelPollInterval = TimeSpan.FromSeconds(15);

        internal static readonly TimeSpan StartProgramPollInterval = TimeSpan.FromSeconds(2);

        internal static readonly TimeSpan StopProgramPollInterval = TimeSpan.FromSeconds(15);

        /// <summary>
        /// Specifies infinite DVR window for a program.
        /// </summary>
        public static readonly TimeSpan? InfiniteDvrLenth = null;
        
        #region Streaming Endpoint constants
        internal static readonly TimeSpan CreateStreamingEndpointPollInterval = TimeSpan.FromSeconds(15);

        internal static readonly TimeSpan DeleteStreamingEndpointPollInterval = TimeSpan.FromSeconds(5);

        internal static readonly TimeSpan StartStreamingEndpointPollInterval = TimeSpan.FromMinutes(1);

        internal static readonly TimeSpan StopStreamingEndpointPollInterval = TimeSpan.FromSeconds(15);

        internal static readonly TimeSpan ScaleStreamingEndpointPollInterval = TimeSpan.FromSeconds(15);
        #endregion Streaming Endpoint constants
    }
}
