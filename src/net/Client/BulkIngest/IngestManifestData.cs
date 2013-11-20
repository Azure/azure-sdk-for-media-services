//-----------------------------------------------------------------------
// <copyright file="IngestManifestData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    [DataServiceKey("Id")]
    internal partial class IngestManifestData : BaseEntity<IIngestManifest>, IIngestManifest
    {
        //private const int AcquringLockMillisecondsTimeout = 100000;
        internal static int MaxNumberOfEncryptionThreadsForFilePerCore = 5;
        private IngestManifestAssetCollection _assetsCollection;
        internal readonly ConcurrentDictionary<string, string> TrackedFilesPaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="IngestManifestData"/> class.
        /// </summary>
        public IngestManifestData()
        {
            Id = String.Empty;
            Statistics = new IngestIngestManifestStatistics();
            TrackedFilesPaths = new ConcurrentDictionary<string, string>();
        }


        /// <summary>
        /// Gets the manifest assets.
        /// </summary>
        IngestManifestAssetCollection IIngestManifest.IngestManifestAssets
        {
            get
            {
                if ((_assetsCollection == null) && !string.IsNullOrWhiteSpace(Id))
                {
                    _assetsCollection = new IngestManifestAssetCollection(GetMediaContext(), this);
                }

                return _assetsCollection;
            }
        }

        /// <summary>
        /// Deletes manifest
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
        /// Deletes manifest asynchronously.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task DeleteAsync()
        {
            IngestManifestCollection.VerifyManifest(this);

            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(IngestManifestCollection.EntitySet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }


        /// <summary>
        /// Encrypts manifest files asyncroniously.
        /// </summary>
        /// <param name="outputPath">The output path where all encrypted files will be located.</param>
        /// <param name="overwriteExistingEncryptedFiles">if set to <c>true</c> method will override files in ouput folder.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        public Task EncryptFilesAsync(string outputPath, bool overwriteExistingEncryptedFiles, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(outputPath))
            {
                throw new DirectoryNotFoundException(outputPath);
            }
            var _this = (IIngestManifest) this;

            //we are forming two arrays of tasks: validation of keys and encryption tasks
            var assetEncryptionTasks = new List<Task>(); 
            var assetCheckKeyTasks = new List<Task>();

            var keys = new ConcurrentDictionary<string, IContentKey>();

            
            foreach (IIngestManifestAsset manifestAsset in _this.IngestManifestAssets)
            {
                IIngestManifestAsset asset = manifestAsset;
                Action checkKeyAction = (() =>
                                             {

                                                 var key = VerifyContentKey(asset);
                                                 //Adding keys which we found for assets.If key has not been found thread throwing exception from VerifyContentKey 
                                                 keys.TryAdd(asset.Id, key);
                                             });
                //Starting validation tasks as soon as we create it
                assetCheckKeyTasks.Add(Task.Factory.StartNew(checkKeyAction, cancellationToken));
                //Creating encryption tasks without starting it
                assetEncryptionTasks.Add(new Task(() => AssetEncryptAction(outputPath, overwriteExistingEncryptedFiles, cancellationToken, keys, asset)));
            }


            return Task.Factory.StartNew(() =>
                                      {
                                          //Wait until all key verification is done
                                          Task.WaitAll(assetCheckKeyTasks.ToArray());

                                          //Starting all encryption tasks
                                          foreach (var encryptTask in assetEncryptionTasks)
                                          {
                                              encryptTask.Start();
                                          }
                                          //Waiting all encryption tasks for completion
                                          Task.WaitAll(assetEncryptionTasks.ToArray());
                                      },cancellationToken);



        }

        private static IContentKey VerifyContentKey(IIngestManifestAsset asset1)
        {
            IAsset asset = asset1.Asset;
            IContentKey key = asset.ContentKeys.Where(c => c.ContentKeyType == ContentKeyType.StorageEncryption).FirstOrDefault();

            if (key == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, StringTable.StorageEncryptionContentKeyIsMissing, asset.Id));
            }
            return key;
        }

        private void AssetEncryptAction(string outputPath, bool overwriteExistingEncryptedFiles, CancellationToken cancellationToken, ConcurrentDictionary<string, IContentKey> keys, IIngestManifestAsset asset)
        {
            cancellationToken.ThrowIfCancellationRequested();
           
            List<Task> encryptTasks = new List<Task>();

            AssetCreationOptions assetCreationOptions = asset.Asset.Options;
            if (assetCreationOptions.HasFlag(AssetCreationOptions.StorageEncrypted))
            {
                IContentKey contentKeyData = keys[asset.Id];
                var fileEncryption = new FileEncryption(contentKeyData.GetClearKeyValue(), EncryptionUtils.GetKeyIdAsGuid(contentKeyData.Id));


                foreach (IngestManifestFileData file in asset.IngestManifestFiles)
                {
                    ulong iv = Convert.ToUInt64(file.InitializationVector, CultureInfo.InvariantCulture);
                    fileEncryption.SetInitializationVectorForFile(file.Name, iv);

                    FileInfo fileInfo = null;
                    fileInfo = TrackedFilesPaths.ContainsKey(file.Id) ? new FileInfo(TrackedFilesPaths[file.Id]) : new FileInfo(file.Name);
                    
                    string destinationPath = Path.Combine(outputPath, fileInfo.Name);
                    if (File.Exists(destinationPath))
                    {
                        if (overwriteExistingEncryptedFiles)
                        {
                            File.Delete(destinationPath);
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, StringTable.BulkIngestFileExists, destinationPath));
                        }
                    }

                    long fileSize = fileInfo.Length;
                    int maxBlockSize = GetBlockSize(fileSize);

                    int numThreads = MaxNumberOfEncryptionThreadsForFilePerCore*Environment.ProcessorCount;
                    ConcurrentQueue<Tuple<int, int>> queue = PrepareUploadQueue(maxBlockSize, fileSize);
                    if (queue.Count < numThreads)
                    {
                        numThreads = queue.Count;
                    }
                    

                    File.Create(destinationPath).Dispose();

                    Action action = GetEncryptionAction(cancellationToken, fileEncryption, file, destinationPath, fileInfo, queue, maxBlockSize);
                    for (int i = 0; i < numThreads; i++)
                    {
                        encryptTasks.Add(Task.Factory.StartNew((action), cancellationToken));
                    }

                }
                try
                {
                    Task.WaitAll(encryptTasks.ToArray());
                }
                finally
                {
                    fileEncryption.Dispose();
                }
            }
        }

        private static Action GetEncryptionAction(
            CancellationToken cancellationToken, 
            FileEncryption fileEncryption, 
            IngestManifestFileData file, 
            string destinationPath, 
            FileInfo fileInfo, 
            ConcurrentQueue<Tuple<int, int>> queue, 
            int maxBlockSize)
        {
            Action action = () =>
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    if (queue.Count > 0)
                                    {
                                        using (var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read))
                                        {
                                            Tuple<int, int> blockIdAndLength;
                                            while (queue.TryDequeue(out blockIdAndLength))
                                            {
                                                cancellationToken.ThrowIfCancellationRequested();
                                                var buffer = new byte[blockIdAndLength.Item2];
                                                var binaryReader = new BinaryReader(fs);

                                                // Move the file system reader to the proper position.
                                                fs.Seek(blockIdAndLength.Item1*(long) maxBlockSize, SeekOrigin.Begin);
                                                int readSize = binaryReader.Read(buffer, 0, blockIdAndLength.Item2);

                                                while (readSize != blockIdAndLength.Item2)
                                                {
                                                    readSize += binaryReader.Read(buffer, readSize, blockIdAndLength.Item2 - readSize);
                                                }

                                                bool lockWasTakenForEncode = false;
                                                try
                                                {
                                                    Monitor.Enter(fileEncryption, ref lockWasTakenForEncode);
                                                    using (FileEncryptionTransform encryptor = fileEncryption.GetTransform(file.Name, blockIdAndLength.Item1*(long) maxBlockSize))
                                                    {
                                                        encryptor.TransformBlock(buffer, 0, readSize, buffer, 0);
                                                    }
                                                }
                                                finally
                                                {
                                                    if (lockWasTakenForEncode) Monitor.Exit(fileEncryption);
                                                }

                                                bool lockWasTakenForWrite = false;
                                                try
                                                {
                                                    Monitor.Enter(file, ref lockWasTakenForWrite);
                                                    using (var writeStream = new FileStream(destinationPath, FileMode.Open, FileAccess.Write))
                                                    {
                                                        writeStream.Seek(blockIdAndLength.Item1*(long) maxBlockSize, SeekOrigin.Begin);
                                                        writeStream.Write(buffer, 0, readSize);
                                                    }
                                                }
                                                finally
                                                {
                                                    if (lockWasTakenForWrite) Monitor.Exit(file);
                                                }
                                            }
                                        }
                                    }
                                };
            return action;
        }

       
        /// <summary>
        /// Gets the size of the block to read from file.
        /// </summary>
        /// <param name="fileSize">Size of the file.</param>
        /// <returns>size of a block</returns>
        private static int GetBlockSize(long fileSize)
        {
            const long kb = 1024;
            //const long mb = 1024 * kb;
            int size = (int) kb*64;
            return (int) (fileSize < size ? fileSize : size);
        }

        /// <summary>
        /// Encrypts all newly added manifest files asyncroniously. All files will be overriden if output folder has files with same names
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns><see cref="Task"/></returns>
        public Task EncryptFilesAsync(string outputPath, CancellationToken token)
        {
            return EncryptFilesAsync(outputPath, true, token);
        }

        /// <summary>
        /// Encrypts manifest files.
        /// </summary>
        /// <param name="outputPath">The output path where all encrypted files will be located.</param>
        /// <param name="overrideExistingEncryptedFiles">if set to <c>true</c> method will override files in ouput folder.</param>
        public void EncryptFiles(string outputPath, bool overrideExistingEncryptedFiles)
        {
            Task task = EncryptFilesAsync(outputPath, overrideExistingEncryptedFiles, CancellationToken.None);
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten();
            }
        }

        /// <summary>
        /// Encrypts all newly added manifest files. All files will be overriden if output folder has files with same names
        /// </summary>
        /// <param name="outputPath">The output path.</param>
        public void EncryptFiles(string outputPath)
        {
            Task task = EncryptFilesAsync(outputPath, CancellationToken.None);
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten();
            }
        }

        private static ConcurrentQueue<Tuple<int, int>> PrepareUploadQueue(int maxBlockSize, long fileSize)
        {
            var queue = new ConcurrentQueue<Tuple<int, int>>();

            int blockId = 0;
            while (fileSize > 0)
            {
                var blockLength = (int) Math.Min(maxBlockSize, fileSize);
                var item = new Tuple<int, int>(blockId++, blockLength);
                queue.Enqueue(item);
                fileSize -= blockLength;
            }

            return queue;
        }

        /// <summary>
        /// Updates manifest.
        /// </summary>
        public void Update()
        {
            try
            {
                UpdateAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        /// <summary>
        /// Updates manifest asynchronously.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task UpdateAsync()
        {
            IngestManifestCollection.VerifyManifest(this);
            IMediaDataServiceContext dataContext = GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(IngestManifestCollection.EntitySet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy();

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }

        /// <summary>
        /// Gets the manifest statistics.
        /// </summary>
        public IIngestManifestStatistics Statistics { get; set; }


        /// <summary>
        /// Gets <see cref="IStorageAccount"/> associated with the <see cref="IIngestManifest"/> 
        /// </summary>
        IStorageAccount IIngestManifest.StorageAccount
        {
            get
            {
                if (GetMediaContext() == null)
                {
                    throw new NullReferenceException("Operation can't be performed. CloudMediaContext hasn't been initiliazed for IngestManifestData type");
                }

                return GetMediaContext().StorageAccounts.Where(c => c.Name == this.StorageAccountName).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the manifest statistics.
        /// </summary>
        IIngestManifestStatistics IIngestManifest.Statistics { get { return Statistics; } }

       

        private static IngestManifestState GetExposedState(int state)
        {
            return (IngestManifestState) state;
        }
    }
}
