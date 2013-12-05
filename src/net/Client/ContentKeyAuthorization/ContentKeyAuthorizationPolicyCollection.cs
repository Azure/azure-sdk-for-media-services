//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    public class ContentKeyAuthorizationPolicyCollection : CloudBaseCollection<IContentKeyAuthorizationPolicy>
    {
        /// <summary>
        /// The Authorization Policy set name.
        /// </summary>
        internal const string ContentKeyAuthorizationPolicySet = "ContentKeyAuthorizationPolicies";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentKeyAuthorizationPolicyCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal ContentKeyAuthorizationPolicyCollection(MediaContextBase context)
            : base(context)
        {
            MediaServicesClassFactory factory = this.MediaContext.MediaServicesClassFactory;
            this.Queryable = factory.CreateDataServiceContext().CreateQuery<ContentKeyAuthorizationPolicyData>(ContentKeyAuthorizationPolicySet);
        }

        /// <summary>
        /// Creates the authorization policy asynchronously.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual Task<IContentKeyAuthorizationPolicy> CreateAsync(string name)
        {

            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            var authorizationPolicyData = new ContentKeyAuthorizationPolicyData
            {
                Name = name
            };

            authorizationPolicyData.SetMediaContext(this.MediaContext);
            dataContext.AddObject(ContentKeyAuthorizationPolicySet, authorizationPolicyData);

            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(authorizationPolicyData))
                .ContinueWith<IContentKeyAuthorizationPolicy>(
                    t =>
                    {
                        return (ContentKeyAuthorizationPolicyData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}