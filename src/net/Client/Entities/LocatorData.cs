//-----------------------------------------------------------------------
// <copyright file="LocatorData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents the application of an access policy to an asset.
    /// </summary>
    /// <remarks>A locator provides access to an asset using the <see cref="Path"/> property.</remarks>
    [DataServiceKey("Id")]
    internal partial class LocatorData : BaseEntity<ILocator>, ILocator
    {
        private AccessPolicyData _accessPolicy;
        private AssetData _asset;

        /// <summary>
        /// The prefix for the locator Id.
        /// </summary>
        internal const string LocatorIdentifierPrefix = "nb:lid:UUID:";

        /// <summary>
        /// Gets or sets the <see cref="IAccessPolicy"/> that defines this locator.
        /// </summary>
        public AccessPolicyData AccessPolicy
        {
            get { return this._accessPolicy; }
            set { this._accessPolicy = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAsset"/> that this locator is attached to.
        /// </summary>
        public AssetData Asset
        {
            get { return this._asset; }
            set { this._asset = value; }
        }

        /// <summary>
        /// Gets the <see cref="IAccessPolicy"/> that defines this locator.
        /// </summary>
        IAccessPolicy ILocator.AccessPolicy
        {
            get
            {
                if ((this._accessPolicy == null) && !String.IsNullOrWhiteSpace(this.Id))
                {
                    IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(LocatorBaseCollection.LocatorSet, this);
                    LoadProperty(dataContext, LocatorBaseCollection.AccessPolicyPropertyName);
                }

                return this._accessPolicy;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAsset"/> that this locator is attached to.
        /// </summary>
        IAsset ILocator.Asset
        {
            get
            {
                if ((this._asset == null) && !String.IsNullOrWhiteSpace(this.Id))
                {
                    IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(LocatorBaseCollection.LocatorSet, this);
                    LoadProperty(dataContext, LocatorBaseCollection.AssetPropertyName);
                }

                return this._asset;
            }
        }

        /// <summary>
        /// Asynchronously updates the expiration time of an Origin locator.
        /// </summary>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        public Task UpdateAsync(DateTime expiryTime)
        {
            return this.UpdateAsync(this.StartTime, expiryTime: expiryTime);
        }

        /// <summary>
        /// Updates the expiration time of an Origin locator.
        /// </summary>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        public void Update(DateTime expiryTime)
        {
            this.Update(this.StartTime, expiryTime: expiryTime);
        }

        /// <summary>
        /// Asynchronously updates the start time or expiration time of an Origin locator.
        /// </summary>
        /// <param name="startTime">The new start time for the origin locator.</param>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        public Task UpdateAsync(DateTime? startTime, DateTime expiryTime)
        {
            LocatorBaseCollection.VerifyLocator(this);

            if (((ILocator)this).Type != LocatorType.OnDemandOrigin)
            {
                throw new InvalidOperationException(StringTable.InvalidOperationUpdateForNotOriginLocator);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(LocatorBaseCollection.LocatorSet, this);

            this.StartTime = startTime;
            this.ExpirationDateTime = expiryTime;

            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Updates the start time or expiration time of an Origin locator.
        /// </summary>
        /// <param name="startTime">The new start time for the origin locator.</param>
        /// <param name="expiryTime">The new expiration time for the origin locator.</param>
        /// <exception cref="InvalidOperationException">When locator is not an Origin Locator.</exception>
        public void Update(DateTime? startTime, DateTime expiryTime)
        {
            try
            {
                this.UpdateAsync(startTime: startTime, expiryTime: expiryTime).Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Asynchronously revokes the specified Locator, denying any access it provided.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        public Task DeleteAsync()
        {
            LocatorBaseCollection.VerifyLocator(this);

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(LocatorBaseCollection.LocatorSet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                .ContinueWith(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        LocatorData data = (LocatorData)t.Result.AsyncState;

                        if (GetMediaContext() != null)
                        {
                            var cloudContextAsset = (AssetData)GetMediaContext().Assets.Where(c => c.Id == data.AssetId).FirstOrDefault();
                            if (cloudContextAsset != null)
                            {
                                cloudContextAsset.InvalidateLocatorsCollection();
                            }
                        }

                        if (data.Asset != null)
                        {
                            data.Asset.InvalidateLocatorsCollection();
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        /// <summary>
        /// Deletes the specified Locator, revoking any access it provided.
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

        internal static string NormalizeLocatorId(string locatorId)
        {
            if (String.IsNullOrWhiteSpace(locatorId))
            {
                return null;
            }

            if (locatorId.StartsWith(LocatorIdentifierPrefix, StringComparison.OrdinalIgnoreCase))
            {
                locatorId = locatorId.Remove(0, LocatorIdentifierPrefix.Length);
            }

            Guid locatorIdGuid;
            if (!Guid.TryParse(locatorId, out locatorIdGuid))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.InvariantCulture, "Invalid locator Id. Make sure to use the following format: '{0}<GUID>'", LocatorIdentifierPrefix),
                    "locatorId");
            }

            return String.Concat((string)LocatorIdentifierPrefix, locatorIdGuid.ToString());
        }
        private static LocatorType GetExposedType(int type)
        {
            return (LocatorType)type;
        }
    }
}
