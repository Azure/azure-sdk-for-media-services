//-----------------------------------------------------------------------
// <copyright file="TestMediaServicesClassFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.UnitTests
{
    [TestClass]
    public class AssetUnitTest
    {
        private CloudMediaContext _mediaContext;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = GetMediaDataServiceContextForUnitTests();
        }

        public static CloudMediaContext GetMediaDataServiceContextForUnitTests()
        {
            CloudMediaContext mediaContext = new TestCloudMediaContext(new Uri("http://contoso.com"), new MediaServicesCredentials("", ""));
            mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(new TestCloudMediaDataContext(mediaContext));
            return mediaContext;
        }

        [TestMethod]
        public void AssetCreateWithEmptyFile()
        {
            IAsset asset = _mediaContext.Assets.Create("Test", AssetCreationOptions.None);
            IAssetFile file = asset.AssetFiles.Create("test");
            Assert.IsNotNull(asset);
        }

        [TestMethod]
        public void AssetCreateAsync()
        {
            Task<IAsset> assetTask = _mediaContext.Assets.CreateAsync("Test", AssetCreationOptions.None, CancellationToken.None);
            IAsset asset = assetTask.Result;
            Assert.IsNotNull(asset);
            IAsset refreshed = _mediaContext.Assets.Where(c => c.Id == asset.Id).FirstOrDefault();
            Assert.IsNotNull(refreshed);
        }

        [TestMethod]
        public void AssetCreateAsyncStorageEncrypted()
        {
            Task<IAsset> assetTask = _mediaContext.Assets.CreateAsync("Test", AssetCreationOptions.StorageEncrypted, CancellationToken.None);
            IAsset asset = assetTask.Result;
            Assert.IsNotNull(asset);
        }

        [TestMethod]
        public void AssetSelectAll()
        {
            List<IAsset> asset = _mediaContext.Assets.ToList();
            Assert.IsNotNull(asset);
        }

       
    }
}