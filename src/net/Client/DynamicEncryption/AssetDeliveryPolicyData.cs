//-----------------------------------------------------------------------
// <copyright file="DeliveryPolicyData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Common;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client.DynamicEncryption
{
    /// <summary>
    /// Describes the polices applied to assets for delivery.
    /// </summary>
    [DataServiceKey("Id")]
    internal class AssetDeliveryPolicyData : BaseEntity<IAssetDeliveryPolicy>, IAssetDeliveryPolicy 
    {
        /// <summary>
        /// Gets Unique identifier of the DeliveryPolicy.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets name of the policy.
        /// An optional friendly name for the policy. It can used by the policy 
        /// creator to help remember what the policy represents or is used for. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Delivery protocol.
        /// </summary>
        public int AssetDeliveryProtocol { get; set; }

        /// <summary>
        /// Delivery protocol.
        /// </summary>
        AssetDeliveryProtocol IAssetDeliveryPolicy.AssetDeliveryProtocol
        {
            get
            {
                return (AssetDeliveryProtocol)AssetDeliveryProtocol;
            }
            set
            {
                this.AssetDeliveryProtocol = (int)value;
            }

        }

        /// <summary>
        /// Policy type.
        /// </summary>
        public int AssetDeliveryPolicyType { get; set; }

        /// <summary>
        /// Policy type.
        /// </summary>
        AssetDeliveryPolicyType IAssetDeliveryPolicy.AssetDeliveryPolicyType
        {
            get
            {
                return (AssetDeliveryPolicyType)AssetDeliveryPolicyType;
            }
            set
            {
                this.AssetDeliveryPolicyType = (int)value;
            }

        }

        /// <summary>
        /// Mapping from the way of obtaining a configuration to the configuration string.
        /// </summary>
        public string AssetDeliveryConfiguration 
        {
            get
            {
                IAssetDeliveryPolicy self = this;

                if (self.AssetDeliveryConfiguration == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    var serializer = new DataContractJsonSerializer(typeof(Dictionary<AssetDeliveryPolicyConfigurationKey, string>));

                    serializer.WriteObject(ms, self.AssetDeliveryConfiguration);

                    var result = Encoding.UTF8.GetString(ms.ToArray());

                    return result;
                }
            }
            set
            {
                IAssetDeliveryPolicy self = this;

                if (string.IsNullOrWhiteSpace(value))
                {
                    self.AssetDeliveryConfiguration = null;
                    return;
                }

                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(Dictionary<AssetDeliveryPolicyConfigurationKey, string>));

                    self.AssetDeliveryConfiguration = serializer.ReadObject(ms) as Dictionary<AssetDeliveryPolicyConfigurationKey, string>;
                }
            }
        }

        /// <summary>
        /// Mapping from the way of obtaining a configuration to the configuration string.
        /// </summary>
        IDictionary<AssetDeliveryPolicyConfigurationKey, string> IAssetDeliveryPolicy.AssetDeliveryConfiguration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetDeliveryPolicyData"/> class.
        /// </summary>
        public AssetDeliveryPolicyData()
        {
        }

        /// <summary>
        /// Asynchronously updates this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task<IAssetDeliveryPolicy> UpdateAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(AssetDeliveryPolicyCollection.DeliveryPolicySet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                .ContinueWith<IAssetDeliveryPolicy>(
                t =>
                {
                    t.ThrowIfFaulted();
                    var data = (AssetDeliveryPolicyData)t.Result.AsyncState;
                    return data;
                });
        }

        /// <summary>
        /// Updates this instance.
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
            dataContext.AttachTo(AssetDeliveryPolicyCollection.DeliveryPolicySet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

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
    }
}
