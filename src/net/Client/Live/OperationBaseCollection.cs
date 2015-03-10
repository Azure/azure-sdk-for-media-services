//-----------------------------------------------------------------------
// <copyright file="OperationBaseCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IOperation"/>.
    /// </summary>
    public class OperationBaseCollection
    {
        internal const string OperationSet = "Operations";
        private readonly CloudMediaContext _cloudMediaContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // todo: remove
        internal OperationBaseCollection(CloudMediaContext cloudMediaContext)
        {
            this._cloudMediaContext = cloudMediaContext;
        }

        /// <summary>
        /// Retrieves an operation by its Id.
        /// </summary>
        /// <param name="id">Id of the operation.</param>
        /// <returns>Operation.</returns>
        public IOperation GetOperation(string id)
        {
            Uri uri = new Uri(string.Format(CultureInfo.InvariantCulture, "/{0}('{1}')", OperationSet, id), UriKind.Relative);
            IMediaDataServiceContext dataContext = this._cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();

            MediaRetryPolicy retryPolicy = _cloudMediaContext.MediaServicesClassFactory.GetQueryRetryPolicy(dataContext as IRetryPolicyAdapter);

            IOperation operation = retryPolicy.ExecuteAction<IEnumerable<OperationData>>(() => dataContext.Execute<OperationData>(uri)).SingleOrDefault();
            return operation;
        }
    }
}
