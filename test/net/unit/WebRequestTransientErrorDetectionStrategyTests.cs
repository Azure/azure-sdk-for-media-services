//-----------------------------------------------------------------------
// <copyright file="WebRequestTransientErrorDetectionStrategyTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class WebRequestTransientErrorDetectionStrategyTests
    {
        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestGeneralException()
        {
            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(new Exception());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestSocketException()
        {
            var exception = new SocketException();

            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestTimeoutException()
        {
            var exception = new TimeoutException();

            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestIOException()
        {
            var exception = new IOException();

            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestDataServiceQueryException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceQueryException("Simulated DataServiceQueryException");

            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyTestDataServiceRequestException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceRequestException("Simulated DataServiceRequestException");

            bool actual = new WebRequestTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyWebExceptionTest()
        {
            WebExceptionStatus[] allWebExceptionStatusValues = (WebExceptionStatus[])Enum.GetValues(typeof(WebExceptionStatus));

            WebRequestTransientErrorDetectionStrategy strategy = new WebRequestTransientErrorDetectionStrategy();

            foreach (WebExceptionStatus status in allWebExceptionStatusValues)
            {
                if (status == WebExceptionStatus.ProtocolError)
                {
                    // This is covered in a separate test
                    continue;
                }

                WebException exception = new WebException("Simulated WebException with " + status.ToString(), status);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(QueryErrorDetectionStrategyTest.SupportedRetryableWebExceptions.Contains(exception.Status), exception.Status.ToString());
                }
                else
                {
                    Assert.IsFalse(QueryErrorDetectionStrategyTest.SupportedRetryableWebExceptions.Contains(exception.Status), exception.Status.ToString());
                }
            }
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyWebExceptionProtocolErrorTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            WebRequestTransientErrorDetectionStrategy strategy = new WebRequestTransientErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                WebException exception = QueryErrorDetectionStrategyTest.GetMockedWebExceptionWithProtocolError(status);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(QueryErrorDetectionStrategyTest.SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
                else
                {
                    Assert.IsFalse(QueryErrorDetectionStrategyTest.SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
            }
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyDataServiceTransportExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            WebRequestTransientErrorDetectionStrategy strategy = new WebRequestTransientErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceTransportException exception = QueryErrorDetectionStrategyTest.GetMockedTransportException(status);

                Assert.IsFalse(strategy.IsTransient(exception));
            }
        }

        [TestMethod]
        public void WebRequestTransientErrorDetectionStrategyDataServiceClientExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            WebRequestTransientErrorDetectionStrategy strategy = new WebRequestTransientErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceClientException exception = QueryErrorDetectionStrategyTest.GetMockedClientException(status);

                Assert.IsFalse(strategy.IsTransient(exception));
            }
        }
    }
}

