//-----------------------------------------------------------------------
// <copyright file="StorageTransientErrorDetectionStrategy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Microsoft.WindowsAzure.Storage.Table.Protocol;


namespace Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling
{
    /// <summary>
    /// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure storage services.
    /// </summary>
    public class StorageTransientErrorDetectionStrategy : MediaErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>True if the specified exception is considered as transient, otherwise false.</returns>
        public override bool IsTransient(Exception ex)
        {
            return ex != null && (CheckIsTransient(ex) || (ex.InnerException != null && CheckIsTransient(ex.InnerException)));
        }

        protected override bool CheckIsTransient(Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null && 
                (webException.Status == WebExceptionStatus.ProtocolError
                 || webException.Status == WebExceptionStatus.ConnectionClosed
                 || webException.Status == WebExceptionStatus.NameResolutionFailure
                 || webException.Status == WebExceptionStatus.PipelineFailure
                 || webException.Status == WebExceptionStatus.Timeout
                 || webException.Status == WebExceptionStatus.ProxyNameResolutionFailure
                 ))
            {
                return true;
            }

            var dataServiceException = ex as DataServiceRequestException;

            if (dataServiceException != null)
            {
                if (IsErrorStringMatch(GetErrorCode(dataServiceException), StorageErrorCodeStrings.InternalError, StorageErrorCodeStrings.ServerBusy, StorageErrorCodeStrings.OperationTimedOut, TableErrorCodeStrings.TableServerOutOfMemory))
                {
                    return true;
                }
            }

            var serverException = ex as StorageException;

            if (serverException != null)
            {
                if (IsErrorStringMatch(serverException, StorageErrorCodeStrings.InternalError, StorageErrorCodeStrings.ServerBusy, StorageErrorCodeStrings.OperationTimedOut, TableErrorCodeStrings.TableServerOutOfMemory))
                {
                    return true;
                }
            }

            if (ex is TimeoutException)
            {
                return true;
            }

            if (ex is SocketException)
            {
                return true;
            }

            if (ex is IOException)
            {
                return true;
            }
            return false;
        }

        #region Private members
        private static string GetErrorCode(DataServiceRequestException ex)
        {
            if (ex != null && ex.InnerException != null)
            {
                var regEx = new Regex(@"<code>(\w+)</code>", RegexOptions.IgnoreCase);
                var match = regEx.Match(ex.InnerException.Message);

                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private static bool IsErrorStringMatch(StorageException ex, params string[] errorStrings)
        {
            return ex != null && ex.RequestInformation.ExtendedErrorInformation != null && errorStrings.Contains(ex.RequestInformation.ExtendedErrorInformation.ErrorCode);
        }

        private static bool IsErrorStringMatch(string exceptionErrorString, params string[] errorStrings)
        {
            return errorStrings.Contains(exceptionErrorString);
        }
        #endregion
    }
}
