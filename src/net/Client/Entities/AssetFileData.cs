//-----------------------------------------------------------------------
// <copyright destinationPath="AssetFileData.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this path except in compliance with the License.
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.WindowsAzure.MediaServices.Client.TransientFaultHandling;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Microsoft.WindowsAzure.MediaServices.Client
{
    /// <summary>
    /// Represents an asset path.
    /// </summary>
    [DataServiceKey("Id")]
    internal partial class AssetFileData : BaseEntity<IAssetFile>,IAssetFile
    {
        /// <summary>
        /// The name of the files set.
        /// </summary>
        internal const string FileSet = "Files";

        private readonly object _lock = new object();

        private IAsset _asset;

        #region IAssetFile Members

        /// <summary>
        /// Occurs when the download progress is updated.
        /// </summary>
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Occurs when the upload progress is updated.
        /// </summary>
        public event EventHandler<UploadProgressChangedEventArgs> UploadProgressChanged;

        /// <summary>
        /// Gets the asset.
        /// </summary>
        IAsset IAssetFile.Asset
        {
            get { return this.Asset; }
        }

        /// <summary>
        /// Gets the asset.
        /// </summary>
        private IAsset Asset
        {
            get
            {
                if ((this._asset == null) && !String.IsNullOrWhiteSpace(this.ParentAssetId))
                {
                    this._asset = this.GetMediaContext().Assets.Where(c => c.Id == this.ParentAssetId).Single();
                }

                return this._asset;
            }
        }

        /// <summary>
        /// Gets the <see cref="Client.AssetFileOptions"/> for this AssetFile
        /// </summary>
        private static AssetFileOptions GetExposedOptions(int options)
        {
            return (AssetFileOptions) options;
        }

        /// <summary>
        /// Asynchronously downloads the represented file to the specified destination path.
        /// </summary>
        /// <param name="destinationPath">The path to download the file to.</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to download files.</param>
        /// <param name="locator">An asset <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A function delegate that returns the future result to be available through the Task.
        /// </returns>
        public Task DownloadAsync(string destinationPath, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken cancellationToken)
        {
            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobDownload);
            }

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetBlobStorageClientRetryPolicy();
            return this.DownloadToFileAsync(destinationPath, blobTransferClient, locator, retryPolicy.AsAzureStorageClientRetryPolicy(), cancellationToken);
        }


        /// <summary>
        /// Downloads the represented file to the specified destination path.
        /// </summary>
        /// <param name="destinationPath">The path to download the file to.</param>
        public void Download(string destinationPath)
        {
            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobDownload);
            }

            IAccessPolicy accessPolicy = null;
            ILocator locator = null;
            try
            {
                accessPolicy = this.GetMediaContext().AccessPolicies.Create("SdkDownload", TimeSpan.FromHours(12), AccessPermissions.Read);
                locator = this.GetMediaContext().Locators.CreateSasLocator(this.Asset, accessPolicy);

                BlobTransferClient blobTransfer = this.GetMediaContext().MediaServicesClassFactory.GetBlobTransferClient();
                blobTransfer.NumberOfConcurrentTransfers = this.GetMediaContext().NumberOfConcurrentTransfers;
                blobTransfer.ParallelTransferThreadCount = this.GetMediaContext().ParallelTransferThreadCount;
                          
                this.DownloadAsync(destinationPath, blobTransfer, locator, CancellationToken.None).Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.Flatten();
            }
            finally
            {
                Cleanup(null,null,locator,accessPolicy);             
            }
        }

        /// <summary>
        /// Asynchronously saves this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UpdateAsync()
        {
            if (this.Asset.State != AssetState.Initialized)
            {
                throw new NotSupportedException(StringTable.NotSupportedFileInfoSave);
            }

            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(FileSet, this);
            dataContext.UpdateObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this))
                .ContinueWith<IAssetFile>(
                    t =>
                        {
                            t.ThrowIfFaulted();
                            AssetFileData data = (AssetFileData)t.Result.AsyncState;
                            return data;
                        });
        }

        /// <summary>
        /// Saves this instance.
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
        /// Asynchronously deletes this instance.
        /// </summary>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task DeleteAsync()
        {
            IMediaDataServiceContext dataContext = this.GetMediaContext().MediaServicesClassFactory.CreateDataServiceContext();
            dataContext.AttachTo(FileSet, this);
            dataContext.DeleteObject(this);

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetSaveChangesRetryPolicy(dataContext as IRetryPolicyAdapter);

            return retryPolicy.ExecuteAsync<IMediaDataServiceResponse>(() => dataContext.SaveChangesAsync(this));
        }


        /// <summary>
        /// Uploads the destinationPath with given path asynchronously
        /// </summary>
        /// <param name="path">The path of a destinationPath to upload</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to upload files.</param>
        /// <param name="locator">An asset <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to use for canceling upload operation.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UploadAsync(string path, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken token)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobUpload);
            }

            ValidateFileName(path);

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return UploadAsync(fs, blobTransferClient, locator, token)
                .ContinueWith(t => fs.Dispose()); // we can't put a using block as the file stream would get disposed before completing the UploadAsync task
        }


        /// <summary>
        /// Uploads the stream asynchronously
        /// </summary>
        /// <param name="stream">The stream to upload</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to upload files.</param>
        /// <param name="locator">An locator <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to use for canceling upload operation.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UploadAsync(Stream stream, BlobTransferClient blobTransferClient, ILocator locator, CancellationToken token)
        {
            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobUpload);
            }

            if (blobTransferClient == null)
            {
                throw new ArgumentNullException("blobTransferClient");
            }

            if (locator == null)
            {
                throw new ArgumentNullException("locator");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            IContentKey contentKeyData = null;
            FileEncryption fileEncryption = null;
            AssetCreationOptions assetCreationOptions = this.Asset.Options;
            if (assetCreationOptions.HasFlag(AssetCreationOptions.StorageEncrypted))
            {
                contentKeyData = this.Asset.ContentKeys.FirstOrDefault(c => c.ContentKeyType == ContentKeyType.StorageEncryption);
                if (contentKeyData == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, StringTable.StorageEncryptionContentKeyIsMissing, this.Asset.Id));
                }

                fileEncryption = new FileEncryption(contentKeyData.GetClearKeyValue(), EncryptionUtils.GetKeyIdAsGuid(contentKeyData.Id));
                ulong iv = Convert.ToUInt64(this.InitializationVector, CultureInfo.InvariantCulture);
                fileEncryption.SetInitializationVectorForFile(this.Name, iv);
            }

            EventHandler<BlobTransferProgressChangedEventArgs> handler = (s, e) => OnUploadProgressChanged(this.Name, e);

            blobTransferClient.TransferProgressChanged += handler;

            MediaRetryPolicy retryPolicy = this.GetMediaContext().MediaServicesClassFactory.GetBlobStorageClientRetryPolicy();

            var streamLength = stream.Length;

            return blobTransferClient.UploadBlob(
                    new Uri(locator.BaseUri),
                    this.Name,
                    stream,
                    null,
                    fileEncryption,
                    token,
                    retryPolicy.AsAzureStorageClientRetryPolicy(),
                    () => locator.ContentAccessComponent)
                .ContinueWith(
                    ts =>
                    {
                        blobTransferClient.TransferProgressChanged -= handler;
                        this.PostUploadAction(ts, streamLength, token);
                    });
        }


        private void OnUploadProgressChanged(string name, BlobTransferProgressChangedEventArgs blobTransferProgressChangedEventArgs)
        {
            if (blobTransferProgressChangedEventArgs.SourceName == name)
            {
                var uploadChangeHandler = UploadProgressChanged;
                if (uploadChangeHandler != null)
                {
                    uploadChangeHandler(
                        sender: this,
                        e: new UploadProgressChangedEventArgs(
                            blobTransferProgressChangedEventArgs.BytesTransferred,
                            blobTransferProgressChangedEventArgs.TotalBytesToTransfer));
                }
            }
        }

        private void ValidateFileName(string path)
        {
            string filename = Path.GetFileName(path).ToUpperInvariant();
            if (filename != this.Name.ToUpperInvariant())
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, StringTable.FileNameMismatch, this.Name, filename));
            }
        }

        private void PostUploadAction(Task task, long streamSize, CancellationToken token)
        {
            task.ThrowIfFaulted();
            token.ThrowIfCancellationRequested();

            // Set the ContentFileSize base on the local file size
            this.ContentFileSize = streamSize;

            this.Update();
        }
        #endregion

        #region IMediaContextContainer Members



        #endregion

        /// <summary>
        /// Downloads the file asynchronously .
        /// </summary>
        /// <param name="destinationPath">The path to download the file to.</param>
        /// <param name="blobTransferClient">The <see cref="BlobTransferClient"/> which is used to download files.</param>
        /// <param name="locator">An asset <see cref="ILocator"/> which defines permissions associated with the Asset.</param>
        /// <param name="retryPolicy">The retry policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        internal Task DownloadToFileAsync(string destinationPath,BlobTransferClient blobTransferClient, ILocator locator, IRetryPolicy retryPolicy, CancellationToken cancellationToken)
        {
            FileEncryption fileEncryption = this.GetFileEncryption();
            cancellationToken.Register(() => this.Cleanup(null, fileEncryption, null, null));
            return Task.Factory.StartNew(() =>
            {
                cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, fileEncryption, null, null));
                ulong iv = Convert.ToUInt64(this.InitializationVector, CultureInfo.InvariantCulture);
                UriBuilder uriBuilder = new UriBuilder(locator.BaseUri);
                uriBuilder.Path += String.Concat("/", Name);
                blobTransferClient.TransferProgressChanged += this.OnDownloadBlobTransferProgressChanged;

                blobTransferClient.DownloadBlob(
                        uriBuilder.Uri, 
                        destinationPath, 
                        fileEncryption, 
                        iv, 
                        cancellationToken, 
                        retryPolicy, 
                        () => locator.ContentAccessComponent)
                    .Wait(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, fileEncryption, null, null));
            },
                    cancellationToken)
                .ContinueWith(
                    t =>
                    {
                        t.ThrowIfFaulted(() => this.Cleanup(null, fileEncryption, null, null));
                        cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, fileEncryption, null, null));
                        this.Cleanup(null, fileEncryption, null, null);
                    },
                    cancellationToken);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exception needs to be caught while cleaning up.")]
        private void Cleanup(BlobTransferClient blobTransfer, FileEncryption fileEncryption, ILocator locator, IAccessPolicy accessPolicy)
        {
            lock (this._lock)
            {
                if (blobTransfer != null)
                {
                    try
                    {
                        blobTransfer.TransferProgressChanged -= this.OnDownloadBlobTransferProgressChanged;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        blobTransfer = null;
                    }
                }

                if (fileEncryption != null)
                {
                    try
                    {
                        fileEncryption.Dispose();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        fileEncryption = null;
                    }
                }

                if (locator != null)
                {
                    try
                    {
                        locator.Delete();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        locator = null;
                    }
                }

                if (accessPolicy != null)
                {
                    try
                    {
                        accessPolicy.Delete();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        accessPolicy = null;
                    }
                }
            }
        }

        private FileEncryption GetFileEncryption()
        {
            if (!this.IsEncrypted)
            {
                return null;
            }

            // We want to support downloading PlayReady encrypted content too.
            if (this.EncryptionScheme != FileEncryption.SchemeName)
            {
                return null;
            }

            IContentKey key = this.Asset.ContentKeys.Where(c => c.ContentKeyType == ContentKeyType.StorageEncryption).FirstOrDefault();
            Guid keyId = EncryptionUtils.GetKeyIdAsGuid(key.Id);

            return new FileEncryption(key.GetClearKeyValue(), keyId);
        }

        private void OnDownloadBlobTransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            EventHandler<DownloadProgressChangedEventArgs> downloadProgressEvent = this.DownloadProgressChanged;
            if (downloadProgressEvent != null)
            {
                downloadProgressEvent(this, new DownloadProgressChangedEventArgs(e.BytesTransferred, e.TotalBytesToTransfer));
            }
        }

        /// <summary>
        /// Gets the MIME type of the specified destinationPath.
        /// </summary>
        /// <param name="fileName">Name of the destinationPath.</param>
        /// <returns>The MIME type.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exception needs to be caught and return null in case of error.")]
        internal static string GetMimeType(string fileName)
        {
            string mimeType = null;

            try
            {
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(fileName));

                if (registryKey != null)
                {
                    object registryValue = registryKey.GetValue("Content Type");
                    mimeType = registryValue != null ? registryValue.ToString() : null;
                }
            }
            catch
            {
                // Catch all exceptions.
                // If this operation fails, the default MIME type will not be set.
            }

            return mimeType;
        }


        /// <summary>
        /// Uploads the file with given path 
        /// </summary>
        /// <param name="path">The path of a file to upload</param>
        public void Upload(string path)
        {
            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobUpload);
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            ValidateFileName(path);

            // we are using the file name as the name to pass forward 
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                UploadAsync(fs, CancellationToken.None).Wait();
            }
        }

        /// <summary>
        /// Uploads the stream with the given name 
        /// </summary>
        /// <param name="name">Name of the stream</param>
        /// <param name="stream">Stream to upload</param>
        public void Upload(Stream stream)
        {
            UploadAsync(stream, CancellationToken.None).Wait();
        }

        /// <summary>
        /// Uploads a stream asynchronously 
        /// </summary>
        /// <param name="name">Name for the stream</param>
        /// <param name="stream">Stream to be uploaded</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for canceling upload operation.</param>
        /// <returns>A function delegate that returns the future result to be available through the Task.</returns>
        public Task UploadAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (this.IsFragmented())
            {
                throw new NotSupportedException(StringTable.NotSupportedFragblobUpload);
            }

            IAccessPolicy accessPolicy = null;
            ILocator locator = null;

            var policyName = "SdkUpload" + Guid.NewGuid().ToString();
            return GetMediaContext().AccessPolicies
                .CreateAsync(policyName, TimeSpan.FromHours(12), AccessPermissions.Write)
                .ContinueWith<ILocator>(
                    t =>
                    {
                        accessPolicy = t.Result;

                        t.ThrowIfFaulted(() => this.Cleanup(null, null, locator, accessPolicy));
                        cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, null, locator, accessPolicy));

                        locator = this.GetMediaContext().Locators.CreateSasLocator(this.Asset, accessPolicy);
                        cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, null, locator, accessPolicy));

                        return locator;
                    },
                    cancellationToken).
                ContinueWith(
                    t =>
                    {
                        locator = t.Result;
                        t.ThrowIfFaulted(() => this.Cleanup(null, null, locator, accessPolicy));
                        cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, null, locator, accessPolicy));

                        var blobTransfer = GetMediaContext().MediaServicesClassFactory.GetBlobTransferClient();

                        blobTransfer.NumberOfConcurrentTransfers = this.GetMediaContext().NumberOfConcurrentTransfers;
                        blobTransfer.ParallelTransferThreadCount = this.GetMediaContext().ParallelTransferThreadCount;

                        UploadAsync(stream, blobTransfer, locator, cancellationToken).Wait();
                        locator.Delete();
                        cancellationToken.ThrowIfCancellationRequested(() => this.Cleanup(null, null, null, accessPolicy));
                        accessPolicy.Delete();
                    },
                    cancellationToken);
        }

        private bool IsFragmented()
        {
            return GetExposedOptions(Options).HasFlag(Client.AssetFileOptions.Fragmented);
        }
    }
}
