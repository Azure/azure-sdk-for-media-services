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

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an IP Address
    /// This is the internal class for the communication to the REST and must match the REST metadata
    /// </summary>
    /// ReSharper disable once InconsistentNaming
    internal class IPAddressData
    {
        /// <summary>
        /// Gets or sets a friendly name for this IP address.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The IP address represented by this instance.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The subnet mask prefix length (see CIDR notation).
        /// </summary>
        public int? SubnetPrefixLength { get; set; }

        /// <summary>
        /// Creates an instance of IPAddressData class.
        /// </summary>
        public IPAddressData() { }

        /// <summary>
        /// Creates an instance of IPAddressData class from an instance of IPAddress.
        /// </summary>
        /// <param name="ipAddress">IP address to copy into newly created instance.</param>
        public IPAddressData(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                throw new ArgumentNullException("ipAddress");
            }

            Name = ipAddress.Name;
            SubnetPrefixLength = ipAddress.SubnetPrefixLength;

            if (ipAddress.Address != null)
            {
                Address = ipAddress.Address.ToString();
            }
        }

        /// <summary>
        /// Casts IPAddressData to IPAddress.
        /// </summary>
        /// <param name="ipAddress">Object to cast.</param>
        /// <returns>Casted object.</returns>
        public static explicit operator IPAddress(IPAddressData ipAddress)
        {
            if (ipAddress == null)
            {
                return null;
            }

            var result = new IPAddress { Name = ipAddress.Name, SubnetPrefixLength = ipAddress.SubnetPrefixLength };

            System.Net.IPAddress address;
            if (!string.IsNullOrEmpty(ipAddress.Address) &&
                System.Net.IPAddress.TryParse(ipAddress.Address, out address))
            {
                result.Address = address;
            }

            return result;
        }
    }
}
