//-----------------------------------------------------------------------
// <copyright file="AsyncHelperTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using Moq;
using System.Net;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    
    
    /// <summary>
    ///This is a test class for AsyncHelperTest and is intended
    ///to contain all AsyncHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AsyncHelperTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        /// <summary>
        ///A test for WaitOperationCompletion
        ///</summary>
        [TestMethod()]
        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        public void WaitOperationCompletionTest()
        {
            var data = new OperationData {Id = "1", State = OperationState.Succeeded.ToString()};

            var dataContextMock = new Mock<IMediaDataServiceContext>();

            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);

            var fakeResponse = new OperationData[] { data };
            int exceptionCount = 2;

            dataContextMock.Setup((ctxt) => ctxt
                .Execute<OperationData>(It.IsAny<Uri>()))
                .Returns(() =>
                {
                    if (--exceptionCount > 0) throw fakeException;
                    return fakeResponse;
                });

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);
            var actual = AsyncHelper.WaitOperationCompletion(_mediaContext, data.Id, TimeSpan.FromMilliseconds(10));
            Assert.AreEqual(data.Id, actual.Id);

            dataContextMock.Verify((ctxt) => ctxt.Execute<OperationData>(It.IsAny<Uri>()), Times.Exactly(2));
        }
    }
}
