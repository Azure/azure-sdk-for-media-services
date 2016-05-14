//-----------------------------------------------------------------------
// <copyright file="TelemetryStorageFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// An Factory for creating Telemetry Storage Client.
    /// </summary>
    internal static class TelemetryStorageFactory
    {
        private static TelemetryStorage _telemetryStorage;

        /// <summary>
        /// Create an instance of Telemetry Storage Client.
        /// </summary>
        /// <param name="storageCredentials">The storage credentials.</param>
        /// <param name="tableEndPoint">The Uri of Azure table endpoint.</param>
        /// <returns></returns>
        public static TelemetryStorage CreateTelemetryStorage(StorageCredentials storageCredentials, Uri tableEndPoint)
        {
            if (_telemetryStorage == null)
            {
                Interlocked.CompareExchange(ref _telemetryStorage, new TelemetryStorage(storageCredentials, tableEndPoint), null);
            }
            return _telemetryStorage;
        }
    }
}
