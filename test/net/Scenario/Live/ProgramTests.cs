//-----------------------------------------------------------------------
// <copyright file="ProgramTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.MediaServices.Client.Tests.Common;
using System.Net;
using Moq;
using System;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class ProgramTests
    {
        private CloudMediaContext _mediaContext;
        [TestInitialize]
        public void SetupTest()
        {
            _mediaContext = WindowsAzureMediaServicesTestConfiguration.CreateCloudMediaContext();
        }

        #region Retry Logic tests

        [TestCategory("ClientSDK")]
        [Owner("ClientSDK")]
        [Priority(0)]
        public void TestProgramCreateRetryAsyn()
        {
            var expected = new ProgramData { Name = "testData" };
            var fakeException = new WebException("test", WebExceptionStatus.ConnectionClosed);
            var dataContextMock = TestMediaServicesClassFactory.CreateSaveChangesMock(fakeException, 2, expected);

            dataContextMock.Setup((ctxt) => ctxt.AddObject("Channels", It.IsAny<object>()));

            _mediaContext.MediaServicesClassFactory = new TestMediaServicesClassFactory(dataContextMock.Object);

            IChannel channel = new ChannelData();
            ProgramBaseCollection programs = new ProgramBaseCollection(_mediaContext, channel);

            var actual = programs.Create(expected.Name, TimeSpan.FromHours(1), Guid.NewGuid().ToString());

            Assert.AreEqual(expected.Name, actual.Name);

            dataContextMock.Verify((ctxt) => ctxt.SaveChangesAsync(It.IsAny<object>()), Times.Exactly(2));
        }

        #endregion Retry Logic tests
    }
}