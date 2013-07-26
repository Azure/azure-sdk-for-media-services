using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.MediaServices.Client.AccountLoadBalancing;
using Microsoft.WindowsAzure.Storage;

namespace SDK.Client.Samples.LoadBalancing
{
    [TestClass]
    public class StorageAccountTests
    {

        private CloudMediaContext _dataContext;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }
        /// <summary>
        /// Enables the analytics for storage accounts and create load balanced asset.
        /// </summary>
        [TestMethod]
        public void EnableAnalyticsForStorageAccountsAndCreateLoadBalancedAsset()
        {

            //Initilize mapping between CloudStorageAccount and IStorageAccount
            Dictionary<CloudStorageAccount, IStorageAccount> accountsMapping = StorageAccountConfigHelper.GetStorageAccountMappingFromConfig(_dataContext);

            //Setup step which will be run once to enable storage analytics
            //Please note that it will take time for capacity metric data to appear in storage account after configuration
            foreach (var storageAccount in accountsMapping.Keys)
            {
                storageAccount.EnableBlobStorageAnalytics();
            }
            var selectionStrategy = new LeastCapacityOrDefaultAccountStrategy(accountsMapping.Keys);
            Dictionary<CloudStorageAccount, AccountSelectionStrategyStatus> selectionResults;
            var inputStorageAccount = selectionStrategy.SelectAccountForInputAsset(out selectionResults);
            
            Assert.AreEqual(selectionResults.Count,accountsMapping.Count);
            
            if (inputStorageAccount != null)
            {
                Assert.AreEqual(selectionResults[inputStorageAccount], AccountSelectionStrategyStatus.Selected);
            }

            IStorageAccount mediaStorageAccount = null;

            //In case we didn't find any storage account with available analytics information we are using default storage account
            mediaStorageAccount = inputStorageAccount != null ? accountsMapping[inputStorageAccount] : _dataContext.DefaultStorageAccount;
            var asset = _dataContext.Assets.Create("LoadBalancedAsset", mediaStorageAccount.Name, AssetCreationOptions.None);

        }

        
    }
}
