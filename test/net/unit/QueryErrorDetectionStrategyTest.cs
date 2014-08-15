//-----------------------------------------------------------------------
// <copyright file="QueryErrorDetectionStrategyTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Moq;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class QueryErrorDetectionStrategyTest
    {
        public static readonly ReadOnlyCollection<WebExceptionStatus> SupportedRetryableWebExceptions 
            = new ReadOnlyCollection<WebExceptionStatus>(new[]
                    {
                        WebExceptionStatus.ConnectFailure,
                        WebExceptionStatus.NameResolutionFailure,
                        WebExceptionStatus.ProxyNameResolutionFailure,
                        WebExceptionStatus.SendFailure,
                        WebExceptionStatus.PipelineFailure,
                        WebExceptionStatus.ConnectionClosed,
                        WebExceptionStatus.KeepAliveFailure,
                        WebExceptionStatus.UnknownError,
                        WebExceptionStatus.ReceiveFailure,
                        WebExceptionStatus.RequestCanceled,
                        WebExceptionStatus.Timeout,
                    });

        public static readonly ReadOnlyCollection<HttpStatusCode> SupportedRetryableHttpStatusCodes 
            = new ReadOnlyCollection<HttpStatusCode>(new[]
                    {
                        HttpStatusCode.InternalServerError,
                        HttpStatusCode.BadGateway,
                        HttpStatusCode.GatewayTimeout,
                        HttpStatusCode.RequestTimeout,
                        HttpStatusCode.ServiceUnavailable,
                    });

        [TestMethod]
        public void QueryErrorDetectionStrategyTestGeneralException()
        {
            bool actual = new QueryErrorDetectionStrategy().IsTransient(new Exception());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyTestSocketException()
        {
            var exception = new SocketException();

            bool actual = new QueryErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyTestTimeoutException()
        {
            var exception = new TimeoutException();

            bool actual = new QueryErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyTestIOException()
        {
            var exception = new System.IO.IOException();

            bool actual = new QueryErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyTestDataServiceQueryException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceQueryException("Simulated DataServiceQueryException");

            bool actual = new QueryErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyTestDataServiceRequestException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceRequestException("Simulated DataServiceRequestException");

            bool actual = new QueryErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyWebExceptionTest()
        {
            WebExceptionStatus[] allWebExceptionStatusValues = (WebExceptionStatus[])Enum.GetValues(typeof(WebExceptionStatus));

            QueryErrorDetectionStrategy strategy = new QueryErrorDetectionStrategy();

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
                    Assert.IsTrue(SupportedRetryableWebExceptions.Contains(exception.Status), exception.Status.ToString());
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableWebExceptions.Contains(exception.Status), exception.Status.ToString());
                }
            }
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyWebExceptionProtocolErrorTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            QueryErrorDetectionStrategy strategy = new QueryErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                WebException exception = GetMockedWebExceptionWithProtocolError(status);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
            }
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyDataServiceTransportExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            QueryErrorDetectionStrategy strategy = new QueryErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceTransportException exception = GetMockedTransportException(status);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
            }
        }

        [TestMethod]
        public void QueryErrorDetectionStrategyDataServiceClientExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            QueryErrorDetectionStrategy strategy = new QueryErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceClientException exception = GetMockedClientException(status);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableHttpStatusCodes.Contains(status), status.ToString());
                }
            }
        }

        public static DataServiceClientException GetMockedClientException(HttpStatusCode statusCode)
        {
            return new DataServiceClientException("Simulated WebException with " + statusCode.ToString(), (int)statusCode);
        }

        public static DataServiceTransportException GetMockedTransportException(HttpStatusCode statusCode)
        {
            var responseMessageMock = new Mock<Data.OData.IODataResponseMessage>();
            responseMessageMock.SetupGet(x => x.StatusCode).Returns((int)statusCode);

            return new DataServiceTransportException(responseMessageMock.Object, new Exception());
        }

        public static WebException GetMockedWebExceptionWithProtocolError(HttpStatusCode statusCode)
        {
            var httpWebResponseMock = new Mock<HttpWebResponse>();
            httpWebResponseMock.SetupGet(x => x.StatusCode).Returns(statusCode);

            return new WebException("Simulated WebException with ProtocolError", null, WebExceptionStatus.ProtocolError, httpWebResponseMock.Object);
        }
    }
}
