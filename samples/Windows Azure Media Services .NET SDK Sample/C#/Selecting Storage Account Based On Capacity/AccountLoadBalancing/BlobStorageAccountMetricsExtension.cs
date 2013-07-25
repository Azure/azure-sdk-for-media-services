using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client.AccountLoadBalancing
{
    public static class BlobStorageAccountMetricsExtension
    {
        private static readonly Lazy<int> RetentionDays = new Lazy<int>(GetRetentionPolicyFromConfig); 

        /// <summary>
        /// Tries the get user data capacity metric for blob service.
        /// </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <param name="fromDate">Date from which retrieve capacity information.</param>
        /// <param name="analyticsEnabled">if set to <c>true</c> if analytics enabled.</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<DateTime,long>> TryGetBlobUserDataCapacityMetric(this CloudStorageAccount cloudStorageAccount, DateTime fromDate, out bool analyticsEnabled)
        {
            
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var serviceProperties = blobClient.GetServiceProperties();
            analyticsEnabled = serviceProperties.Metrics.MetricsLevel == MetricsLevel.Service;

            if (!analyticsEnabled)
            {
                return null;
            }

            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var table = cloudTableClient.GetTableReference("$MetricsCapacityBlob");
            //Selecting 
            TableQuery<MetricsCapacityBlob> query =
                new TableQuery<MetricsCapacityBlob>().Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, (fromDate.ToUniversalTime()).ToString("yyyyMMddTHH00")),
                                                                                      TableOperators.And,
                                                                                      TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "data")));

            
            return table.ExecuteQuery(query).Select(c => new Tuple<DateTime, long>(c.Timestamp.Date, c.Capacity));


        }

        /// <summary>
        /// Checks if $MetricsCapacityBlob table exists in a storage account.
        /// </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        /// <returns></returns>
        public static bool MetricsTableExists(this CloudStorageAccount cloudStorageAccount)
        {
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            var table = cloudTableClient.GetTableReference("$MetricsCapacityBlob");
            return table.Exists();
        }

        /// <summary>
        /// Enables the storage analytics for blob service.
        /// </summary>
        /// <param name="cloudStorageAccount">The cloud storage account.</param>
        public static void EnableBlobStorageAnalytics(this CloudStorageAccount cloudStorageAccount)
        {
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            ServiceProperties serviceProperties = cloudBlobClient.GetServiceProperties();
            serviceProperties.Metrics.MetricsLevel = MetricsLevel.Service;
            serviceProperties.Metrics.RetentionDays = RetentionDays.Value;
            cloudBlobClient.SetServiceProperties(serviceProperties);
        }

        /// <summary>
        /// Gets the retention policy from config.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">RetentionDays stting is missing or has wrong format in App.config</exception>
        private static int GetRetentionPolicyFromConfig()
        {
            string retentionPolicyString = ConfigurationManager.AppSettings["RetentionDays"];
            int retentionPolicy;
            if (string.IsNullOrEmpty("retentionPolicyString") || !int.TryParse(retentionPolicyString, out retentionPolicy))
            {
                throw new ApplicationException("RetentionDays setting is missing or has wrong format in App.config");
            }
            return retentionPolicy;
        }
    }
}