//-----------------------------------------------------------------------
// <copyright file="IngestManifestCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents <see cref="IQueryable"/> collection of <see cref="IIngestManifest"/>. 
    /// </summary>
    public class IngestManifestCollection : CloudBaseCollection<IIngestManifest>
    {

        /// <summary>
        /// The name of the entity set.
        /// </summary>
        internal const string EntitySet = "IngestManifests";

        /// <summary>
        /// Initializes a new instance of the <see cref="IngestManifestCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "By design")]
        internal IngestManifestCollection(MediaContextBase cloudMediaContext)
            : base(cloudMediaContext)
        {
            this.Queryable = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext().CreateQuery<IngestManifestData>(EntitySet);
        }


        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><see cref="IIngestManifest"/></returns>
        public IIngestManifest Create(string name)
        {
            return Create(name, this.MediaContext.DefaultStorageAccount.Name);
        }

        /// <summary>
        /// Creates the manifest async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><see cref="Task"/> of type <see cref="IIngestManifest"/></returns>
        public Task<IIngestManifest> CreateAsync(string name)
        {
            return CreateAsync(name, this.MediaContext.DefaultStorageAccount.Name);
        }

        /// <summary>
        /// Creates the manifest async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="storageAccountName">The name of storage account </param>
        /// <returns><see cref="Task"/> of type <see cref="IIngestManifest"/></returns>
        public Task<IIngestManifest> CreateAsync(string name,string storageAccountName)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (storageAccountName == null) throw new ArgumentNullException("storageAccountName");

            IngestManifestData ingestManifestData = new IngestManifestData
                                    {
                                        Name = name,
                                        StorageAccountName = storageAccountName
                                    };


            ingestManifestData.SetMediaContext(this.MediaContext);
            IMediaDataServiceContext dataContext = this.MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AddObject(EntitySet, ingestManifestData);

            return dataContext
                .SaveChangesAsync(ingestManifestData)
                .ContinueWith<IIngestManifest>(
                    t =>
                    {
                        t.ThrowIfFaulted();
                        IngestManifestData data = (IngestManifestData)t.Result.AsyncState;
                        return data;

                    });
        }

        /// <summary>
        /// Verifies the manifest.
        /// </summary>
        /// <param name="ingestManifest">The manifest to verify.</param>
        internal static void VerifyManifest(IIngestManifest ingestManifest)
        {
            if (ingestManifest == null)
            {
                throw new ArgumentNullException("ingestManifest");
            }

            if (!(ingestManifest is IngestManifestData))
            {
                throw new InvalidCastException(StringTable.ErrorInvalidManifestType);
            }
        }

        /// <summary>
        /// Creates the manifest
        /// </summary>
        /// <param name="manifestName">Name of the manifest.</param>
        /// <param name="storageAccountName">Name of the storage account.</param>
        /// <returns></returns>
        public IIngestManifest Create(string manifestName, string storageAccountName)
        {
            try
            {
                Task<IIngestManifest> task = this.CreateAsync(manifestName, storageAccountName);
                task.Wait();
                return task.Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }
    }
}