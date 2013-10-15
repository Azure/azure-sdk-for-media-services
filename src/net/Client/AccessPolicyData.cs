//-----------------------------------------------------------------------
// <copyright file="AccessPolicyData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Common;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Defines an access policy to an <see cref="IAsset"/> in the system.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class AccessPolicyData :BaseEntity<IAccessPolicy>, IAccessPolicy
    {

        /// <summary>
        /// Gets or sets the duration in minutes.
        /// </summary>
        /// <value>
        /// The duration in minutes.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Remains for interface compatibility.")]
        public double DurationInMinutes { get; set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        TimeSpan IAccessPolicy.Duration
        {
            get
            {
                return GetExposedDuration(this.DurationInMinutes);
            }
        }

        

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            AccessPolicyBaseCollection.VerifyAccessPolicy(this);

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AccessPolicyBaseCollection.AccessPolicySet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            try
            {
                this.DeleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The duration in minutes.</returns>
        internal static double GetInternalDuration(TimeSpan value)
        {
            return value.TotalMinutes;
        }

        /// <summary>
        /// Gets the permissions.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The access permisisons value.</returns>
        internal static int GetInternalPermissions(AccessPermissions value)
        {
            return (int)value;
        }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <param name="duration">The duration value.</param>
        /// <returns>The duration.</returns>
        private static TimeSpan GetExposedDuration(double duration)
        {
            return TimeSpan.FromMinutes(duration);
        }

        /// <summary>
        /// Gets the permissions.
        /// </summary>
        /// <param name="permissions">The permissions.</param>
        /// <returns>The access permissions.</returns>
        private static AccessPermissions GetExposedPermissions(int permissions)
        {
            return (AccessPermissions)permissions;
        }
    }
}
