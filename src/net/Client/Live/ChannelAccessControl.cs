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

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Specifies access control properties on the channel endpoints.
    /// This is the public class exposed to SDK interfaces and used by users
    /// </summary>
    public class ChannelAccessControl
    {
        /// <summary>
        /// The list of IP addresses that are allowed to connect to channel endpoint.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public IList<IPAddress> IPAllowList { get; set; }
    }

    /// <summary>
    /// Specifies access control properties on the channel endpoints.
    /// This is the internal class for the communication to the REST and must match the REST metadata.
    /// </summary>
    internal class ChannelServiceAccessControl
    {
        /// <summary>
        /// The list of IP addresses that are allowed to connect to channel endpoint.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public IList<ServiceIPAddress> IPAllowList { get; set; }

        /// <summary>
        /// Creates an instance of StreamingEndpointServiceAccessControl class.
        /// </summary>
        public ChannelServiceAccessControl() { }

        /// <summary>
        /// Creates an instance of ChannelServiceAccessControl class from an instance of ChannelAccessControl.
        /// </summary>
        /// <param name="accessControl">ChannelAccessControl object to copy into newly created instance.</param>
        public ChannelServiceAccessControl(ChannelAccessControl accessControl)
        {
            if (accessControl == null) return;

            if (accessControl.IPAllowList != null)
            {
                IPAllowList = new List<ServiceIPAddress>(accessControl.IPAllowList.Count);

                foreach (var ipAddress in accessControl.IPAllowList)
                {
                    IPAllowList.Add(new ServiceIPAddress(ipAddress));
                }
            }
        }
        
        /// <summary>
        /// Casts ChannelServiceAccessControl to ChannelAccessControl.
        /// </summary>
        /// <param name="accessControl">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator ChannelAccessControl(ChannelServiceAccessControl accessControl)
        {
            if (accessControl == null)
            {
                return null;
            }

            var result = new ChannelAccessControl();

            if (accessControl.IPAllowList != null)
            {
                result.IPAllowList = new List<IPAddress>(accessControl.IPAllowList.Count);

                foreach (var ipAddress in accessControl.IPAllowList)
                {
                    result.IPAllowList.Add((IPAddress) ipAddress);
                }
            }

            return result;
        }
    }
}
