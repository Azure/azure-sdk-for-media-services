//-----------------------------------------------------------------------
// <copyright file="SaveChangesErrorDetectionStrategy.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    public class SaveChangesErrorDetectionStrategy : MediaErrorDetectionStrategy
    {
        protected override bool CheckIsTransient(Exception ex)
        {
            bool returnValue = false;

            try
            {
                var dataServiceException = ex.FindInnerException<DataServiceRequestException>();

                if ((dataServiceException != null) && (dataServiceException.Response != null))
                {
                    if (dataServiceException.Response.IsBatchResponse)
                    {
                        returnValue = CommonRetryableWebExceptions.Any(s => (int)s == dataServiceException.Response.BatchStatusCode);
                    }
                    else
                    {
                        // If this isn't a batch response we have to check the StatusCode on the Response object itself
                        var responses = dataServiceException.Response.ToList();

                        if (responses.Count == 1)
                        {
                            returnValue = CommonRetryableWebExceptions.Any(s => (int)s == responses[0].StatusCode);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // we don't want to hide the original exception with any errors we might generate here 
                // so just swallow the exception and don't retry
                returnValue = false;
            }

            return returnValue;
        }


    }
}
