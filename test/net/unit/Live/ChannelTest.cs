//-----------------------------------------------------------------------
// <copyright file="OperationTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Live.UnitTests
{
    [TestClass]
    public class ChannelTest
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = Helper.GetMediaDataServiceContextForUnitTests();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelCreateRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup(ctxt => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.Channels.Create(
                    new ChannelCreationOptions
                    {
                        Name = "unittest",
                        Input = MakeChannelInput(),
                        Preview = MakeChannelPreview(),
                        Output = MakeChannelOutput()
                    });
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }


        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelQueryRetry()
        {
            CloudMediaContext mediaContext = Helper.GetMediaDataServiceContextForUnitTests(0);
            var result = mediaContext.Channels.FirstOrDefault();
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        [ExpectedException(typeof(WebException))]
        public void TestChannelCreateFailedRetry()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 10, expected);

            dataContextMock.Setup(ctxt => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.Channels.Create(
                    new ChannelCreationOptions
                    {
                        Name = "unittest",
                        Input = MakeChannelInput(),
                        Preview = MakeChannelPreview(),
                        Output = MakeChannelOutput()
                    });
            }
            catch (WebException x)
            {
                dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.AtLeast(3));
                Assert.AreEqual(fakeException, x);
                throw;
            }

            Assert.Fail("Expected exception");
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelUpdateRetry()
        {
            var data = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup(ctxt => ctxt.AttachTo("Channels", data));
            dataContextMock.Setup(ctxt => ctxt.UpdateObject(data));

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

            dataContextMock.Verify(ctxt => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelDeleteRetry()
        {
            var data = new ChannelData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup(ctxt => ctxt.AttachTo("Channels", data));
            dataContextMock.Setup(ctxt => ctxt.DeleteObject(data));

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

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(data), Times.Exactly(2));
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelSendDeleteOperationRetry()
        {
            var data = new ChannelData { Name = "testData", Id = "1" };

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, data);

            dataContextMock.Setup(ctxt => ctxt.AttachTo("Origins", data));
            dataContextMock.Setup(ctxt => ctxt.DeleteObject(data));

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

            dataContextMock.Verify(ctxt => ctxt.SaveChanges(), Times.Exactly(2));
        }

        [TestMethod]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestChannelSendCreateOperation()
        {
            var expected = new ChannelData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup(ctxt => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            try
            {
                _mediaContext.Channels.SendCreateOperation(
                    new ChannelCreationOptions
                    {
                        Name = "unittest",
                        Input = MakeChannelInput(),
                        Preview = MakeChannelPreview(),
                        Output = MakeChannelOutput()
                    });
            }
            catch (NotImplementedException x)
            {
                Assert.AreEqual(TestMediaDataServiceResponse.TestMediaDataServiceResponseExceptionMessage, x.Message);
            }

            dataContextMock.Verify(ctxt => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }
        
        static ChannelInput MakeChannelInput()
        {
            return new ChannelInput
            {
                KeyFrameInterval = TimeSpan.FromSeconds(2),
                StreamingProtocol = StreamingProtocol.FragmentedMP4,
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "testName1",
                            Address = IPAddress.Parse("1.1.1.1"),
                            SubnetPrefixLength = 24
                        }
                    }
                }
            };
        }

        static ChannelPreview MakeChannelPreview()
        {
            return new ChannelPreview
            {
                AccessControl = new ChannelAccessControl
                {
                    IPAllowList = new List<IPRange>
                    {
                        new IPRange
                        {
                            Name = "testName1",
                            Address = IPAddress.Parse("1.1.1.1"),
                            SubnetPrefixLength = 24
                        }
                    }
                }
            };
        }

        static ChannelOutput MakeChannelOutput()
        {
            return new ChannelOutput
            {
                Hls = new ChannelOutputHls { FragmentsPerSegment = 1 }
            };
        }
    }
}