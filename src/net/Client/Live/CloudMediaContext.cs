//-----------------------------------------------------------------------
// <copyright file="CloudMediaContext.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Threading;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the context from which all entities in the Microsoft WindowsAzure Media Services platform can be accessed.
    /// </summary>
    public partial class CloudMediaContext : MediaContextBase
    {
        private ChannelBaseCollection _channels;
        private ProgramBaseCollection _programs;
        private StreamingEndpointBaseCollection _streamingEndpoints;
        private OperationBaseCollection _operations;

        /// <summary>
        /// Gets the collection of channels in the system.
        /// </summary>
        public override ChannelBaseCollection Channels
        {
            get
            {
                if (_channels == null)
                {
                    Interlocked.CompareExchange(ref _channels, new ChannelBaseCollection(this), null);
                }
                return _channels;

            }
        }

        /// <summary>
        /// Gets the collection of programs in the system.
        /// </summary>
        public override ProgramBaseCollection Programs
        {
            get
            {
                if (_programs == null)
                {
                    Interlocked.CompareExchange(ref _programs, new ProgramBaseCollection(this), null);
                }
                return _programs;
            }
        }

        /// <summary>
        /// Gets the collection of streaming endpoints in the system.
        /// </summary>
        public override StreamingEndpointBaseCollection StreamingEndpoints
        {
            get
            {
                if (_streamingEndpoints == null)
                {
                    Interlocked.CompareExchange(ref _streamingEndpoints, new StreamingEndpointBaseCollection(this), null);
                }
                return _streamingEndpoints;
            }
        }

        /// <summary>
        /// Gets the collection of operation in the system.
        /// </summary>
        public override OperationBaseCollection Operations
        {
            get
            {
                if (_operations == null)
                {
                    Interlocked.CompareExchange(ref _operations, new OperationBaseCollection(this), null);
                }
                return _operations;
            }
        }
    }
}
