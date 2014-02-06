//-----------------------------------------------------------------------
// <copyright file="ContentKeyData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a content key that can be used for encryption and decryption.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class ContentKeyData : BaseEntity<IContentKey>, IContentKey
    {
       /// <summary>
        /// Gets the clear key value.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;byte[]&gt;.</returns>
        public Task<byte[]> GetClearKeyValueAsync()
        {
            // Start a new task here because the ExecutAsync on the DataContext returns a Task<string>
            return System.Threading.Tasks.Task.Factory.StartNew<byte[]>(() =>
            {
                byte[] returnValue = null;
                if (this.GetMediaContext() != null)
                {
                    Uri uriRebindContentKey = new Uri(string.Format(CultureInfo.InvariantCulture, "/RebindContentKey?id='{0}'&x509Certificate=''", this.Id), UriKind.Relative);
                    IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

                    MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetQueryRetryPolicy();

                    IEnumerable<string> results = retryPolicy.ExecuteAction<IEnumerable<string>>(() => dataContext.Execute<string>(uriRebindContentKey));
                    string reboundContentKey = results.Single();

                    returnValue = Convert.FromBase64String(reboundContentKey);
                }

                return returnValue;
            });
        }

        /// <summary>
        /// Gets the clear key value.
        /// </summary>
        /// <returns>The clear key value.</returns>
        public byte[] GetClearKeyValue()
        {
            try
            {
                Task<byte[]> task = this.GetClearKeyValueAsync();
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }    
        }

        /// <summary>
        /// Gets the encrypted key value.
        /// </summary>
        /// <param name="certToEncryptTo">The cert to use.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task&lt;byte[]&gt;.</returns>
        public Task<byte[]> GetEncryptedKeyValueAsync(X509Certificate2 certToEncryptTo)
        {
            if (certToEncryptTo == null)
            {
                throw new ArgumentNullException("certToEncryptTo");
            }

            // Start a new task here because the ExecutAsync on the DataContext returns a Task<string>
            return System.Threading.Tasks.Task.Factory.StartNew<byte[]>(() =>
                {
                    byte[] returnValue = null;

                    if (this.GetMediaContext() != null)
                    {
                        string certToSend = Convert.ToBase64String(certToEncryptTo.Export(X509ContentType.Cert));
                        certToSend = HttpUtility.UrlEncode(certToSend);

                        Uri uriRebindContentKey = new Uri(string.Format(CultureInfo.InvariantCulture, "/RebindContentKey?id='{0}'&x509Certificate='{1}'", this.Id, certToSend), UriKind.Relative);
                        IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();

                        MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetQueryRetryPolicy();

                        IEnumerable<string> results = retryPolicy.ExecuteAction<IEnumerable<string>>(() => dataContext.Execute<string>(uriRebindContentKey));

                        string reboundContentKey = results.Single();

                        returnValue = Convert.FromBase64String(reboundContentKey);
                    }

                    return returnValue;
                });
        }

        /// <summary>
        /// Gets the encrypted key value.
        /// </summary>
        /// <param name="certToEncryptTo">The cert to use.</param>
        /// <returns>The encrypted key value.</returns>
        public byte[] GetEncryptedKeyValue(X509Certificate2 certToEncryptTo)
        {
            try
            {
                Task<byte[]> task = this.GetEncryptedKeyValueAsync(certToEncryptTo);
                task.Wait();

                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns>A function delegate.</returns>
        public Task DeleteAsync()
        {
            ContentKeyBaseCollection.VerifyContentKey(this);

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyBaseCollection.ContentKeySet, this);
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
        /// Updates this instance asynchronously.
        /// </summary>
        /// <returns></returns>
        public Task<IContentKey> UpdateAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyBaseCollection.ContentKeySet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                    .ContinueWith<IContentKey>(
                    t =>
                    {
                        var data = (ContentKeyData)t.Result.AsyncState;
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
                var asset = UpdateAsync().Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten().InnerException;
            }
        }

        private static ContentKeyType GetExposedContentKeyType(int contentKeyType)
        {
            return (ContentKeyType)contentKeyType;
        }

        private static ProtectionKeyType GetExposedProtectionKeyType(int protectionKeyType)
        {
            return (ProtectionKeyType)protectionKeyType;
        }
    }
}
