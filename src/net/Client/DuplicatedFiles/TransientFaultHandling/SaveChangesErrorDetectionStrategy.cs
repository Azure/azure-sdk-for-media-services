//-----------------------------------------------------------------------
// <copyright file="RetryStrategyFactory.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using Microsoft.Practices.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    public class SaveChangesErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        private static readonly ReadOnlyCollection<int> CommonRetryableWebExceptions =
            new ReadOnlyCollection<int>(
                new[]
                    {
                        (int)WebExceptionStatus.Timeout,
                        (int)WebExceptionStatus.KeepAliveFailure,
                        (int)WebExceptionStatus.ConnectionClosed,
                        (int)WebExceptionStatus.ProtocolError,
                        (int)WebExceptionStatus.PipelineFailure,
                        (int)WebExceptionStatus.SendFailure,
                        (int)WebExceptionStatus.ReceiveFailure,
                        (int)WebExceptionStatus.ConnectFailure,
                    });

        protected virtual bool OnIsTransient(Exception ex)
        {
            return false;
        }

        public bool IsTransient(Exception ex)
        {
            return ex != null && (CheckIsTransient(ex) || OnIsTransient(ex));
        }

        private static bool CheckIsTransient(Exception ex)
        {
            var dataServiceException = ex.FindInnerException<DataServiceRequestException>();
            if (dataServiceException.Response.IsBatchResponse && CommonRetryableWebExceptions.Contains(dataServiceException.Response.BatchStatusCode))
            {
                return true;
            }
            var responses = dataServiceException.Response.ToList();

            //If we have responses with retryable status codes we should retry
            return responses.Any(r => CommonRetryableWebExceptions.Contains(r.StatusCode));
        }
    }
}
