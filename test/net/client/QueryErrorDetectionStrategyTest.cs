//-----------------------------------------------------------------------
// <copyright file="QueryErrorDetectionStrategyTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
