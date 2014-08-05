// Copyright 2014 Microsoft Corporation
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
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies access control properties on the channel endpoints.
    /// This is the internal class for the communication to the REST and must match the REST metadata.
    /// </summary>
    internal class ChannelAccessControlData
    {
        /// <summary>
        /// The list of IP addresses that are allowed to connect to channel endpoint.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public IPAccessControlData IP { get; set; }

        /// <summary>
        /// Creates an instance of ChannelAccessControlData class.
        /// </summary>
        public ChannelAccessControlData() { }

        /// <summary>
        /// Creates an instance of ChannelAccessControlData class from an instance of ChannelAccessControl.
        /// </summary>
        /// <param name="accessControl">ChannelAccessControl object to copy into newly created instance.</param>
        public ChannelAccessControlData(ChannelAccessControl accessControl)
        {
            if (accessControl == null)
            {
                throw new ArgumentNullException("accessControl");
            }

            if (accessControl.IPAllowList != null)
            {
                IP = new IPAccessControlData
                {
                    Allow = accessControl.IPAllowList
                        .Select(a => a == null ? null : new IPRangeData(a))
                        .ToList()
                };
            }
        }

        /// <summary>
        /// Casts ChannelAccessControlData to ChannelAccessControl.
        /// </summary>
        /// <param name="accessControl">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelAccessControl(ChannelAccessControlData accessControl)
        {
            if (accessControl == null)
            {
                return null;
            }

            var result = new ChannelAccessControl();

            if (accessControl.IP != null && accessControl.IP.Allow != null)
            {
                result.IPAllowList = accessControl.IP.Allow
                    .Select(a => (IPRange) a)
                    .ToList();
            }

            return result;
        }
    }
}
