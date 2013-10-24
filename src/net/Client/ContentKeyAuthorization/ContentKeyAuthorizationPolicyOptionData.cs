//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyOptionData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Threading.Tasks;
using System.Data.Services.Client;
using System.Data.Services.Common;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    /// A list of different ways a client can be authorized to access the content key.
    /// </summary>
    [DataServiceKey("Id")]
    internal class ContentKeyAuthorizationPolicyOptionData : BaseEntity<IContentKeyAuthorizationPolicyOption>, IContentKeyAuthorizationPolicyOption 
    {
        /// <summary>
        /// Gets Unique identifier of the ContentKeyAuthorizationPolicyOption.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of ContentKeyAuthorizationPolicyOption.
        /// An optional friendly name for the policy. It can used by the policy 
        /// creator to help remember what the policy represents or is used for. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the delivery method of the content key to the client.
        /// </summary>
        public int KeyDeliveryType { get; set; }

        /// <summary>
        /// Gets or sets the delivery method of the content key to the client.
        /// </summary>
        ContentKeyDeliveryType IContentKeyAuthorizationPolicyOption.KeyDeliveryType 
        {
            get
            {
                return (ContentKeyDeliveryType)this.KeyDeliveryType;
            }
            set
            {
                this.KeyDeliveryType = (int)value;
            }
        }

        /// <summary>
        /// Xml data, specific to the key delivery type that defines how the key is delivered to the client.
        /// </summary>
        public string KeyDeliveryConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the restrictions.
        /// The requirements of each  restriction MUST be met in order to deliver the key using the key delivery data. 
        /// </summary>
        public List<ContentKeyAuthorizationPolicyRestriction> Restrictions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentKeyAuthorizationPolicyOptionData"/> class.
        /// </summary>
        public ContentKeyAuthorizationPolicyOptionData()
        {
            this.Restrictions = new List<ContentKeyAuthorizationPolicyRestriction>();
        }

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IContentKeyAuthorizationPolicyOption> UpdateAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyAuthorizationPolicyOptionCollection.ContentKeyAuthorizationPolicyOptionSet, this);
            dataContext.UpdateObject(this);

            return dataContext.SaveChangesAsync(this).ContinueWith<IContentKeyAuthorizationPolicyOption>(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        var data = (ContentKeyAuthorizationPolicyOptionData)t.Result.AsyncState;
                        return data;
                    });
        }

        /// <summary>
        /// Updates this asset instance.
        /// </summary>  
        public void Update()
        {
            try
            {
                this.UpdateAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Asynchronously deletes this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IMediaDataServiceResponse> DeleteAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyAuthorizationPolicyOptionCollection.ContentKeyAuthorizationPolicyOptionSet, this);
            dataContext.DeleteObject(this);

            return dataContext.SaveChangesAsync(this);
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
    }
}
