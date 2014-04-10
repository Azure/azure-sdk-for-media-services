using System;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    internal static class ServicePointModifier
    {
        private const int DefaultConnectionLimitMultiplier = 8;
        private static readonly TimeSpan DefaultConnectionLeaseTimeout = TimeSpan.FromMinutes(5);

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
