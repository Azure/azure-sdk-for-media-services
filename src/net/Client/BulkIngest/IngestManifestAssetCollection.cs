//-----------------------------------------------------------------------
// <copyright file="IngestManifestAssetCollection.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents a collection of <see cref="IIngestManifestAsset"/>.
    /// </summary>
    public class IngestManifestAssetCollection : BaseCollection<IIngestManifestAsset>
    {
        internal const string EntitySet = "IngestManifestAssets";
        private readonly IMediaDataServiceContext _dataContext;
        private readonly IIngestManifest _parentIngestManifest;
        private readonly Lazy<IQueryable<IIngestManifestAsset>> _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="IngestManifestAssetCollection"/> class.
        /// </summary>
        /// <param name="cloudMediaContext">The <seealso cref="CloudMediaContext"/> instance.</param>
        /// <param name="parentIngestManifest">parent manifest if collection associated with manifest </param>
        internal IngestManifestAssetCollection(MediaContextBase cloudMediaContext, IIngestManifest parentIngestManifest):base(cloudMediaContext)
        {
             MediaContext = cloudMediaContext;
            _dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            _parentIngestManifest = parentIngestManifest;
            _query = new Lazy<IQueryable<IIngestManifestAsset>>(() => _dataContext.CreateQuery<IngestManifestAssetData>(EntitySet));
        }

        /// <summary>
        /// Gets the queryable collection of <see cref="IIngestManifestAsset"/>.
        /// </summary>
        protected override IQueryable<IIngestManifestAsset> Queryable
        {
            get
            {
                if (_parentIngestManifest != null)
                {
                    return _query.Value.Where(c => c.ParentIngestManifestId == _parentIngestManifest.Id);
                }
                else
                {
                    return _query.Value;
                }
            }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Creates the manifest asset syncroniously. 
        /// Overloaded metho with IManifest parameter need to be used,if collection is used from CloudMediaContext.
        /// </summary>
        /// <param name="asset">The destination asset where all manifest asset files will be uploaded</param>
        /// <param name="files">The files.</param>
        /// <returns><see cref="IIngestManifestAsset"/></returns>
        public IIngestManifestAsset Create(IAsset asset, string[] files)
        {
            if (_parentIngestManifest ==null)
            {
                throw new InvalidOperationException(StringTable.InvalidCreateManifestAssetOperation);
            }
            return CreateAsync(_parentIngestManifest, asset, files, CancellationToken.None).Result;
        }


       
        /// <summary>
        /// Creates the manifest asset asyncroniously
        /// Overloaded metho with IManifest parameter need to be used,if collection is used from CloudMediaContext.
        /// </summary>
        /// <param name="asset">The destination asset for which uploaded and processed files will be associated.</param>
        /// <param name="files">The files which needs to be uploaded and processed.</param>
        /// <param name="token"><see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/> of type <see cref="IIngestManifestAsset"/></returns>
        public Task<IIngestManifestAsset> CreateAsync(IAsset asset, string[] files, CancellationToken token)
        {
            if (_parentIngestManifest == null)
            {
                throw new InvalidOperationException(StringTable.InvalidCreateManifestAssetOperation);
            }
            return CreateAsync(_parentIngestManifest, asset, files, token);
        }

        /// <summary>
        /// Creates the manifest asset asyncroniously
        /// </summary>
        /// <param name="ingestManifest">The manifest where asset will be defined.</param>
        /// <param name="asset">The destination asset for which uploaded and processed files will be associated.</param>
        /// <param name="files">The files which needs to be uploaded and processed.</param>
        /// <param name="token"><see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Task"/> of type <see cref="IIngestManifestAsset"/></returns>
        private Task<IIngestManifestAsset> CreateAsync(IIngestManifest ingestManifest, IAsset asset, string[] files, CancellationToken token)
        {
            if (ingestManifest == null)
            {
                throw new ArgumentNullException("ingestManifest");
            }
            Action<IngestManifestAssetData> continueWith = (IngestManifestAssetData manifestData) =>
            {
                Task<IIngestManifestFile>[] tasks = new Task<IIngestManifestFile>[files.Count()];
                int i = 0;

                foreach (string file in files)
                {
                    token.ThrowIfCancellationRequested();

                    tasks[i] = ((IIngestManifestAsset)manifestData).IngestManifestFiles.CreateAsync(file, token);

                    i++;
                }

                Task continueTask = Task.Factory.ContinueWhenAll(
                    tasks,
                    (fileTasks) =>
                    {   
                        //Updating statistic
                        var _this = MediaContext.IngestManifests.Where(c => c.Id == _parentIngestManifest.Id).FirstOrDefault();
                        if (_this != null)
                        {
                            ((IngestManifestData)_parentIngestManifest).Statistics = _this.Statistics;
                        } 
                        else
                        {
                            throw new DataServiceClientException(String.Format(CultureInfo.InvariantCulture, StringTable.BulkIngestManifest404,_parentIngestManifest.Id),404);
                        }

                        List<Exception> exceptions = new List<Exception>();

                        foreach (Task<IIngestManifestFile> task in fileTasks)
                        {
                            if (task.IsFaulted)
                            {
                                if (task.Exception != null) exceptions.AddRange(task.Exception.InnerExceptions); 
                                continue;
                            }
                            if (task.IsCanceled)
                            {
                                if (task.Exception != null)
                                {
                                    exceptions.Add(task.Exception.Flatten());
                                }
                            }
                            IngestManifestData ingestManifestData = ((IngestManifestData)ingestManifest);
                            if (task.Result != null)
                            {
                                if (!ingestManifestData.TrackedFilesPaths.ContainsKey(task.Result.Id))
                                {
                                    ingestManifestData.TrackedFilesPaths.TryAdd(task.Result.Id, ((IngestManifestFileData) task.Result).Path);
                                }
                                else
                                {
                                    ingestManifestData.TrackedFilesPaths[task.Result.Id] = ((IngestManifestFileData) task.Result).Path;
                                }
                            } 
                            else
                            {
                                //We should not be here if task successfully completed and not cancelled
                                exceptions.Add(new NullReferenceException(StringTable.BulkIngestNREForFileTaskCreation));
                            }
                           
                        }

                        if (exceptions.Count > 0)
                        {
                            var exception = new AggregateException(exceptions.ToArray());
                            throw exception;
                        }
                    },
                TaskContinuationOptions.ExecuteSynchronously);
                continueTask.Wait();
            };

            return CreateAsync(ingestManifest, asset, token, continueWith);

        }
      

        private Task<IIngestManifestAsset> CreateAsync(IIngestManifest ingestManifest, IAsset asset, CancellationToken token, Action<IngestManifestAssetData> continueWith)
        {
            IngestManifestCollection.VerifyManifest(ingestManifest);

            IMediaDataServiceContext dataContext = MediaContext.MediaServicesClassFactory.CreateDataServiceContext();
            var data = new IngestManifestAssetData
                           {
                               ParentIngestManifestId = ingestManifest.Id
                               
                           };

            
            dataContext.AddObject(IngestManifestAssetCollection.EntitySet,data);
            dataContext.AttachTo(AssetCollection.AssetSet, asset);
            dataContext.SetLink(data,"Asset",asset);

            Task<IIngestManifestAsset> task = dataContext.SaveChangesAsync(data).ContinueWith<IIngestManifestAsset>(t =>
                                                                                                            {
                                                                                                                t.ThrowIfFaulted();
                                                                                                                token.ThrowIfCancellationRequested();
                                                                                                                IngestManifestAssetData ingestManifestAsset = (IngestManifestAssetData)t.Result.AsyncState;
                                                                                                                continueWith(ingestManifestAsset);
                                                                                                                return ingestManifestAsset;
                                                                                                            }, TaskContinuationOptions.ExecuteSynchronously);

            return task;
        }

        internal static void VerifyManifestAsset(IIngestManifestAsset ingestManifestAsset)
        {
            if(ingestManifestAsset == null)
            {
                throw new ArgumentNullException("ingestManifestAsset");
            }
        }

        /// <summary>
        /// Creates the empty manifest asset asyncroniosly.
        /// </summary>
        /// <param name="asset">The destination asset for which uploaded and processed files will be associated </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> of type <see cref="IIngestManifestAsset"/></returns>
        public Task<IIngestManifestAsset> CreateAsync(IAsset asset,CancellationToken cancellationToken)
        {
            if (_parentIngestManifest == null)
            {
                throw new InvalidOperationException(StringTable.InvalidCreateManifestAssetOperation);
            }
            return CreateAsync(_parentIngestManifest, asset, cancellationToken, (IngestManifestAssetData manifestData) => { });
        }

      
    }
}