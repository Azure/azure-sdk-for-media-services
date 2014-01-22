using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

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
        public void ShouldVerifyParametersInLocatorCreate()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("Test", TimeSpan.FromDays(1), AccessPermissions.Read);
            bool assetNullverified = false;
            try
            {
                _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, null, policy);
            }
            catch (ArgumentNullException)
            {
                assetNullverified = true;
            }

            bool policyNullverified = false;
            try
            {
                _mediaContext.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, null);
            }
            catch (ArgumentNullException)
            {
                policyNullverified = true;
            }

            Assert.IsTrue(assetNullverified, "Expecting ArgumentNullException passing asset as null");
            Assert.IsTrue(policyNullverified, "Expecting ArgumentNullException passing policy as null");
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
            locator.UpdateAsync(DateTime.UtcNow.AddDays(5));
            locator.Delete();
            Assert.IsNull(_mediaContext.Locators.Where(c => c.Id == locator.Id).FirstOrDefault());

            var sas = _mediaContext.Locators.CreateSasLocator(asset, policy);
            sas.Delete();

            sas = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy).Result;
            sas.Delete();
            var sasAsync = _mediaContext.Locators.CreateSasLocatorAsync(asset, policy,DateTime.UtcNow,"Name").Result;
            sasAsync.DeleteAsync().Wait();

        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateAccessPolicyInCreateLocatorAsync()
        {
            var asset = _mediaContext.Assets.FirstOrDefault();
            Assert.IsNotNull(asset);
            var policy = _mediaContext.AccessPolicies.FirstOrDefault();
            Assert.IsNotNull(policy);
            _mediaContext.Locators.CreateLocatorAsync(LocatorType.None, asset, null);
        }
        [TestMethod]
        public void CreateLocatorAsync()
        {
            var asset = _mediaContext.Assets.FirstOrDefault();
            Assert.IsNotNull(asset);
            var policy = _mediaContext.AccessPolicies.FirstOrDefault();
            Assert.IsNotNull(policy);
            _mediaContext.Locators.CreateLocatorAsync(LocatorType.None, asset, policy);
        }

        [TestMethod]
        public void AccessPolicyCRUD()
        {
            IAccessPolicy policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            Assert.AreEqual(AccessPermissions.Read, policy.Permissions);
            Assert.AreEqual(TimeSpan.FromDays(1), policy.Duration);
            Assert.AreEqual("AccessPolicyCRUD", policy.Name);
            policy.Delete();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(1), AccessPermissions.Read);
            policy.DeleteAsync();
            policy = _mediaContext.AccessPolicies.Create("AccessPolicyCRUD", TimeSpan.FromDays(-1), AccessPermissions.Read);
            policy.DeleteAsync();
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