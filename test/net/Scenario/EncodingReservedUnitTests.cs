using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using System.Linq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class EncodingReservedUnitTests
    {

        private CloudMediaContext _dataContext;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _dataContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Priority(1)]
        public void ReservedUnitCollectionShouldNotBeNullOrEmpty()
        {
            var encodingReservedUnits = _dataContext.EncodingReservedUnits;
            Assert.IsNotNull(encodingReservedUnits);
            Assert.IsTrue(encodingReservedUnits.Count() == 1);
            Assert.IsTrue(encodingReservedUnits.FirstOrDefault().CurrentReservedUnits <= encodingReservedUnits.FirstOrDefault().MaxReservableUnits);
        }

        [TestMethod]
        [Priority(1)]
        public void BasicReservedUnitTypeShouldbePresentByDefault()
        {
            var a = _dataContext.EncodingReservedUnits.FirstOrDefault();
            var encodingBasicReservedUnitsCount = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).Count();
            Assert.AreEqual(encodingBasicReservedUnitsCount, 1, "Expecting to have atleast one EncodingReservedUnit of type Basic by default");
        }

        [TestMethod]
        [Priority(1)]
        public void StandardReservedUnitTypeShouldNotbePresentByDefault()
        {
            var encodingStandardReservedUnitsCount =
                _dataContext.EncodingReservedUnits.Where(
                    c => c.ReservedUnitType == (int)ReservedUnitType.Standard).Count();
            Assert.AreEqual(encodingStandardReservedUnitsCount, 0,
                "Expecting to have no EncodingReservedUnit of type Standard");
        }

        [TestMethod]
        [Priority(1)]
        public void PremiumReservedUnitTypeShouldNotbePresentByDefault()
        {
            var encodingPremiumReservedUnitsCount = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Premium).Count();
            Assert.AreEqual(encodingPremiumReservedUnitsCount, 0, "Expecting to have no EncodingReservedUnit of type Premium");
        }

        [TestMethod]
        [Priority(1)]
        public void UpdateBasicReservedEncodingUnitToOneRU()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            var initialReservedUnitCount = encodingBasicReservedUnit.CurrentReservedUnits;
            encodingBasicReservedUnit.CurrentReservedUnits ++;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, initialReservedUnitCount+1,
                "Expecting Encoding ReservedUnit to have increased");
            encodingBasicReservedUnit.CurrentReservedUnits--;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, initialReservedUnitCount,
                "Expecting Encoding ReservedUnit to have decreased again");
        }

        [TestMethod]
        [Priority(1)]
        public void UpdateBasicReservedEncodingUnitToMaxRU()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            var initialReservedUnitCount = encodingBasicReservedUnit.CurrentReservedUnits;
            encodingBasicReservedUnit.CurrentReservedUnits = encodingBasicReservedUnit.MaxReservableUnits;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, encodingBasicReservedUnit.MaxReservableUnits,
                "Expecting Encoding ReservedUnit to have increased to Max");
            encodingBasicReservedUnit.CurrentReservedUnits = initialReservedUnitCount;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, initialReservedUnitCount,
                "Expecting Encoding ReservedUnit to have decreased again to initialCount from Max");
        }

        [TestMethod]
        [Priority(1)]
        public void UpdateBasicReservedEncodingUnitToSameNumberOfRU()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            var initialReservedUnitsCount = encodingBasicReservedUnit.CurrentReservedUnits;
            encodingBasicReservedUnit.CurrentReservedUnits = initialReservedUnitsCount;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits,initialReservedUnitsCount,
                "Expecting Encoding ReservedUnit to have increased to Max");
        }

        [TestMethod]
        [Priority(3)]
        public void UpdateBasicToStandardReservedUnitType()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            encodingBasicReservedUnit.ReservedUnitType = (int)ReservedUnitType.Standard;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Standard).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.ReservedUnitType, (int)ReservedUnitType.Standard,
                "Expecting Encoding ReservedUnit to have increased to Max");
            encodingBasicReservedUnit.ReservedUnitType = (int)ReservedUnitType.Basic;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.ReservedUnitType, (int)ReservedUnitType.Basic,
                "Expecting Encoding ReservedUnit to have increased to Max");
        }

        [TestMethod]
        [Priority(3)]
        public void UpdateBasicToPremiumReservedUnitType()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            encodingBasicReservedUnit.ReservedUnitType = (int)ReservedUnitType.Premium;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Premium).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.ReservedUnitType, (int)ReservedUnitType.Premium,
                "Expecting Encoding ReservedUnit to have increased to Max");
            encodingBasicReservedUnit.ReservedUnitType = (int)ReservedUnitType.Basic;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            Assert.AreEqual(encodingBasicReservedUnit.ReservedUnitType, (int)ReservedUnitType.Basic,
                "Expecting Encoding ReservedUnit to have increased to Max");
        }

        [TestMethod]
        [Priority(3)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateMaxRuShouldGiveError()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            var initialMaxReservedUnitCount = encodingBasicReservedUnit.MaxReservableUnits;
            encodingBasicReservedUnit.MaxReservableUnits++;
            encodingBasicReservedUnit.Update();
            encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
        }


        [TestMethod]
        [Priority(1)]
        public void UpdateAsyncBasicReservedEncodingUnitToOneRU()
        {
            var encodingBasicReservedUnit = _dataContext.EncodingReservedUnits.Where(c => c.ReservedUnitType == (int)ReservedUnitType.Basic).FirstOrDefault();
            var initialReservedUnitCount = encodingBasicReservedUnit.CurrentReservedUnits;
            encodingBasicReservedUnit.CurrentReservedUnits++;
            encodingBasicReservedUnit = encodingBasicReservedUnit.UpdateAsync().Result;
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, initialReservedUnitCount + 1,
                "Expecting Encoding ReservedUnit to have increased");
            encodingBasicReservedUnit.CurrentReservedUnits--;
            encodingBasicReservedUnit = encodingBasicReservedUnit.UpdateAsync().Result;
            Assert.AreEqual(encodingBasicReservedUnit.CurrentReservedUnits, initialReservedUnitCount,
                "Expecting Encoding ReservedUnit to have decreased again");
        }
    }
}
