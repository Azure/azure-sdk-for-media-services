//-----------------------------------------------------------------------
// <copyright file="SaveChangesErrorDetectionStrategyTest.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
    public class SaveChangesErrorDetectionStrategyTest
    {
        private static readonly ReadOnlyCollection<WebExceptionStatus> SupportedRetryableWebExceptions 
            = new ReadOnlyCollection<WebExceptionStatus>(new[]
                    {
                        WebExceptionStatus.ConnectFailure,
                        WebExceptionStatus.NameResolutionFailure,
                        WebExceptionStatus.ProxyNameResolutionFailure,
                        WebExceptionStatus.SendFailure,
                    });

        private static readonly ReadOnlyCollection<HttpStatusCode> SupportedRetryableHttpStatusCodes 
            = new ReadOnlyCollection<HttpStatusCode>(new[]
                    {
                        HttpStatusCode.RequestTimeout,
                        HttpStatusCode.ServiceUnavailable,
                    });

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestGeneralException()
        {
            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(new Exception());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestSocketException()
        {
            var exception = new SocketException();

            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestTimeoutException()
        {
            var exception = new TimeoutException();

            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestIOException()
        {
            var exception = new System.IO.IOException();

            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestDataServiceQueryException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceQueryException("Simulated DataServiceQueryException");

            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyTestDataServiceRequestException()
        {
            // Unfortunately this exception isn't easy to Mock with an actual error code so just
            // do a basic test
            var exception = new DataServiceRequestException("Simulated DataServiceRequestException");

            bool actual = new SaveChangesErrorDetectionStrategy().IsTransient(exception);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void SaveChangesErrorDetectionStrategyWebExceptionTest()
        {
            WebExceptionStatus[] allWebExceptionStatusValues = (WebExceptionStatus[])Enum.GetValues(typeof(WebExceptionStatus));

            SaveChangesErrorDetectionStrategy strategy = new SaveChangesErrorDetectionStrategy();

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
        public void SaveChangesErrorDetectionStrategyWebExceptionProtocolErrorTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            SaveChangesErrorDetectionStrategy strategy = new SaveChangesErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                WebException exception = QueryErrorDetectionStrategyTest.GetMockedWebExceptionWithProtocolError(status);

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
        public void SaveChangesErrorDetectionStrategyDataServiceTransportExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            SaveChangesErrorDetectionStrategy strategy = new SaveChangesErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceTransportException exception = QueryErrorDetectionStrategyTest.GetMockedTransportException(status);

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
        public void SaveChangesErrorDetectionStrategyDataServiceClientExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            SaveChangesErrorDetectionStrategy strategy = new SaveChangesErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceClientException exception = QueryErrorDetectionStrategyTest.GetMockedClientException(status);

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
    }
}
