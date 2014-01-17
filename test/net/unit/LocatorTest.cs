using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class LocatorTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void LocatorCRUD()
        {
            var stubed = _mediaContext.Locators.FirstOrDefault();
            Assert.IsNotNull(stubed);
            Assert.IsNotNull(stubed.Asset);
            Assert.IsNotNull(stubed.AccessPolicy);

            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, policy);
            Assert.IsNotNull(locator.AccessPolicy);
            Assert.IsNotNull(locator.Asset);
            locator.Update(DateTime.UtcNow.AddDays(5));
            locator.Update(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(5));
            locator.Delete();
            Assert.IsNull(_mediaContext.Locators.Where(c => c.Id == locator.Id).FirstOrDefault());

            var sas = _mediaContext.Locators.CreateSasLocator(asset, policy);
            sas.Delete();

            var sasAsync = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy,DateTime.UtcNow,"Name").Result;
            sasAsync.DeleteAsync().Wait();



        }

        [TestMethod]
        [ExpectedException(typeof(System.InvalidOperationException))]
        public void OnlyOriginLocatorCanBeUpdated()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            ILocator locator = _mediaContext.Locators.CreateLocator(LocatorType.Sas, asset, policy);
            locator.Update(DateTime.UtcNow.AddDays(5));

        }
    }
}