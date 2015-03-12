//-----------------------------------------------------------------------
// <copyright file="AssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Generic;
using System.Net;
using System.Data.Services.Client;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests.Common
{
    public class TestMediaDataServiceResponse : IMediaDataServiceResponse
    {
        public const string TestMediaDataServiceResponseExceptionMessage = "TestMediaDataServiceResponseExceptionMessage";

        private readonly Dictionary<string, string> _headers;

        public TestMediaDataServiceResponse(Dictionary<string, string> headers = null)
        {
            _headers = headers;
        }

        #region IMediaDataServiceResponse Members

        public IDictionary<string, string> BatchHeaders
        {
            get { return new Dictionary<string, string>(); }
        }

        public int BatchStatusCode
        {
            get { return (int)WebExceptionStatus.Success; }
        }

        public bool IsBatchResponse
        {
            get { return false; }
        }

        public object AsyncState {get; set;}

        #endregion

        #region IEnumerable<OperationResponse> Members

        public IEnumerator<OperationResponse> GetEnumerator()
        {
            if (_headers == null)
            {
                throw new NotImplementedException(TestMediaDataServiceResponseExceptionMessage);
            }

            return new List<OperationResponse> {new InvokeResponse(_headers)}.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
