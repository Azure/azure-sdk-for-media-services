//-----------------------------------------------------------------------
// <copyright file="AssetFilesTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{    
    /// <summary>
    ///This is a test class for StreamingEndpointDataTest and is intended
    ///to contain all StreamingEndpointDataTest Unit Tests
    ///</summary>
    [TestClass]
    public class StreamingEndpointDataTest
    {
        private CloudMediaContext _mediaContext;

        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void SettingsTestSubProperties()
        {
            IStreamingEndpoint target = new StreamingEndpointData();

            var settings = new StreamingEndpointCacheControl
            {
                MaxAge = TimeSpan.FromMinutes(1)
            };

            target.CacheControl = settings;

            Assert.AreEqual(60, target.CacheControl.MaxAge.Value.TotalSeconds);
        }
        
        /// <summary>
        ///A test for Settings
        ///</summary>
        [TestMethod]
        public void CdnSettingsTest()
        {
            var dataContextMock = new Mock<IMediaDataServiceContext>();
            var streamingEndpointMock = new Mock<IStreamingEndpoint>();

            var originCreationOptions = new StreamingEndpointCreationOptions("unittest", 1)
            {
                CdnEnabled = true,
                CdnProfile = "testCdnProfile",
                CdnProvider = CdnProviderType.StandardAkamai,
                StreamingEndpointVersion = new Version("1.0")
            };
            var fakeResponse = new TestMediaDataServiceResponse(new Dictionary<string, string>
            {
                {StreamingConstants.OperationIdHeader, ""}
            })
            {
                AsyncState = new TestStreamingEndpointData(originCreationOptions)
            };
            
            dataContextMock.Setup(ctxt => ctxt.AddObject("StreamingEndpoints", It.IsAny<object>()));
            dataContextMock.Setup(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()))
                .Returns(() => Task<IMediaDataServiceResponse>.Factory.StartNew(() => fakeResponse));
            dataContextMock.Setup(ctxt => ctxt.Execute<OperationData>(It.IsAny<Uri>()))
                .Returns(() => new List<OperationData> {new OperationData {State = OperationState.Succeeded.ToString()}});

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            var actual = _mediaContext.StreamingEndpoints.Create(originCreationOptions);

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(1));
            
            Assert.AreEqual(originCreationOptions.Name, actual.Name);
            Assert.AreEqual(originCreationOptions.ScaleUnits, actual.ScaleUnits);
            Assert.AreEqual(originCreationOptions.CdnEnabled, actual.CdnEnabled);
            Assert.AreEqual(originCreationOptions.CdnProvider.ToString(), actual.CdnProvider);
            Assert.AreEqual(originCreationOptions.CdnProfile, actual.CdnProfile);
            Assert.AreEqual(originCreationOptions.StreamingEndpointVersion.ToString(), actual.StreamingEndpointVersion);
        }
    }
}
