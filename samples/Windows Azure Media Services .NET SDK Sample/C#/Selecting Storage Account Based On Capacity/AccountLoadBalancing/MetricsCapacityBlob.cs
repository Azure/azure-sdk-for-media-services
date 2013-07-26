using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.WindowsAzure.MediaServices.Client.AccountLoadBalancing
{
    public class MetricsCapacityBlob : TableEntity
    {
        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <value>
        /// The amount of storage used by the storage account’s Blob service, in bytes.
        /// </value>
        public long Capacity { get; set; }
        
        /// <summary>
        /// Gets or sets the container count.
        /// </summary>
        /// <value>
        /// The number of blob containers in the storage account’s Blob service.
        /// </value>
        public long ContainerCount { get; set; }
        
        /// <summary>
        /// Gets or sets the object count.
        /// </summary>
        /// <value>
        /// The number of committed and uncommitted block or page blobs in the storage account’s Blob service.
        /// </value>
        public long ObjectCount { get; set; }
    }
}