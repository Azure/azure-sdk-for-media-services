//-----------------------------------------------------------------------
// <copyright file="MediaErrorDetectionStrategy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    public abstract class MediaErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        //
        //  Note that WebExceptionStatus.ProtocolError and WebExceptionStatus.Timeout can also be
        //  retried but they have additional logic associated with them in IsRetriableWebException.
        //
        private static readonly ReadOnlyCollection<WebExceptionStatus> CommonRetryableWebExceptions =
            new ReadOnlyCollection<WebExceptionStatus>(
                new[]
                    {
                        WebExceptionStatus.ConnectFailure,
                        WebExceptionStatus.NameResolutionFailure,
                        WebExceptionStatus.ProxyNameResolutionFailure,
                        WebExceptionStatus.SendFailure,
                    });

        private static readonly ReadOnlyCollection<WebExceptionStatus> RetryableWebExceptionsForIdempotentOperations =
            new ReadOnlyCollection<WebExceptionStatus>(
                new[]
                    {
                        WebExceptionStatus.PipelineFailure,
                        WebExceptionStatus.ConnectionClosed,
                        WebExceptionStatus.KeepAliveFailure,
                        WebExceptionStatus.UnknownError,
                        WebExceptionStatus.ReceiveFailure,
                        WebExceptionStatus.RequestCanceled,
                        WebExceptionStatus.Timeout,
                    });

        //
        //  Http response status errors to be INCLUDED in retry.  Note that all 500 level errors may 
        //  also retried in IsRetriableHttpStatusCode.
        //
        private static readonly ReadOnlyCollection<int> CommonRetryableHttpErrorStatusCodes =
            new ReadOnlyCollection<int>(
                new[]
                    {
                        (int)HttpStatusCode.RequestTimeout,      //  408 - Note this is a request timeout which means the server didn't receive the entire request
                        429,                                     //  429 - Too Many Requests doesn't map to HttpStatusCode
                        (int)HttpStatusCode.ServiceUnavailable,  //  503
                    });

        private static readonly ReadOnlyCollection<int> RetryableHttpStatusCodesForIdempotentOperations =
            new ReadOnlyCollection<int>(
                new[]
                    {
                        (int)HttpStatusCode.InternalServerError, //  500 - this may or may not succeed on retry depending on the issue with the server.  Still a reasonable number of retries is okay.
                        (int)HttpStatusCode.BadGateway,          //  502
                        (int)HttpStatusCode.GatewayTimeout ,     //  504
                    });

        protected bool IsRetriableWebException(Exception ex, bool operationIdempotentOnRetry, bool retryOnUnauthorizedErrors)
        {
            try
            {
                var webException = ex.FindInnerException<WebException>();

                if (webException != null)
                {
                    if (CommonRetryableWebExceptions.Contains(webException.Status))
                    {
                        return true;
                    }
                    else if (webException.Status == WebExceptionStatus.ProtocolError)
                    {
                        // The response received from the server was complete but indicated a protocol-level error. 
                        // Decide if the HTTP protocol error is retriable or not.
                        var response = webException.Response as HttpWebResponse;

                        if (response == null || IsRetriableHttpStatusCode(response.StatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                        {
                            return true;
                        }
                    }
                    else if (operationIdempotentOnRetry)
                    {
                        if (RetryableWebExceptionsForIdempotentOperations.Contains(webException.Status))
                        {
                            return true;
                        }
                    }
                }
            }
            catch(Exception)
            {
                // do nothing, just return false below
            }

            return false;
        }

        protected bool IsRetriableHttpStatusCode(HttpStatusCode statusCode, bool operationIdempotentOnRetry, bool retryOnUnauthorizedErrors)
        {
            return IsRetriableHttpStatusCode((int)statusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors);
        }

        protected bool IsRetriableHttpStatusCode(int statusCode, bool operationIdempotentOnRetry, bool retryOnUnauthorizedErrors)
        {
            try
            {
                if (CommonRetryableHttpErrorStatusCodes.Contains(statusCode))
                {
                    return true;
                }
                else if (operationIdempotentOnRetry && RetryableHttpStatusCodesForIdempotentOperations.Contains(statusCode))
                {
                    return true;
                }
                else if (retryOnUnauthorizedErrors && ((statusCode == (int)HttpStatusCode.Unauthorized) || (statusCode == (int)HttpStatusCode.Forbidden)))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // do nothing, just return false below
            }

            return false;
        }

        protected bool IsRetriableDataServiceException(Exception ex, bool operationIdempotentOnRetry, bool retryOnUnauthorizedErrors)
        {
            try
            {
                var transportException = ex.FindInnerException<DataServiceTransportException>();

                if (transportException != null)
                {
                    if (transportException.Response == null)
                    {
                        // If we don't have a response object to look at then we don't really know what happened on the server.
                        // Thus if the operation is Idempotent, we will go ahead and retry because even if the first operation
                        // succeeded on the server redoing the operation won't change the final result.  If the operation is
                        // not idempotent, we won't retry since we don't have any details on the error.
                        return operationIdempotentOnRetry;
                    }
                    else if (IsRetriableHttpStatusCode(transportException.Response.StatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                    {
                        return true;
                    }
                }

                var requestException = ex.FindInnerException<DataServiceRequestException>();

                if (requestException != null)
                {
                    if (requestException.Response == null)
                    {
                        // If we don't have a response object to look at then we don't really know what happened on the server.
                        // Thus if the operation is Idempotent, we will go ahead and retry because even if the first operation
                        // succeeded on the server redoing the operation won't change the final result.  If the operation is
                        // not idempotent, we won't retry since we don't have any details on the error.
                        return operationIdempotentOnRetry;
                    }
                    else if (requestException.Response.IsBatchResponse)
                    {
                        if (IsRetriableHttpStatusCode(requestException.Response.BatchStatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // If this isn't a batch response we have to check the StatusCode on the Response object itself
                        var responses = requestException.Response.ToList();

                        if ((responses.Count == 1) && IsRetriableHttpStatusCode(responses[0].StatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                        {
                            return true;
                        }                    
                    }
                }

                var queryException = ex.FindInnerException<DataServiceQueryException>();

                if (queryException != null)
                {
                    if (queryException.Response == null)
                    {
                        // If we don't have a response object to look at then we don't really know what happened on the server.
                        // Thus if the operation is Idempotent, we will go ahead and retry because even if the first operation
                        // succeeded on the server redoing the operation won't change the final result.  If the operation is
                        // not idempotent, we won't retry since we don't have any details on the error.
                        return operationIdempotentOnRetry;
                    }
                    else if (IsRetriableHttpStatusCode(queryException.Response.StatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                    {
                        return true;
                    }
                }

                var clientException = ex.FindInnerException<DataServiceClientException>();

                if (clientException != null)
                {
                    if (IsRetriableHttpStatusCode(clientException.StatusCode, operationIdempotentOnRetry, retryOnUnauthorizedErrors))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // do nothing, just return false below
            }

            return false;
        }

        protected bool IsSocketException(Exception ex)
        {
            return (ex is SocketException || (ex.FindInnerException<SocketException>() != null));
        }

        protected bool IsTimeoutException(Exception ex)
        {
            return (ex is TimeoutException || (ex.FindInnerException<TimeoutException>() != null));
        }

        protected virtual bool OnIsTransient(Exception ex)
        {
            return false;
        }

        public virtual bool IsTransient(Exception ex)
        {
            return ex != null && (CheckIsTransient(ex) || OnIsTransient(ex));
        }

        protected abstract bool CheckIsTransient(Exception ex);
    }
}
