//-----------------------------------------------------------------------
// <copyright file="CollectionQueryTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class CollectionQueryTests
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        public void QueryStorageAccounts()
        {
            //checking stubbed data
            Assert.IsNotNull(_mediaContext.StorageAccounts.Where(c => c.IsDefault).FirstOrDefault());
            //Should not return for non existing items
            Assert.IsNull(_mediaContext.StorageAccounts.Where(c => c.Name == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        

        //TODO: Move to separate file if we have more media processor tests
        [TestMethod]
        public void QueryMediaprocessors()
        {
            //We should have at least one for job testing
            IMediaProcessor firstOrDefault = _mediaContext.MediaProcessors.FirstOrDefault();
            Assert.IsNotNull(firstOrDefault);
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Id));
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Name));
            Assert.IsFalse(String.IsNullOrEmpty(firstOrDefault.Version));
            Assert.IsNull(_mediaContext.MediaProcessors.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

        //TODO: Move to separate file if we have more job templates tests
        [TestMethod]
        public void QueryJobTemplates()
        {
            //We should have at least one for job testing
            Assert.IsNull(_mediaContext.JobTemplates.FirstOrDefault());
            Assert.IsNull(_mediaContext.JobTemplates.Where(c => c.Id == Guid.NewGuid().ToString()).FirstOrDefault());
        }

       
    }
}
