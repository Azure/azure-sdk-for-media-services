//-----------------------------------------------------------------------
// <copyright file="EncodingReservedUnit.cs" company="Microsoft">Copyright 2014 Microsoft Corporation</copyright>
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
    ///     Represents Azure Encoding Reserved Unit in a system
    /// </summary>
    [DataServiceKey("AccountId")]
    internal class EncodingReservedUnitData : BaseEntity<IEncodingReservedUnit>, IEncodingReservedUnit
    {
        public Guid AccountId { get; set; }
        /// <summary>
        /// Gets or sets the ReservedUnitType.
        /// </summary>
        public int ReservedUnitType { get; set; }
        /// <summary>
        ///  Gets or sets the ReservedUnitType.
        /// </summary>
        ReservedUnitType IEncodingReservedUnit.ReservedUnitType
        {
            get
            {
                return (ReservedUnitType)this.ReservedUnitType;
            }
            set
            {
                this.ReservedUnitType = (int)value;
            }
        }

        /// <summary>
        ///     Maximum Number of Reservable units of this encoding type
        /// </summary>
        public int MaxReservableUnits { get; set; }

        /// <summary>
        ///     Current Number of Reservable units of this encoding type
        /// </summary>
        public int CurrentReservedUnits { get; set; }

        /// <summary>
        /// Updates this instance asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task<IEncodingReservedUnit> UpdateAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(EncodingReservedUnitCollection.EncodingReservedUnitSet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                    .ContinueWith<IEncodingReservedUnit>(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        var data = (EncodingReservedUnitData)t.Result.AsyncState;
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
                var result = UpdateAsync().Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten().InnerException;
            }
        }
    }
}
