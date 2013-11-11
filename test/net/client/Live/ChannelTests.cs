//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Helpers;
using System.Net;
using Moq;
using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ChannelTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        [TestMethod]
        [Priority(1)]
        [Ignore] // enable when environment is ready
        public void ChannelTestReset()
        {
            IChannel channel = _mediaContext.Channels.Create(Guid.NewGuid().ToString().Substring(0, 30), ChannelSize.Large, MakeChannelSettings());
            channel.Reset();
        }

        [TestMethod]
        [Priority(1)]
        //[Ignore] // enable when environment is ready
        public void ChannelTestCreateTrivial()
        {
            IChannel channel = _mediaContext.Channels.Create(Guid.NewGuid().ToString().Substring(0, 30), ChannelSize.Large, MakeChannelSettings());
            channel.Delete();
        }

        #region Retry Logic tests

        [TestMethod]
        [Priority(0)]
        public void TestChannelCreateRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                var actual = _mediaContext.Channels.Create("unittest", ChannelSize.Large, MakeChannelSettings());
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestChannelCreateFailedRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                var actual = _mediaContext.Channels.Create("unittest", ChannelSize.Large, MakeChannelSettings());
            }
            catch (WebException x)
            {
                dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [Priority(0)]
        public void TestChannelUpdateRetry()
        {
            var data = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Channels", data));
            dataContextMock.Setup((ctxt) => ctxt.UpdateObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.Update();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestChannelDeleteRetry()
        {
            var data = new ChannelData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Channels", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.Delete();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestChannelSendDeleteOperationRetry()
        {
            var data = new ChannelData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup((ctxt) => ctxt.AttachTo("Origins", data));
            dataContextMock.Setup((ctxt) => ctxt.DeleteObject(data));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            try
            {
                data.SendDeleteOperation();
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestChannelGetMetric()
        {
            var data = new ChannelData { Name = "testData", Id = "1" };

            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var fakeResponse = new ChannelMetricData[] { new ChannelMetricData() { ChannelName = "test" } };
            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute<ChannelMetricData>(It.IsAny<Uri>()))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            data.SetMediaContext(_mediaContext);

            var result = data.GetMetric();
            Assert.AreEqual("test", result.ChannelName);

            dataContextMock.Verify((ctxt) => ctxt.Execute<ChannelMetricData>(It.IsAny<Uri>()), Times.Exactly(2));
        }

        [TestMethod]
        [Priority(0)]
        public void TestChannelSendCreateOperation()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                var actual = _mediaContext.Channels.SendCreateOperation("unittest", ChannelSize.Large, MakeChannelSettings());
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify((ctxt) => ctxt.SaveChanges(), Times.Exactly(2));
        }
        #endregion Retry Logic tests


        #region Helper/utility methods

        static ChannelSettings MakeChannelSettings()
        {
            var ipList = new List<Ipv4>
                {
                    new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                };

            var settings = new ChannelSettings
            {
                Ingest = new IngestEndpointSettings { Security = new IngestEndpointSecuritySettings { IPv4AllowList = ipList } },
                Preview = new PreviewEndpointSettings { Security = new PreviewEndpointSecuritySettings { IPv4AllowList = ipList } },
                Input = new InputSettings { FMp4FragmentDuration = null },
                Output = new OutputSettings { FragmentsPerHlsSegment = null }
            };

            return settings;
        }

        #endregion
    }
}