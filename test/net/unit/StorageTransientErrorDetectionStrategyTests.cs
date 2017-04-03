//-----------------------------------------------------------------------
// <copyright file="StorageTransientErrorDetectionStrategyTests.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System.Reflection;
using System.Xml;
using System.IO;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Unit
{
    [TestClass]
    public class StorageTransientErrorDetectionStrategyTests
    {
        public static readonly ReadOnlyCollection<HttpStatusCode> SupportedRetryableHttpStatusCodes
            = new ReadOnlyCollection<HttpStatusCode>(new[]
                    {
                        HttpStatusCode.InternalServerError,
                        HttpStatusCode.BadGateway,
                        HttpStatusCode.GatewayTimeout,
                        HttpStatusCode.RequestTimeout,
                        HttpStatusCode.ServiceUnavailable,
                        HttpStatusCode.Unauthorized,
                        HttpStatusCode.Forbidden,
                    });

        private static readonly ReadOnlyCollection<String> SupportedRetryableStorageErrorStrings 
            = new ReadOnlyCollection<String>(new[]
                    {
                        StorageErrorCodeStrings.InternalError, 
                        StorageErrorCodeStrings.ServerBusy, 
                        StorageErrorCodeStrings.OperationTimedOut, 
                        TableErrorCodeStrings.TableServerOutOfMemory
                    });


        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestGeneralException()
        {
            bool actual = new StorageTransientErrorDetectionStrategy().IsTransient(new Exception());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestSocketException()
        {
            var exception = new SocketException();

            bool actual = new StorageTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestTimeoutException()
        {
            var exception = new TimeoutException();

            bool actual = new StorageTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestIOException()
        {
            var exception = new System.IO.IOException();

            bool actual = new StorageTransientErrorDetectionStrategy().IsTransient(exception);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestStorageException()
        {
            List<String> allStorageErrorCodeStrings = GetAllStorageErrorStringConstants();

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

            foreach (string errorString in allStorageErrorCodeStrings)
            {
                var exception = GetSimulatedStorageTransientErrorDetectionStrategy(errorString);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
            }
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestDataServiceQueryException()
        {
            List<String> allStorageErrorCodeStrings = GetAllStorageErrorStringConstants();

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

            foreach (string errorString in allStorageErrorCodeStrings)
            {
                var innerException = new Exception(FormatErrorString(errorString));
                var exception = new DataServiceQueryException("Simulated DataServiceQueryException", innerException);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
            }
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyTestDataServiceRequestException()
        {
            List<String> allStorageErrorCodeStrings = GetAllStorageErrorStringConstants();

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

            foreach (string errorString in allStorageErrorCodeStrings)
            {
                var innerException = new Exception(FormatErrorString(errorString));
                var exception = new DataServiceRequestException("Simulated DataServiceRequestException", innerException);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
            }
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyWebExceptionTest()
        {
            WebExceptionStatus[] allWebExceptionStatusValues = (WebExceptionStatus[])Enum.GetValues(typeof(WebExceptionStatus));

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

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
        public void StorageTransientErrorDetectionStrategyWebExceptionProtocolErrorTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

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
        public void StorageTransientErrorDetectionStrategyDataServiceTransportExceptionTest()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

            foreach (HttpStatusCode status in allHttpStatusCodeValues)
            {
                DataServiceTransportException exception = QueryErrorDetectionStrategyTest.GetMockedTransportException(status);

                Assert.IsFalse(strategy.IsTransient(exception));
            }
        }

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyDataServiceClientExceptionTestByStatusCode()
        {
            HttpStatusCode[] allHttpStatusCodeValues = (HttpStatusCode[])Enum.GetValues(typeof(HttpStatusCode));

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

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

        [TestMethod]
        public void StorageTransientErrorDetectionStrategyDataServiceClientExceptionTestByErrorString()
        {
            List<String> allStorageErrorCodeStrings = GetAllStorageErrorStringConstants();

            StorageTransientErrorDetectionStrategy strategy = new StorageTransientErrorDetectionStrategy();

            foreach (string errorString in allStorageErrorCodeStrings)
            {
                var innerException = new Exception(FormatErrorString(errorString));
                var exception = new DataServiceQueryException("Simulated DataServiceQueryException", innerException);

                if (strategy.IsTransient(exception))
                {
                    Assert.IsTrue(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
                else
                {
                    Assert.IsFalse(SupportedRetryableStorageErrorStrings.Contains(errorString));
                }
            }
        }

        private string FormatErrorString(string errorString)
        { 
            return string.Format(@"<code>{0}</code>", errorString);
        }

        private void GetAllStringConstantsFromType(Type type, List<String> constants)
        {
            FieldInfo[] fields = type.GetFields();
            
            foreach (FieldInfo fi in fields)
            {                
                if ((fi.IsLiteral) && (fi.FieldType == typeof(String)))
                {
                    constants.Add((String)fi.GetRawConstantValue());
                }
            }
        }

        private List<String> GetAllStorageErrorStringConstants()
        {
            Type storageErrorCodeStrings = typeof(StorageErrorCodeStrings);
            Type tableErrorCodeStrings = typeof(TableErrorCodeStrings);

            List<string> returnValue = new List<string>();
            GetAllStringConstantsFromType(storageErrorCodeStrings, returnValue);
            GetAllStringConstantsFromType(tableErrorCodeStrings, returnValue);

            return returnValue;
        }

        private StorageException GetSimulatedStorageTransientErrorDetectionStrategy(string errorString)
        {
            string requestResultAsXml = String.Format("<RequestResult><HTTPStatusCode>500</HTTPStatusCode><HttpStatusMessage>fake status message</HttpStatusMessage><TargetLocation>Primary</TargetLocation><ServiceRequestID>fake requestId</ServiceRequestID><ContentMd5>fake md5</ContentMd5><Etag>fake etag</Etag><RequestDate>fake request date</RequestDate><StartTime>{1}</StartTime><EndTime>{1}</EndTime><ExtendedErrorInformation><Code>{0}</Code></ExtendedErrorInformation></RequestResult>", errorString, DateTime.UtcNow);

            RequestResult requestResult = new RequestResult();
            XmlTextReader reader = new XmlTextReader(new StringReader(requestResultAsXml));
            requestResult.ReadXml(reader);
            Exception innerException = null;

            return new StorageException(requestResult, "Simulated StorageException", innerException);
        }
    }
}
