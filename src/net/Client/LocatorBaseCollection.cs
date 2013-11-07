//-----------------------------------------------------------------------
// <copyright file="LocatorCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
    /// Represents a collection of <see cref="ILocator"/>.
    /// </summary>
    public class LocatorBaseCollection : CloudBaseCollection<ILocator>
    {
        /// <summary>
        /// The entity set name for locators.
        /// </summary>
        internal const string LocatorSet = "Locators";

        /// <summary>
        /// The name of the AccessPolicy property of the <see cref="ILocator"/>.
        /// </summary>
        internal const string AccessPolicyPropertyName = "AccessPolicy";

        /// <summary>
        /// The name of the Asset property of the <see cref="ILocator"/>.
        /// </summary>
        internal const string AssetPropertyName = "Asset";

       

        /// <summary>
        /// Initializes a new instance of the <see cref="LocatorBaseCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "By design")]
        internal LocatorBaseCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
           this.Queryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<LocatorData>(LocatorSet);
        }

        /// <summary>
        /// Asynchronously creates a SAS Locator with the specified access policy and asset.
        /// </summary>
        /// <param name="asset">The asset to create a SAS Locator for.</param>
        /// <param name="accessPolicy">The AccessPolicy that governs access for the locator.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="asset"/> is null.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="accessPolicy"/> is null.</exception>
        public Task<ILocator> CreateSasLocatorAsync(IAsset asset, IAccessPolicy accessPolicy)
        {
            return this.CreateSasLocatorAsync(asset, accessPolicy, null);
        }

        /// <summary>
        /// Creates a SAS Locator with the specified access policy and asset.
        /// </summary>
        /// <param name="asset">The asset to create a SAS Locator for.</param>
        /// <param name="accessPolicy">The AccessPolicy that governs access for the locator.</param>
        /// <returns>A locator granting access specified by <paramref name="accessPolicy" /> to the provided <paramref name="asset" />.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="asset"/> is null.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="accessPolicy"/> is null.</exception>
        public ILocator CreateSasLocator(IAsset asset, IAccessPolicy accessPolicy)
        {
            return this.CreateSasLocator(asset, accessPolicy, null);
        }

      

       

        /// <summary>
        /// Creates a SAS Locator with the specified access policy and asset.
        /// </summary>
        /// <param name="asset">The asset to create a SAS Locator for.</param>
        /// <param name="accessPolicy">The AccessPolicy that governs access for the locator.</param>
        /// <param name="startTime">The access start time of the locator.</param>
        /// <param name="name">The locator name.</param>
        /// <returns>
        /// A locator granting access specified by <paramref name="accessPolicy" /> to the provided <paramref name="asset" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="asset" /> is null.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="asset" /> is null.</exception>
        public ILocator CreateSasLocator(IAsset asset, IAccessPolicy accessPolicy, DateTime? startTime, string name =null)
        {
            return this.CreateLocator(LocatorType.Sas, asset, accessPolicy, startTime, name);
        }

        /// <summary>
        /// Asynchronously creates a SAS Locator with the specified access policy and asset.
        /// </summary>
        /// <param name="asset">The asset to create a SAS Locator for.</param>
        /// <param name="accessPolicy">The AccessPolicy that governs access for the locator.</param>
        /// <param name="startTime">The access start time of the locator.</param>
        /// <param name="name">The locator name.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="asset" /> is null.</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="asset" /> is null.</exception>
        public Task<ILocator> CreateSasLocatorAsync(IAsset asset, IAccessPolicy accessPolicy, DateTime? startTime, string name = null)
        {
            return this.CreateLocatorAsync(LocatorType.Sas, asset, accessPolicy, startTime, name);
        }

       

        /// <summary>
        /// Verifies the locator.
        /// </summary>
        /// <param name="locator">The locator.</param>
        internal static void VerifyLocator(ILocator locator)
        {
            if (locator == null)
            {
                throw new ArgumentNullException("locator");
            }

            if (!(locator is LocatorData))
            {
                throw new ArgumentException(StringTable.InvalidLocatorType, "locator");
            }
        }

        /// <summary>
        /// Creates the locator async.
        /// </summary>
        /// <param name="locatorType">Type of the locator.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="accessPolicy">The access policy.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task&lt;ILocator&gt;.
        /// </returns>
        public Task<ILocator> CreateLocatorAsync(LocatorType locatorType, IAsset asset, IAccessPolicy accessPolicy, DateTime? startTime, string name =null)
        {
            AccessPolicyBaseCollection.VerifyAccessPolicy(accessPolicy);
            AssetCollection.VerifyAsset(asset);

            AssetData assetData = (AssetData)asset;

            LocatorData locator = new LocatorData
            {
                AccessPolicy = (AccessPolicyData)accessPolicy,
                Asset = assetData,
                Type = (int)locatorType,
                StartTime = startTime,
                Name = name
            };

            locator.SetMediaContext(this.MediaContext);
            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AssetCollection.AssetSet, asset);
            dataContext.AttachTo(AccessPolicyBaseCollection.AccessPolicySet, accessPolicy);
            dataContext.AddObject(LocatorSet, locator);
            dataContext.SetLink(locator, AccessPolicyPropertyName, accessPolicy);
            dataContext.SetLink(locator, AssetPropertyName, asset);

            MediaRetryPolicy retryPolicy = this.MediaContext.MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(locator))
                .ContinueWith<ILocator>(
                    t =>
                    {
                        t.ThrowIfFaulted();

                        assetData.InvalidateLocatorsCollection();

                        return (LocatorData)t.Result.AsyncState;
                    },
                    TaskContinuationOptions.ExecuteSynchronously);
        }

        public Task<ILocator> CreateLocatorAsync(LocatorType locatorType, IAsset asset, IAccessPolicy accessPolicy)
        {
            return CreateLocatorAsync(locatorType, asset, accessPolicy, startTime: null, name:null);
        }

        /// <summary>
        /// Creates the locator.
        /// </summary>
        /// <param name="locatorType">Type of the locator.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="accessPolicy">The access policy.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// A locator enabling streaming access to the specified <paramref name="asset" />.
        /// </returns>
        public ILocator CreateLocator(LocatorType locatorType, IAsset asset, IAccessPolicy accessPolicy, DateTime? startTime, string name =null)
        {
            try
            {
                Task<ILocator> task = this.CreateLocatorAsync(locatorType, asset, accessPolicy, startTime, name);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public ILocator CreateLocator(LocatorType locatorType, IAsset asset, IAccessPolicy accessPolicy)
         {
             return CreateLocator(locatorType, asset, accessPolicy,startTime: null);
         }

        
    }
}
