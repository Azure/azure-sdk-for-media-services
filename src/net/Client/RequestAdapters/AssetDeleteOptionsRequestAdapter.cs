﻿//-----------------------------------------------------------------------
// <copyright file="AssetDeleteOptionsRequestAdapter.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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

namespace Microsoft.WindowsAzure.MediaServices.Client.RequestAdapters
{
    public class AssetDeleteOptionsRequestAdapter: IDataServiceContextAdapter
    {
        private bool _keepAzureStorageContainer;

        public AssetDeleteOptionsRequestAdapter(bool keepAzureStorageContainer)
        {
            _keepAzureStorageContainer = keepAzureStorageContainer;
        }
        public void Adapt(DataServiceContext context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }
            context.BuildingRequest += this.AddAssetDeleteUriParameter;
        }

        private void AddAssetDeleteUriParameter(object sender, BuildingRequestEventArgs e)
        {
            UriBuilder builder = new UriBuilder(e.RequestUri);
            builder.Query = e.RequestUri.Query + "&keepcontainer=" + _keepAzureStorageContainer.ToString().ToLower();
            e.RequestUri = builder.Uri;
        }
    }
}