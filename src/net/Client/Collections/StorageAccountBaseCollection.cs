//-----------------------------------------------------------------------
// <copyright file="StorageAccountBaseCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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


namespace Microsoft.WindowsAzure.MediaServices.Client
{
    public class StorageAccountBaseCollection:CloudBaseCollection<IStorageAccount>
    {
        /// <summary>
        /// The name of the storage account processor set.
        /// </summary>
        internal const string EntitySet = "StorageAccounts";


        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessorBaseCollection"/> class to be used for Mocking purposes.Introduced in version 3.0
        /// </summary>
        public StorageAccountBaseCollection()
            : base(null)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessorBaseCollection"/> class.
        /// </summary>
        /// <param name="mediaContext">The media context.</param>
        internal StorageAccountBaseCollection(MediaContextBase mediaContext)
            : base(mediaContext)
        {
            MediaContext = mediaContext;
            MediaServicesClassFactory factory = MediaContext.MediaServicesClassFactory;
            Queryable = factory.CreateDataServiceContext().CreateQuery<IStorageAccount, StorageAccountData>(EntitySet);
        }
         
    }
}