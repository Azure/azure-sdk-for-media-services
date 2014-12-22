using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
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

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void ShouldReturnAtLeastOneAccount()
        {
            var account = _dataContext.StorageAccounts.FirstOrDefault();
            Assert.IsNotNull(account);
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void ShouldReturnOnlyOneDefaultAccount()
        {
            var defaultAccountsCount = _dataContext.StorageAccounts.Where(c => c.IsDefault == true).Count();
            Assert.AreEqual(1, defaultAccountsCount, "Expecting to have only one default storage account");
        }

        /// <summary>
        /// This test can be executed if you media account has multiple storage accounts
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Ignore]
        public void ShouldReturnNoneDefaultAccount()
        {
            var noneDefaultAccount = _dataContext.StorageAccounts.Where(c => c.IsDefault == false).FirstOrDefault();
            Assert.IsNotNull(noneDefaultAccount, "Expecting to have at least one none default account");
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void DeaultStorageAccountonContextShouldBeSameAsFromQuery()
        {
            var defaultAccountsCount = _dataContext.StorageAccounts.Where(c => c.IsDefault == true).FirstOrDefault();
            Assert.IsNotNull(defaultAccountsCount);
            Assert.AreEqual(_dataContext.DefaultStorageAccount.Name, defaultAccountsCount.Name);
        }

        [TestMethod]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void ShouldNotReturnAnyRecordsForNonExistingAccount()
        {
            var account = _dataContext.StorageAccounts.FirstOrDefault();
            Assert.IsNotNull(account);

            account = _dataContext.StorageAccounts.Where(c => c.Name == Guid.NewGuid().ToString()).FirstOrDefault();
            Assert.IsNull(account);
        }

    }
}