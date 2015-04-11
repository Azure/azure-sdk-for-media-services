//-----------------------------------------------------------------------
// <copyright file="ServicePointModifier.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal static class ServicePointModifier
    {
        private const int DefaultConnectionLimitMultiplier = 8;
        private const int MaxConnectionLimit = 30;
        private static readonly TimeSpan DefaultConnectionLeaseTimeout = TimeSpan.FromMinutes(5);

 	    public static int DefaultConnectionLimit()
        {
            return Math.Min(MaxConnectionLimit, Environment.ProcessorCount * DefaultConnectionLimitMultiplier);
        }

        public static void SetConnectionPropertiesForSmallPayloads(
            Uri uri,
            int connectionLimit = default(int),
            TimeSpan connectionLeaseTimeout = default(TimeSpan))
        {
            SetConnectionPropertiesForSmallPayloads(
                ServicePointManager.FindServicePoint(uri),
                connectionLimit,
                connectionLeaseTimeout);
        }

        public static void SetConnectionPropertiesForSmallPayloads(
            ServicePoint servicePoint,
            int connectionLimit = default(int),
            TimeSpan connectionLeaseTimeout = default(TimeSpan))
        {
            if (servicePoint == null)
            {
                throw new ArgumentNullException("servicePoint");
            }
            if (connectionLimit == default(int))
            {
                connectionLimit = Environment.ProcessorCount*DefaultConnectionLimitMultiplier;
            }
            if (connectionLeaseTimeout == default(TimeSpan))
            {
                connectionLeaseTimeout = DefaultConnectionLeaseTimeout;
            }

            servicePoint.ConnectionLimit = connectionLimit;
            servicePoint.ConnectionLeaseTimeout = (int)connectionLeaseTimeout.TotalMilliseconds;
            servicePoint.MaxIdleTime = (int)connectionLeaseTimeout.TotalMilliseconds;
            servicePoint.UseNagleAlgorithm = false;
        }
    }
}
