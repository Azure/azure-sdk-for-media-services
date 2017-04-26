//-----------------------------------------------------------------------
// <copyright file="RentalAndLeaseKeyType.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.FairPlay
{
    /// <summary>
    /// Rental and Lease Key Types
    /// </summary>
    public enum RentalAndLeaseKeyType : uint
    {
        /// <summary>
        /// Key duration is not specified.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Content key can be persisted with unlimited validity duration.
        /// </summary>
        PersistentUnlimited = 0x3df2d9fbU,

        /// <summary>
        /// Content key can be persisted, and it’s validity duration is limited to the “Rental Duration” value.
        /// </summary>
        PersistentLimited = 0x18f06048U,
    }
}
