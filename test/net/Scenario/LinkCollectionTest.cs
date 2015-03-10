//-----------------------------------------------------------------------
// <copyright file="LinkCollectionTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    /// <summary>
    ///This is a test class for LinkCollectionTest and is intended
    ///to contain all LinkCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LinkCollectionTest
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod()]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void LinkCollectionTestInsertRetry()
        {
            var data = new AssetData { };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);
            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var target = new LinkCollection<IAsset, AssetData>(dataContextMock.Object, data, "", new IAsset[] { });

            target.Add(data);

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod()]
        [Priority(0)]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [TestCategory("Bvt")]
        public void LinkCollectionTestRemoveRetry()
        {
            var data = new AssetData { };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);
            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var target = new LinkCollection<IAsset, AssetData>(dataContextMock.Object, data, "", new IAsset[] { data });

            target.RemoveAt(0);

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }
    }
}
