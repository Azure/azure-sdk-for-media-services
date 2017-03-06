//-----------------------------------------------------------------------
// <copyright file="DeliveryPolicyData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MediaServices.Client.Telemetry
{
    /// <summary>
    /// Contains the SAS URIs for monitoring data for a specified date.
    /// </summary>
    public class MonitoringSasUri 
    {
        /// <summary>
        /// SAS URIs derived from primary and secondary keys.
        /// </summary>
        public IList<string> SasUris { get; set; }

        /// <summary>
        /// Date for which data is provided.
        /// </summary>
        public DateTime MetricDataDate { get; set; }

        /// <summary>
        /// Expiry date of SAS URIs.
        /// </summary>
        public DateTime SasUriExpiryDate { get; set; }

        /// <summary>
        /// Account Id.
        /// </summary>
        public Guid AccountId { get; set; }
    }
}
