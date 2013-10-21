//-----------------------------------------------------------------------
// <copyright file="IngestManifestAssetData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Data.Services.Common;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal partial class IngestManifestAssetData : IIngestManifestAsset, ICloudMediaContextInit
    {
        private AssetData _asset;
        private IngestManifestFileCollection _filesCollection;
        private CloudMediaContext _cloudMediaContext;
        


        public IngestManifestAssetData()
        {
            Id = String.Empty;
        }

        #region ICloudMediaContextInit Members

        public void InitCloudMediaContext(CloudMediaContext context)
        {
            _cloudMediaContext = context;
        }

        #endregion

        #region IManifestAsset Members

        IngestManifestFileCollection IIngestManifestAsset.IngestManifestFiles
        {
            get
            {
                if ((_filesCollection == null) && !string.IsNullOrWhiteSpace(Id))
                {
                    _filesCollection = new IngestManifestFileCollection(_cloudMediaContext, this);
                }

                return _filesCollection;
            }
        }



        /// <summary>
        /// Deletes the manifest asset and manifest asset files asynchronously.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task DeleteAsync()
        {
            IMediaDataServiceContext dataContext = _cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(IngestManifestAssetCollection.EntitySet, this);
            dataContext.DeleteObject(this);
            return dataContext.SaveChangesAsync(this);
        }


        /// <summary>
        /// Deletes manifest asset and manifest asset files synchronously.
        /// </summary>
        public void Delete()
        {
            try
            {
                DeleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAsset"/> that this manifest asset is attached to.
        /// </summary>
        IAsset IIngestManifestAsset.Asset
        {
            get
            {
                if ((_asset == null) && !string.IsNullOrWhiteSpace(Id))
                {
                    IMediaDataServiceContext dataContext = _cloudMediaContext.MediaServicesClassFactory.CreateDataServiceContext();
                    dataContext.AttachTo(IngestManifestAssetCollection.EntitySet, this);
                    dataContext.LoadProperty(this, "Asset");
                }

                return _asset;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAsset"/> that this manifest asset is attached to.
        /// </summary>
        public AssetData Asset
        {
            get { return _asset; }
            set { _asset = value; }
        }
        #endregion
    }
}