//-----------------------------------------------------------------------
// <copyright file="AccessPolicyCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IAccessPolicy"/>.
    /// </summary>
    public class AccessPolicyBaseCollection : CloudBaseCollection<IAccessPolicy>
    {
        /// <summary>
        /// The AccessPolicy set name.
        /// </summary>
        internal const string AccessPolicySet = "AccessPolicies";


        /// <summary>
        /// Initializes a new instance of the <see cref="AccessPolicyBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal AccessPolicyBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
			this.Queryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IAccessPolicy, AccessPolicyData>(AccessPolicySet);
        }

        /// <summary>
        /// Asynchronously creates an <see cref="IAccessPolicy"/> with the provided name and permissions, valid for the provided duration.
        /// </summary>
        /// <param name="name">Specifies a friendly name for the AccessPolicy.</param>
        /// <param name="duration">Specifies the duration that locators created from this AccessPolicy will be valid for.</param>
        /// <param name="permissions">Specifies permissions for the created AccessPolicy.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;IAccessPolicy&gt;.</returns>
        public Task<IAccessPolicy> CreateAsync(string name, TimeSpan duration, AccessPermissions permissions)
        {
            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            AccessPolicyData accessPolicy = new AccessPolicyData
            {
                Name = name,
                DurationInMinutes = AccessPolicyData.GetInternalDuration(duration),
                Permissions = AccessPolicyData.GetInternalPermissions(permissions)
            };

            dataContext.AddObject(AccessPolicySet, accessPolicy);

            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(accessPolicy))
                .ContinueWith<IAccessPolicy>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        return (AccessPolicyData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Creates an AccessPolicy with the provided name and permissions, valid for the provided duration.
        /// </summary>
        /// <param name="name">Specifies a friendly name for the AccessPolicy.</param>
        /// <param name="duration">Specifies the duration that locators created from this AccessPolicy will be valid for.</param>
        /// <param name="permissions">Specifies permissions for the created AccessPolicy.</param>
        /// <returns>An <see cref="IAccessPolicy"/> with the provided <paramref name="name"/>, <paramref name="duration"/> and <paramref name="permissions"/>.</returns>              
        public IAccessPolicy Create(string name, TimeSpan duration, AccessPermissions permissions)
        {
            try
            {
                Task<IAccessPolicy> task = this.CreateAsync(name, duration, permissions);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Verifies the access policy.
        /// </summary>
        /// <param name="accessPolicy">The access policy.</param>
        internal static void VerifyAccessPolicy(IAccessPolicy accessPolicy)
        {
            if (accessPolicy == null)
            {
                throw new ArgumentNullException("accessPolicy");
            }

            if (!(accessPolicy is AccessPolicyData))
            {
                throw new ArgumentException(StringTable.ErrorInvalidAccessPolicyType, "accessPolicy");
            }
        }
    }
}
