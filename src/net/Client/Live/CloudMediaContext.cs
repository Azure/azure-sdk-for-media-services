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

using System;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client.OAuth;
using Microsoft.WindowsAzure.MediaServices.Client.Versioning;
using Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization;
using Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Describes the context from which all entities in the Microsoft WindowsAzure Media Services platform can be accessed.
    /// </summary>
    public partial class CloudMediaContext : MediaContextBase
    {
        private ChannelBaseCollection _channels;
        private ProgramBaseCollection _programs;
        private OriginBaseCollection _origins;
        private OperationBaseCollection _operations;
        private OriginMetricBaseCollection _originMetrics;
        private ChannelMetricBaseCollection _channelMetrics;

        private void InitializeLiveCollections()
        {
            this._channels = new ChannelBaseCollection(this);
            this._programs = new ProgramBaseCollection(this);
            this._origins = new OriginBaseCollection(this);
            this._operations = new OperationBaseCollection(this);
            this._originMetrics = new OriginMetricBaseCollection(this);
            this._channelMetrics = new ChannelMetricBaseCollection(this);
            this._originMetrics = new OriginMetricBaseCollection(this);
            this._channelMetrics = new ChannelMetricBaseCollection(this);
        }


        /// <summary>
        /// Gets the collection of channels in the system.
        /// </summary>
        public override ChannelBaseCollection Channels
        {
            get { return this._channels; }
        }

        /// <summary>
        /// Gets the collection of programs in the system.
        /// </summary>
        public override ProgramBaseCollection Programs
        {
            get { return this._programs; }
        }

        /// <summary>
        /// Gets the collection of origins in the system.
        /// </summary>
        public override OriginBaseCollection Origins
        {
            get { return this._origins; }
        }

        /// <summary>
        /// Gets the collection of operation in the system.
        /// </summary>
        public override OperationBaseCollection Operations
        {
            get { return this._operations; }
        }

        /// <summary>
        /// Gets the collection of origin metrics in the system.
        /// </summary>
        public override OriginMetricBaseCollection OriginMetrics
        {
            get { return this._originMetrics; }
        }

        /// <summary>
        /// Gets the collection of channel metrics in the system.
        /// </summary>
        public override ChannelMetricBaseCollection ChannelMetrics
        {
            get { return this._channelMetrics; }
        }
    }
}
