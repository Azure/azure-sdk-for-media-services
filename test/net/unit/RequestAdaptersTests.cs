//-----------------------------------------------------------------------
// <copyright file="RequestAdaptersTests.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class RequestAdaptersTests
    {

        [TestMethod]
        public void ClientRequestIdPresentInWebRequestAndValidGuid()
        {
            var mock = new MockRepository(MockBehavior.Loose);
            var request = mock.Create<HttpWebRequest>();
            var headers = new WebHeaderCollection();
            request.SetupProperty(c => c.Headers,headers);
            ClientRequestIdAdapter adapter = new ClientRequestIdAdapter();
            adapter.AddClientRequestId(request.Object);
            Assert.AreEqual(1, request.Object.Headers.Count);
            string xMsClientRequestId = "x-ms-client-request-id";
            Assert.IsNotNull(request.Object.Headers[xMsClientRequestId]);
            Guid guid; 
            Assert.IsTrue(Guid.TryParse(request.Object.Headers[xMsClientRequestId],out guid));
        }

        [TestMethod]
        public void ServiceVersionHeaderPresentAndContainsVersion()
        {
            var mock = new MockRepository(MockBehavior.Loose);
            var request = mock.Create<HttpWebRequest>();
            var headers = new WebHeaderCollection();
            request.SetupProperty(c => c.Headers, headers);
            ServiceVersionAdapter adapter = new ServiceVersionAdapter(new Version(1,0));
            adapter.AddVersionToRequest(request.Object);
            Assert.AreEqual(1, request.Object.Headers.Count);
            string xMsVersion = "x-ms-version";
            Assert.IsNotNull(request.Object.Headers[xMsVersion]);
            Assert.AreEqual("1.0",request.Object.Headers[xMsVersion]);
        }

        [TestMethod]
        public void UserAgentHeaderPresentAndContainsVersion()
        {
            var mock = new MockRepository(MockBehavior.Loose);
            var request = mock.Create<WebRequest>();
            var headers = new WebHeaderCollection();
            request.SetupProperty(c => c.Headers, headers);
            UserAgentAdapter adapter = new UserAgentAdapter(new Version(1, 0));
            adapter.AddUserAgentToRequest(request.Object);
            Assert.AreEqual(1, request.Object.Headers.Count);

            Assert.IsNotNull(request.Object.Headers[HttpRequestHeader.UserAgent]);
            Assert.AreEqual("Azure Media Services .NET SDK v1.0", request.Object.Headers[HttpRequestHeader.UserAgent]);
        }
         
    }
}