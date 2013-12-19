using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    class TestQueryErrorDetectionStrategy : QueryErrorDetectionStrategy
    {
        public bool CheckIsTransientTest(Exception ex)
        {
 	         return CheckIsTransient(ex);
        }
    }

    [TestClass]
    public class QueryErrorDetectionStrategyTest
    {
        [TestMethod]
        public void QueryErrorDetectionStrategyTestGeneralException()
        {
            bool actual = new TestQueryErrorDetectionStrategy().CheckIsTransientTest(new Exception());

            Assert.IsFalse(actual);
        }
        
        [TestMethod]
        public void QueryErrorDetectionStrategyTestDataServiceQueryException()
        {
            var exception = new System.Data.Services.Client.DataServiceQueryException();

            bool actual = new TestQueryErrorDetectionStrategy().CheckIsTransientTest(exception);

            Assert.IsFalse(actual);
        }
    }
}
