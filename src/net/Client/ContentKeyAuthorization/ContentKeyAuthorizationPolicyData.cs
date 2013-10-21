//-----------------------------------------------------------------------
// <copyright file="ContentKeyAuthorizationPolicyData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client.ContentKeyAuthorization
{
    /// <summary>
    ///     Defines ContentKeyAuthorizationPolicy
    /// </summary>
    [DataServiceKey("Id")]
    internal class ContentKeyAuthorizationPolicyData : IContentKeyAuthorizationPolicy, ICloudMediaContextInit
    {
        public const string OptionsPropertyName = "Options";
        private readonly object _lock = new object();
        private CloudMediaContext _cloudMediaContext;
        private IList<IContentKeyAuthorizationPolicyOption> _optionsCollection;

        public ContentKeyAuthorizationPolicyData()
        {
            Options = new List<ContentKeyAuthorizationPolicyOptionData>();
        }

        /// <summary>
        ///     Gets or sets the options
        /// </summary>
        /// <value>
        ///     The locators.
        /// </value>
        public List<ContentKeyAuthorizationPolicyOptionData> Options { get; set; }

        /// <summary>
        ///     Initializes the instance with a <see cref="CloudMediaContext" />.
        /// </summary>
        /// <param name="context">The <see cref="CloudMediaContext" /> instance.</param>
        public void InitCloudMediaContext(CloudMediaContext context)
        {
            _cloudMediaContext = context;
        }

        /// <summary>
        ///     Gets the id.
        /// </summary>
        /// <value>
        ///     The id.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Asynchronously updates this instance.
        /// </summary>
        /// <returns>
        ///     Task to wait on for operation completion.
        /// </returns>
        public Task<IContentKeyAuthorizationPolicy> UpdateAsync()
        {
            IMediaDataServiceContext dataContext = _cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyAuthorizationPolicyCollection.ContentKeyAuthorizationPolicySet, this);
            dataContext.UpdateObject(this);

            return dataContext.SaveChangesAsync(this).ContinueWith<IContentKeyAuthorizationPolicy>(t =>
            {
                IMediaDataServiceResponse response = t.Result;
                return (ContentKeyAuthorizationPolicyData) t.AsyncState;
            });
        }

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        public void Update()
        {
            try
            {
                Task<IContentKeyAuthorizationPolicy> t = UpdateAsync();
                IContentKeyAuthorizationPolicy updated = t.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten();
            }
        }

        /// <summary>
        ///     Deletes the IContentKeyAuthorizationPolicy.
        /// </summary>
        public void Delete()
        {
            try
            {
                Task<IMediaDataServiceResponse> t = DeleteAsync();
                IMediaDataServiceResponse updated = t.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten();
            }
        }

        /// <summary>
        ///     Deletes the IContentKeyAuthorizationPolicy asynchronously.
        /// </summary>
        /// <returns>
        ///     Task to wait on for operation completion.
        /// </returns>
        public Task<IMediaDataServiceResponse> DeleteAsync()
        {
            IMediaDataServiceContext dataContext = _cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(ContentKeyAuthorizationPolicyCollection.ContentKeyAuthorizationPolicySet, this);
            dataContext.DeleteObject(this);
            return dataContext.SaveChangesAsync(this);
        }


        /// <summary>
        ///     Gets a collection of <see cref="IContentKeyAuthorizationPolicyOption" /> contained by the
        ///     <see cref="IContentKeyAuthorizationPolicy" />
        /// </summary>
        /// <value>
        ///     A collection of files contained by the Asset.
        /// </value>
        IList<IContentKeyAuthorizationPolicyOption> IContentKeyAuthorizationPolicy.Options
        {
            get
            {
                lock (_lock)
                {
                    if ((_optionsCollection == null) && !string.IsNullOrWhiteSpace(Id))
                    {
                        IMediaDataServiceContext dataContext = _cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                        dataContext.AttachTo(ContentKeyAuthorizationPolicyCollection.ContentKeyAuthorizationPolicySet, this);
                        dataContext.LoadProperty(this, OptionsPropertyName);
                        _optionsCollection = new LinkCollection<IContentKeyAuthorizationPolicyOption, ContentKeyAuthorizationPolicyOptionData>(dataContext, this, OptionsPropertyName, Options);
                    }

                    return _optionsCollection;
                }
            }
        }
    }
}