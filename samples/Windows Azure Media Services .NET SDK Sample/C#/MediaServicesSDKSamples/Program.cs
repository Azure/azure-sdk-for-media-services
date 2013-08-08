using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Reflection;

namespace ConsoleApplication1
{
    // This code sample demonstrates how to use the Windows Azure Media Services   
    // SDK to accomplish common developer tasks.  To set up this project including 
    // prerequisites and required assembly references, see the accompanying MSDN
    // documentation series "Building Applications with the Media Services SDK" 
    // at http://go.microsoft.com/fwlink/?linkid=247821. Before you can run the 
    // samples in this project, you will need to set it up as described in the 
    // topic in this series titled "Setup for Development on the Media Services 
    // SDK for .NET." 

    public enum MediaContentType
    {
        SmoothStreaming,
        HLS
    }

    class Program
    {
        // Class-level field used to keep a reference to the service context.
        private static CloudMediaContext _context = null;

        // ********************************
        // Paths for media files to pass to methods.  You must provide local 
        // paths to these files/folders that will be used in your code. A 
        // set of sample media and task preset config files is included with 
        // this project download in the supportFiles folder. If you update the 
        // _supportFiles base path below to point to it, you will not need  
        // to update all the following paths to the individual files.  

        // Base support files path.  Update this field to point to the base path  
        // for the local support files folder that you create. 

        private static readonly string _supportFiles = 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\supportFiles";

        // Paths to support files (within the above base path). You can use 
        // the provided sample media files from the "supportFiles" folder, or 
        // provide paths to your own media files below to run these samples.
        private static readonly string _singleInputFilePath =
            Path.GetFullPath(_supportFiles + @"\multifile\interview2.wmv");
        private static readonly string _singleInputMp4Path =
            Path.GetFullPath(_supportFiles + @"\multifile\BigBuckBunny.mp4");
        private static readonly string _primaryFilePath =
            Path.GetFullPath(_supportFiles + @"\multifile\interview1.wmv");
        private static readonly string _inputFilesFolder =
            Path.GetFullPath(_supportFiles + @"\multifile");
        private static readonly string _streamingInputFilesFolder =
            Path.GetFullPath(_supportFiles + @"\streamingfiles");
        private static readonly string _configFilePath =
            Path.GetFullPath(_supportFiles + @"\configuration");
        private static readonly string _outputFilesFolder =
            Path.GetFullPath(_supportFiles + @"\outputfiles");
        private static readonly string _multibitrateFilesFolder =
            Path.GetFullPath(_supportFiles + @"\multibitratefiles");
        private static readonly string _primaryMultibitrateFile =
            Path.GetFullPath(_supportFiles + @"\multibitratefiles\interview1_384p_H264_350Kbps_AAC.mp4");


        // ********************************
        // Authentication and connection settings.  These settings are pulled from 
        // the App.Config file and are required to connect to Media Services, 
        // authenticate, and get a token so that you can access the server context.
        // These values are also used in the LogJobStop and LogJobDetails methods. 

        private static readonly string _accountName = 
            ConfigurationManager.AppSettings["MediaServicesAccountName"];
        private static readonly string _accountKey = 
            ConfigurationManager.AppSettings["MediaServicesAccountKey"];

        // Media Services storage account credentials.
        private static readonly string _storageAccountName =
            ConfigurationManager.AppSettings["MediaServicesStorageAccountName"];
        private static readonly string _storageAccountKey =
            ConfigurationManager.AppSettings["MediaServicesStorageAccountKey"];

        // External storage account credentials.  This for the future code that you can 
        // use to copy a blob from externa storage acct to your Media Services storage acct. 
        private static readonly string _externalStorageAccountName =
            ConfigurationManager.AppSettings["ExternalStorageAccountName"];
        private static readonly string _externalStorageAccountKey =
            ConfigurationManager.AppSettings["ExternalStorageAccountKey"];

        // Task configuration protection settings. Here are the available task configuration settings 
        // to use when you create tasks.  
        //    TaskCreationOptions.None:  means data in your config file is sent in the clear, unencrypted.
        //    TaskCreationOptions.ProtectedConfiguration:  If you have sensitive config data, use the 
        //        ProtectedConfiguration setting when you create tasks.
        const Microsoft.WindowsAzure.MediaServices.Client.TaskOptions _protectedConfig =
            Microsoft.WindowsAzure.MediaServices.Client.TaskOptions.ProtectedConfiguration;
        const Microsoft.WindowsAzure.MediaServices.Client.TaskOptions _clearConfig =
            Microsoft.WindowsAzure.MediaServices.Client.TaskOptions.None;

        // Lock used to make sure console writing doesn't overlap between BulkIngest monitoring thread
        private static System.Object consoleWriteLock = new Object();

        static void Main()
        {
            // Get server context.  This line should be left uncommented as it
            // creates the static context object used by all other methods. 
            _context = GetContext();
            CreateBulkIngestManifest("manifestname");
            // To run any of the sample code, call it from here.
        }

        #region  Samples from Ingest Assets with the Media Services SDK for .NET
        // Create an empty asset
        static private IAsset CreateEmptyAsset(string assetName, AssetCreationOptions assetCreationOptions)
        {
            var asset = _context.Assets.Create(assetName, assetCreationOptions);

            Console.WriteLine("Asset name: " + asset.Name);
            Console.WriteLine("Time created: " + asset.Created.Date.ToString());

            return asset;
        }

        // Create asset and upload a single file int the Asset
        static public IAsset CreateAssetAndUploadSingleFile(AssetCreationOptions assetCreationOptions, string singleFilePath)
        {
            // For the AssetCreationOptions you can specify encryption options.
            //   None:  no encryption. By default, storage encryption is used. If you want to 
            //          create an unencrypted asset, you must set this option.
            //   StorageEncrypted:  storage encryption. Encrypts a clear input file 
            //          before it is uploaded to Azure storage. This is the default if not specified
            //   CommonEncryptionProtected:  for Common Encryption Protected (CENC) files. An 
            //         example is a set of files that are already PlayReady encrypted. 

            var assetName = "UploadSingleFile_" + DateTime.UtcNow.ToString();
            var asset = CreateEmptyAsset(assetName, assetCreationOptions);

            var fileName = Path.GetFileName(singleFilePath);

            var assetFile = asset.AssetFiles.Create(fileName);

            Console.WriteLine("Created assetFile {0}", assetFile.Name);
            Console.WriteLine("Upload {0}", assetFile.Name);

            assetFile.Upload(singleFilePath);
            Console.WriteLine("Done uploading of {0}", assetFile.Name);

            return asset;
        }

        // Create asset and upload multiple files into the Asset
        static public IAsset CreateAssetAndUploadMultipleFiles(AssetCreationOptions assetCreationOptions, string folderPath)
        {
            var assetName = "UploadMultipleFiles_" + DateTime.UtcNow.ToString();

            var asset = CreateEmptyAsset(assetName, assetCreationOptions);

            var accessPolicy = _context.AccessPolicies.Create(assetName, TimeSpan.FromDays(30),
                                                                AccessPermissions.Write | AccessPermissions.List);
            var locator = _context.Locators.CreateLocator(LocatorType.Sas, asset, accessPolicy);

            var blobTransferClient = new BlobTransferClient();
            blobTransferClient.NumberOfConcurrentTransfers = 20;
            blobTransferClient.ParallelTransferThreadCount = 20;

            blobTransferClient.TransferProgressChanged += blobTransferClient_TransferProgressChanged;

            var filePaths = Directory.EnumerateFiles(folderPath);

            Console.WriteLine("There are {0} files in {1}", filePaths.Count(), folderPath);

            if (!filePaths.Any())
            {
                throw new FileNotFoundException(String.Format("No files in directory, check folderPath: {0}", folderPath));
            }

            var uploadTasks = new List<Task>();
            foreach (var filePath in filePaths)
            {
                var assetFile = asset.AssetFiles.Create(Path.GetFileName(filePath));
                // It is recommended to validate AccestFiles before upload. 
                Console.WriteLine("Start uploading of {0}", assetFile.Name);
                uploadTasks.Add(assetFile.UploadAsync(filePath, blobTransferClient, locator, CancellationToken.None));
            }

            Task.WaitAll(uploadTasks.ToArray());
            Console.WriteLine("Done uploading the files");

            blobTransferClient.TransferProgressChanged -= blobTransferClient_TransferProgressChanged;

            locator.Delete();
            accessPolicy.Delete();

            return asset;
        }
        #endregion

        #region Samples from Ingesting Assets in Bulk
        // Upload a file into Blob Storage
        static void UploadBlobFile(string destBlobURI, string filename)
        {
            Task copytask = new Task(() =>
            {
                var storageaccount = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true);
                CloudBlobClient blobClient = storageaccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(destBlobURI);

                string[] splitfilename = filename.Split('\\');
                var blob = blobContainer.GetBlockBlobReference(splitfilename[splitfilename.Length - 1]);

                using (var stream = System.IO.File.OpenRead(filename))
                    blob.UploadFromStream(stream);

                lock (consoleWriteLock)
                {
                    Console.WriteLine("Upload for {0} completed.", filename);
                }
            });

            copytask.Start();
        }

        // Monitor a bulk ingest 
        static void MonitorBulkManifest(string manifestID)
        {
            bool bContinue = true;
            while (bContinue)
            {
                //=== We need a new context here because IIngestManifestStatistics is considered an expensive ===//
                //=== property and not updated realtime for a context.                                        ===//
                CloudMediaContext context = GetContext();

                IIngestManifest manifest = context.IngestManifests.Where(m => m.Id == manifestID).FirstOrDefault();

                if (manifest != null)
                {
                    lock (consoleWriteLock)
                    {
                        Console.WriteLine("\nWaiting on all file uploads.");
                        Console.WriteLine("PendingFilesCount  : {0}", manifest.Statistics.PendingFilesCount);
                        Console.WriteLine("FinishedFilesCount : {0}", manifest.Statistics.FinishedFilesCount);
                        Console.WriteLine("{0}% complete.\n", (float)manifest.Statistics.FinishedFilesCount / (
                            float)(manifest.Statistics.FinishedFilesCount + manifest.Statistics.PendingFilesCount) * 100);


                        if (manifest.Statistics.PendingFilesCount == 0)
                        {
                            Console.WriteLine("Completed\n");
                            bContinue = false;
                        }
                    }

                    if (manifest.Statistics.FinishedFilesCount < manifest.Statistics.PendingFilesCount)
                    {
                        Thread.Sleep(60000);
                    }
                }
                else //=== Manifest is null ===//
                    bContinue = false;
            }
        }

        // Create bulk ingest manifest
        static IIngestManifest CreateBulkIngestManifest(string name)
        {
            Console.WriteLine("\n===============================================");
            Console.WriteLine("========[ CREATE BULK INGEST MANIFEST ]========");
            Console.WriteLine("===============================================\n");


            IIngestManifest manifest = _context.IngestManifests.Create(name);

            IAsset destAsset1 = _context.Assets.Create(name + "_asset_1", AssetCreationOptions.None);
            IAsset destAsset2 = _context.Assets.Create(name + "_asset_2", AssetCreationOptions.None);

            string filename1 = _singleInputMp4Path;
            string filename2 = _primaryFilePath;
            string filename3 = _singleInputFilePath;

            //=== Preently, each asset filename uploaded must be unique for an individual Bulk ingest manifest. So two assets can not have ===//
            //=== the same asset filename or an exception will be thrown for duplicate filename.                                           ===//
            IIngestManifestAsset bulkAsset1 = manifest.IngestManifestAssets.Create(destAsset1, new[] { filename1 });
            IIngestManifestAsset bulkAsset2 = manifest.IngestManifestAssets.Create(destAsset2, new[] { filename2, filename3 });

            ListIngestManifests(manifest.Id);

            Console.WriteLine("\n===============================================");
            Console.WriteLine("===[ BULK INGEST MANIFEST MONITOR FILE COPY]===");
            Console.WriteLine("===============================================\n");

            UploadBlobFile(manifest.BlobStorageUriForUpload, filename1);
            UploadBlobFile(manifest.BlobStorageUriForUpload, filename2);
            UploadBlobFile(manifest.BlobStorageUriForUpload, filename3);

            MonitorBulkManifest(manifest.Id);
            ListIngestManifests(manifest.Id);

            return manifest;
        }

        // Delete bulk ingest manifest
        static void DeleteBulkManifest(string name)
        {
            Console.WriteLine("\n===============================================");
            Console.WriteLine("=======[ DELETE BULK INGEST MANIFESTS ]========");
            Console.WriteLine("===============================================\n");

            foreach (IIngestManifest manifest in _context.IngestManifests.Where(c => c.Name == name))
            {
                DeleteBulkManifestAssets(manifest.Id);

                Console.WriteLine("Deleting Manifest...\n\tName : {0}\n\tManifest ID : {1}...", manifest.Name, manifest.Id);
                manifest.Delete();
                Console.WriteLine("Delete Complete.\n");
            }
        }

        // Delete assets for a specified bulk ingest manifest
        static void DeleteBulkManifestAssets(string manifestID)
        {
            Console.WriteLine("\n===============================================");
            Console.WriteLine("=====[ DELETE BULK INGEST MANIFEST ASSETS ]====");
            Console.WriteLine("===============================================\n");

            foreach (IIngestManifest manifest in _context.IngestManifests.Where(c => c.Id == manifestID))
            {
                Console.WriteLine("Deleting assets for manifest named : {0}...\n", manifest.Name);
                foreach (IIngestManifestAsset manifestAsset in manifest.IngestManifestAssets)
                {
                    foreach (ILocator locator in manifestAsset.Asset.Locators)
                    {
                        Console.WriteLine("Deleting locator {0} for asset {1}", locator.Path, manifestAsset.Asset.Id);
                        locator.Delete();
                    }
                    Console.WriteLine("Deleting asset {0}\n", manifestAsset.Asset.Id);
                    manifestAsset.Asset.Delete();
                }
            }
        }

        // List ingest manifests
        static IQueryable<IIngestManifest> ListIngestManifests(string manifestId = "")
        {
            CloudMediaContext context = GetContext();

            Console.WriteLine("\n===============================================");
            Console.WriteLine("===========[ BULK INGEST MANIFESTS ]===========");
            Console.WriteLine("===============================================\n");

            IQueryable<IIngestManifest> manifests = null;

            //=== If an Id is supplied, list the manifest with that Id. Otherwise, list all manifests ===//
            if (manifestId == "")
                manifests = context.IngestManifests;
            else
                manifests = context.IngestManifests.Where(m => m.Id == manifestId);

            foreach (IIngestManifest manifest in manifests)
            {
                Console.WriteLine("Manifest Name  : {0}", manifest.Name);
                Console.WriteLine("Manifest State : {0}", manifest.State.ToString());
                Console.WriteLine("Manifest Id    : {0}", manifest.Id);
                Console.WriteLine("Manifest Last Modified      : {0}", manifest.LastModified.ToLocalTime().ToString());
                Console.WriteLine("Manifest PendingFilesCount  : {0}", manifest.Statistics.PendingFilesCount);
                Console.WriteLine("Manifest FinishedFilesCount : {0}", manifest.Statistics.FinishedFilesCount);
                Console.WriteLine("Manifest BLOB URI : {0}\n", manifest.BlobStorageUriForUpload);

                foreach (IIngestManifestAsset manifestasset in manifest.IngestManifestAssets)
                {
                    Console.WriteLine("\tAsset Name    : {0}", manifestasset.Asset.Name);
                    Console.WriteLine("\tAsset ID      : {0}", manifestasset.Asset.Id);
                    Console.WriteLine("\tAsset Options : {0}", manifestasset.Asset.Options.ToString());
                    Console.WriteLine("\tAsset State   : {0}", manifestasset.Asset.State.ToString());
                    Console.WriteLine("\tAsset Files....");

                    foreach (IIngestManifestFile assetfile in manifestasset.IngestManifestFiles)
                    {
                        Console.WriteLine("\t\t{0}\n\t\tFile State : {1}\n", assetfile.Name, assetfile.State.ToString());
                    }
                    Console.WriteLine("");
                }
            }

            return manifests;
        }

        private static void blobTransferClient_TransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            Console.WriteLine("{0}% upload competed for {1}.", e.ProgressPercentage, e.LocalFile);
        }

        static public void CopyBlobToAssetContainerInTheSameMediaServicesAccount()
        {
            CloudMediaContext context = new CloudMediaContext(_accountName, _accountKey);

            var storageAccount = new CloudStorageAccount(new StorageCredentials(_storageAccountName, _storageAccountKey), true);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var mediaBlobContainer = cloudBlobClient.GetContainerReference(cloudBlobClient.BaseUri + "mediafiles001");

            mediaBlobContainer.CreateIfNotExists();

            string StreamingFilesFolder = Path.GetFullPath(@"c:../..\SupportFiles\streamingfiles\");

            // Upload some files to the blob container (for testing purposes). 
            DirectoryInfo uploadDirectory = new DirectoryInfo(StreamingFilesFolder);
            foreach (var file in uploadDirectory.EnumerateFiles())
            {
                CloudBlockBlob blob = mediaBlobContainer.GetBlockBlobReference(file.Name);
                var name = file.Name;
                using (var stream = System.IO.File.OpenRead(StreamingFilesFolder + name))
                    blob.UploadFromStream(stream);
            }


            // Create a new asset.
            IAsset asset = context.Assets.Create("NewAsset_" + Guid.NewGuid(), AssetCreationOptions.None);
            IAccessPolicy writePolicy = context.AccessPolicies.Create("writePolicy",
                TimeSpan.FromMinutes(120), AccessPermissions.Write);
            ILocator destinationLocator = context.Locators.CreateLocator(LocatorType.Sas, asset, writePolicy);

            // Get the asset container URI and copy blobs from mediaContainer to assetContainer.
            Uri uploadUri = new Uri(destinationLocator.Path);
            string assetContainerName = uploadUri.Segments[1];
            CloudBlobContainer assetContainer =
                cloudBlobClient.GetContainerReference(assetContainerName);

            foreach (var sourceBlob in mediaBlobContainer.ListBlobs())
            {
                string fileName = HttpUtility.UrlDecode(Path.GetFileName(sourceBlob.Uri.AbsoluteUri));

                var sourceCloudBlob = mediaBlobContainer.GetBlockBlobReference(fileName);
                sourceCloudBlob.FetchAttributes();

                if (sourceCloudBlob.Properties.Length > 0)
                {
                    IAssetFile assetFile = asset.AssetFiles.Create(fileName);
                    var destinationBlob = assetContainer.GetBlockBlobReference(fileName);

                    destinationBlob.DeleteIfExists();
                    destinationBlob.StartCopyFromBlob(sourceCloudBlob);

                    destinationBlob.FetchAttributes();
                    if (sourceCloudBlob.Properties.Length != destinationBlob.Properties.Length)
                        Console.WriteLine("Failed to copy");
                }
            }

            destinationLocator.Delete();
            writePolicy.Delete();

            // Refresh the asset.
            asset = context.Assets.Where(a => a.Id == asset.Id).FirstOrDefault();

            //At this point, you can create a job using your asset.
            Console.WriteLine("You are ready to use " + asset.Name);

            // Since we copied a set of Smooth Streaming files,
            // set the .ism file to be the primary file
            var ismAssetFiles = asset.AssetFiles.ToList().
                        Where(f => f.Name.EndsWith(".ism", StringComparison.OrdinalIgnoreCase))
                        .ToArray();

            if (ismAssetFiles.Count() != 1)
                throw new ArgumentException("The asset should have only one, .ism file");

            ismAssetFiles.First().IsPrimary = true;
            ismAssetFiles.First().Update();

        }

        #endregion

        #region Samples from Process Assets with the Media Services SDK for .NET
        private static IMediaProcessor GetLatestMediaProcessorByName(string mediaProcessorName)
        {
            // The possible strings that can be passed into the 
            // method for the mediaProcessor parameter:
            //   Windows Azure Media Encoder
            //   Windows Azure Media Packager
            //   Windows Azure Media Encryptor
            //   Storage Decryption

            var processor = _context.MediaProcessors.Where(p => p.Name == mediaProcessorName).
                ToList().OrderBy(p => new Version(p.Version)).LastOrDefault();

            if (processor == null)
                throw new ArgumentException(string.Format("Unknown media processor", mediaProcessorName));

            return processor;
        }

        // Shows how to encode an input media file using a preset string
        static IJob CreateEncodingJob(string inputMediaFilePath, string outputFolder)
        {
            //Create an encrypted asset and upload to storage. 
            IAsset asset = CreateAssetAndUploadSingleFile(AssetCreationOptions.StorageEncrypted, inputMediaFilePath);

            // Declare a new job.
            IJob job = _context.Jobs.Create("My encoding job");
            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task.
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Encoder");

            // Create a task with the encoding details, using a string preset.
            ITask task = job.Tasks.AddNew("My encoding task",
                processor,
                "H264 Broadband 720p",
                _protectedConfig);

            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset);

            // Add an output asset to contain the results of the job. 
            // This output is specified as AssetCreationOptions.None, which 
            // means the output asset is in the clear (unencrypted). 
            IAsset outputAsset = task.OutputAssets.AddNew("Output asset", AssetCreationOptions.None);

            // Use the following event handler to check job progress.  
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. This displays basic job details
            // to the console and saves them to a JobDetails-{JobId}.txt file 
            // in your output folder.
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            Console.WriteLine("outputAsset: " + outputAsset.Name);
            Console.WriteLine("     " + outputAsset.Id);
            Console.WriteLine("     " + outputAsset.Options.ToString());
            Console.WriteLine("     " + outputAsset.State.ToString());

            // **********
            // Optional code.  Code after this point is not required for 
            // an encoding job, but shows how to access the assets that 
            // are the output of a job, either by creating URLs to the 
            // asset on the server, or by downloading. 
            // **********

            // Get an updated job reference.
            job = GetJob(job.Id);

            // Check for errors
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
                return job;
            }

            // Get a reference to the output asset from the job.
            //IAsset outputAsset = job.OutputMediaAssets[0];

            // Build a list of SAS URLs to each file in the asset. 
            // BuildAndSaveAssetSasUrlList(outputAsset);

            return job;
        }

        // Shows how to encode an input media file using a preset string, encrypt 
        // the output file with storage encryption, and then create a decrypted 
        // output file. Uses a sequence of chained tasks. 
        static IJob CreateChainedTaskEncodingJob(string inputMediaFilePath, string outputFolder)
        {
            //Create an encrypted asset and upload to storage. 
            IAsset asset = CreateAssetAndUploadSingleFile(AssetCreationOptions.StorageEncrypted, inputMediaFilePath);

            // Declare a new job.
            IJob job = _context.Jobs.Create("My task-chained encoding job");

            // Set up the first task to encode the input file.

            // Get a media processor reference
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Encoder");

            // Create a task with the encoding details, using a string preset.
            ITask task = job.Tasks.AddNew("My encoding task",
                processor,
                "H264 Broadband 720p",
                _protectedConfig);

            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset);

            // Specify the storage-encrypted output asset.
            task.OutputAssets.AddNew("My storage-encrypted output asset",
                AssetCreationOptions.StorageEncrypted);

            // Set up the second task to decrypt the encoded output file from 
            // the first task.

            // Declare another media proc for a storage decryption task.
            IMediaProcessor decryptProcessor = GetLatestMediaProcessorByName("Storage Decryption");

            // Declare the decryption task. 
            ITask decryptTask = job.Tasks.AddNew("My decryption task",
                decryptProcessor,
                string.Empty,
                _clearConfig);

            // Specify the input asset to be decrypted. This is the output 
            // asset from the first task. 
            decryptTask.InputAssets.Add(task.OutputAssets[0]);

            // Specify an output asset to contain the results of the job. 
            // This should have AssetCreationOptions.None. 
            decryptTask.OutputAssets.AddNew("My decrypted output asset",
                AssetCreationOptions.None);

            // Use the following event handler to check job progress. 
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. This displays basic job details
            // to the console and saves them to a JobDetails-{JobId}.txt file 
            // in your output folder.
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();


            // **********
            // Optional code.  Code after this point is not required for 
            // an encoding job, but shows how to access the assets that 
            // are the output of a job, either by creating URLs to the 
            // asset on the server, or by downloading. 
            // **********

            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // If job state is Error, the event handling 
            // method for job progress should log errors.  Here we check 
            // for error state and exit if needed.
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
                return job;
            }

            // Query for the decrypted output asset, which is the one 
            // the one that you set to not use storage encryption.
            var decryptedAsset =
                                 from a in job.OutputMediaAssets
                                 where a.Options == AssetCreationOptions.None
                                 select a;
            // Cast the reference as an IAsset.
            IAsset decryptedOutputAsset = decryptedAsset.First();

            // Optionally download the decrypted output.
            DownloadAssetToLocal(job.Id, outputFolder);

            return job;
        }

        // Upload a video file, and encode to Smooth Streaming format
        private static IJob EncodeToSmoothStreaming(string inputMediaFilePath)
        {
            //Create an encrypted asset and upload the mp4. 
            IAsset asset = CreateAssetAndUploadSingleFile(AssetCreationOptions.StorageEncrypted, inputMediaFilePath);

            // Declare a new job.
            IJob job = _context.Jobs.Create("My MP4 to Smooth Streaming encoding job");

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task.
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Encoder");


            // Create a task with the conversion details, using a configuration file. 
            ITask task = job.Tasks.AddNew("My Mp4 to Smooth Task",
                processor,
                "H264 Smooth Streaming 720p",
                _clearConfig);

            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset);

            // Add an output asset to contain the results of the job. We do not need 
            // to persist the output asset to storage, so set the shouldPersistOutputOnCompletion
            // param to false. 
            task.OutputAssets.AddNew("Output asset", AssetCreationOptions.None);

            // Use the following event handler to check job progress. 
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the async job.

            job.Submit();

            // Optionally log job details. 
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // Check for errors
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
            }

            return job;
        }

        // Upload a video, encode to Adaptive Bitrate MP4 and convert to Smooth Streaming format
        private static IJob EncodeToAdaptiveBitrateAndConvertToSmooth(string inputMediaFilePath)
        {
            // Create asset and upload file
            IAsset asset = CreateAssetAndUploadSingleFile(AssetCreationOptions.None, inputMediaFilePath);

            // Get instance of Windows Media Encoder
            IMediaProcessor encoder = GetLatestMediaProcessorByName("Windows Azure Media Encoder");

            // Create a new Job
            IJob job = _context.Jobs.Create("Encode to multi-bitrate and convert to smooth job");

            // Create a new task to encode to adaptive bitrate
            ITask adpativeBitrateTask = job.Tasks.AddNew("MP4 to Adaptive Bitrate Task", encoder, "H264 Adaptive Bitrate MP4 Set 720p", TaskOptions.None);
            adpativeBitrateTask.InputAssets.Add(asset);
            IAsset abrAsset = adpativeBitrateTask.OutputAssets.AddNew("Adaptive Bitrate Asset", AssetCreationOptions.None);

            // Get instance of Windows Media Packager
            IMediaProcessor packager = GetLatestMediaProcessorByName("Windows Azure Media Packager");

            // Windows Media Packager does not accept string presets, so load xml configuration
            string smoothConfig = File.ReadAllText(Path.Combine(_configFilePath, "MediaPackager_MP4toSmooth.xml"));

            // Create a new Task to convert adaptive bitrate to Smooth Streaming
            ITask smoothStreamingTask = job.Tasks.AddNew("Adaptive Bitrate to Smooth Task", packager, smoothConfig, TaskOptions.None);
            smoothStreamingTask.InputAssets.Add(abrAsset);
            smoothStreamingTask.OutputAssets.AddNew("Smooth Asset", AssetCreationOptions.None);

            // Use the following event handler to check job progress.  
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. 
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();


            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // Check for error
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
            }

            return job;
        }

        // Uploads a set of MP4 files and converts to smooth streaming format.
        private static IJob ConvertMultipleBitrateToSmoothStreaming(string configFilePath,
            string inputFolder,
            string outputFolder)
        {
            // Create the asset that contains MP4 multi-bitrate and .ism files 
            IAsset mbrAsset = CreateAssetAndUploadMultipleFiles(AssetCreationOptions.None, inputFolder);

            // Check for the .ism file and set it as the primary file
            var mbrAssetFiles = mbrAsset.AssetFiles.ToList().
                      Where(f => f.Name.EndsWith(".ism", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (mbrAssetFiles.Count() != 1)
                throw new ArgumentException("The asset should have only one, .ism file");

            mbrAssetFiles.First().IsPrimary = true;
            mbrAssetFiles.First().Update();

            // Get an instance of the Windows Azure media packager
            IMediaProcessor packager = GetLatestMediaProcessorByName("Windows Azure Media Packager");

            // Create a new Job
            IJob job = _context.Jobs.Create("Multi Bitrate To Smooth Job");

            // Read in config file
            string smoothConfig = File.ReadAllText(Path.Combine(_configFilePath, "MediaPackager_MP4ToSmooth.xml"));

            // Create a new Task to do the conversion
            ITask mbrToSmoothTask = job.Tasks.AddNew("MBR to Smooth Task", packager, smoothConfig, TaskOptions.None);
            mbrToSmoothTask.InputAssets.Add(mbrAsset);
            mbrToSmoothTask.OutputAssets.AddNew("Smooth Asset", AssetCreationOptions.None);

            // Use the following event handler to check job progress.  
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // Check for error 
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
            }

            return job;
        }

        static IJob ConvertSmoothToHls(string configFilePath, string inputMediaFilePath)
        {
            // Call method to encode an MP4 file to Smooth Streaming
            IJob encodeJob = EncodeToSmoothStreaming(inputMediaFilePath);

            // Get the Smooth Streaming output asset
            IAsset smoothAsset = encodeJob.OutputMediaAssets[0];

            // Find the .ism file and set as the primary file
            var ismAssetFiles = smoothAsset.AssetFiles.ToList().
                       Where(f => f.Name.EndsWith(".ism", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (ismAssetFiles.Count() != 1)
                throw new ArgumentException("The asset should have only one, .ism file");

            ismAssetFiles.First().IsPrimary = true;
            ismAssetFiles.First().Update();

            // Create a new job.
            IJob job = _context.Jobs.Create("Convert Smooth Stream to Apple HLS");

            // Get an instance of the Windows Azure Media Packager
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Packager");

            // Read the configuration data into a string. 
            string configuration = File.ReadAllText(Path.GetFullPath(configFilePath
                + @"\MediaPackager_SmoothToHLS.xml"));


            // Create a task with the encoding details, using a configuration file.
            ITask task = job.Tasks.AddNew("My Smooth to HLS Task",
                processor,
                configuration,
                _protectedConfig);

            // Specify the input asset to be encoded.
            task.InputAssets.Add(smoothAsset);

            // Add an output asset to contain the results of the job.
            task.OutputAssets.AddNew("HLS asset",
                AssetCreationOptions.None);

            // Use the following event handler to check job progress.  
            job.StateChanged += new
                    EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details.
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();


            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // Check for errors
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
            }

            return job;
        }
        #endregion

        #region Samples from Manage Assets with the Media Services SDK for .NET
        static IAsset GetAsset(string assetId)
        {
            // Use a LINQ Select query to get an asset.
            var assetInstance =
                from a in _context.Assets
                where a.Id == assetId
                select a;
            // Reference the asset as an IAsset.
            IAsset asset = assetInstance.FirstOrDefault();

            return asset;
        }

        static IJob GetJob(string jobId)
        {
            // Use a Linq select query to get an updated 
            // reference by Id. 
            var jobInstance =
                from j in _context.Jobs
                where j.Id == jobId
                select j;
            // Return the job reference as an Ijob. 
            IJob job = jobInstance.FirstOrDefault();

            return job;
        }

        // List all assets in the context object. 
        static void ListAssets()
        {
            string waitMessage = "Building the list. This may take a few "
                + "seconds to a few minutes depending on how many assets "
                + "you have."
                + Environment.NewLine + Environment.NewLine
                + "Please wait..."
                + Environment.NewLine;
            Console.Write(waitMessage);

            // Create a Stringbuilder to store the list that we build. 
            StringBuilder builder = new StringBuilder();

            foreach (IAsset asset in _context.Assets)
            {
                // Display the collection of assets.
                builder.AppendLine("");
                builder.AppendLine("******ASSET******");
                builder.AppendLine("Asset ID: " + asset.Id);
                builder.AppendLine("Name: " + asset.Name);
                builder.AppendLine("==============");
                builder.AppendLine("******ASSET FILES******");

                // Display the files associated with each asset. 
                foreach (IAssetFile fileItem in asset.AssetFiles)
                {
                    builder.AppendLine("Name: " + fileItem.Name);
                    builder.AppendLine("Size: " + fileItem.ContentFileSize);
                    builder.AppendLine("==============");
                }
            }

            // Display output in console.
            Console.Write(builder.ToString());
        }

        // List all jobs on the server, and for each job, also all assets
        static void ListJobsAndAssets()
        {
            string waitMessage = "Building the list. This may take a few "
                + "seconds to a few minutes depending on how many assets "
                + "you have."
                + Environment.NewLine + Environment.NewLine
                + "Please wait..."
                + Environment.NewLine;
            Console.Write(waitMessage);

            // Create a Stringbuilder to store the list that we build. 
            StringBuilder builder = new StringBuilder();

            foreach (IJob job in _context.Jobs)
            {
                // Display the collection of jobs on the server.
                builder.AppendLine("");
                builder.AppendLine("******JOB*******");
                builder.AppendLine("Job ID: " + job.Id);
                builder.AppendLine("Name: " + job.Name);
                builder.AppendLine("State: " + job.State);
                builder.AppendLine("Order: " + job.Priority);
                builder.AppendLine("==============");


                // For each job, display the associated tasks (a job  
                // has one or more tasks). 
                builder.AppendLine("******TASKS*******");
                foreach (ITask task in job.Tasks)
                {
                    builder.AppendLine("Task Id: " + task.Id);
                    builder.AppendLine("Name: " + task.Name);
                    builder.AppendLine("Progress: " + task.Progress);
                    builder.AppendLine("Configuration: " + task.Configuration);
                    if (task.ErrorDetails != null)
                    {
                        builder.AppendLine("Error: " + task.ErrorDetails);
                    }
                    builder.AppendLine("==============");

                }

                // For each job, display the list of input media assets.
                builder.AppendLine("******JOB INPUT MEDIA ASSETS*******");
                foreach (IAsset inputAsset in job.InputMediaAssets)
                {

                    if (inputAsset != null)
                    {
                        builder.AppendLine("Input Asset Id: " + inputAsset.Id);
                        builder.AppendLine("Name: " + inputAsset.Name);
                        builder.AppendLine("==============");
                    }

                }

                // For each job, display the list of output media assets.
                builder.AppendLine("******JOB OUTPUT MEDIA ASSETS*******");
                foreach (IAsset theAsset in job.OutputMediaAssets)
                {
                    if (theAsset != null)
                    {
                        builder.AppendLine("Output Asset Id: " + theAsset.Id);
                        builder.AppendLine("Name: " + theAsset.Name);
                        builder.AppendLine("==============");
                    }
                }
            }

            // Display output in console.
            Console.Write(builder.ToString());
        }

        static void ListAllPolicies()
        {
            foreach (IAccessPolicy policy in _context.AccessPolicies)
            {
                Console.WriteLine("");
                Console.WriteLine("Name:  " + policy.Name);
                Console.WriteLine("ID:  " + policy.Id);
                Console.WriteLine("Permissions: " + policy.Permissions);
                Console.WriteLine("==============");
            }
        }

        static void ListAllLocators()
        {
            foreach (ILocator locator in _context.Locators)
            {
                Console.WriteLine("***********");
                Console.WriteLine("Locator Id: " + locator.Id);
                Console.WriteLine("Locator asset Id: " + locator.AssetId);
                Console.WriteLine("Locator access policy Id: " + locator.AccessPolicyId);
                Console.WriteLine("Access policy permissions: " + locator.AccessPolicy.Permissions);
                Console.WriteLine("Locator expiration: " + locator.ExpirationDateTime);
                // The locator path is the base or parent path (with included permissions) to access  
                // the media content of an asset. To create a full URL to a specific media file, take 
                // the locator path and then append a file name and info as needed.  
                Console.WriteLine("Locator base path: " + locator.Path);
                Console.WriteLine("");
            }
        }

        static void DeleteAsset(IAsset asset)
        {
            // delete the asset
            asset.Delete();

            // Verify asset deletion
            if (GetAsset(asset.Id) == null)
                Console.WriteLine("Deleted the Asset");

        }

        // Deletes a job based on its state.
        // You can create a loop to call this method and delete all jobs:   
        // foreach(IJob job in _context.Jobs)
        //       DeleteJob(job.Id)
        // **Warning:  if you call this job in a foreach loop as above, do so  
        // with caution as it will try to delete all jobs in an account. 
        static void DeleteJob(string jobId)
        {
            bool jobDeleted = false;

            while (!jobDeleted)
            {
                // Get an updated job reference.  
                IJob job = GetJob(jobId);

                // Check and handle various possible job states. You can 
                // only delete a job whose state is Finished, Error, or Canceled.   
                // You can cancel jobs that are Queued, Scheduled, or Processing,  
                // and then delete after they are canceled.
                switch (job.State)
                {
                    case JobState.Finished:
                    case JobState.Canceled:
                    case JobState.Error:
                        // Job errors should already be logged by the StateChanged event 
                        // handling method.
                        // You can also call job.DeleteAsync to do async deletes.
                        job.Delete();
                        Console.WriteLine("Job has been deleted.");
                        jobDeleted = true;
                        break;
                    case JobState.Canceling:
                        Console.WriteLine("Job is cancelling and will be deleted "
                            + "when finished.");
                        Console.WriteLine("Wait while job finishes canceling...");
                        Thread.Sleep(5000);
                        break;
                    case JobState.Queued:
                    case JobState.Scheduled:
                    case JobState.Processing:
                        job.Cancel();
                        Console.WriteLine("Job is scheduled or processing and will "
                            + "be deleted.");
                        break;
                    default:
                        break;
                }

            }
        }

        static void DeleteAccessPolicy(string existingPolicyId)
        {
            // To delete a specific access policy, get a reference to the policy.  
            // based on the policy Id passed to the method.
            var policyInstance =
                 from p in _context.AccessPolicies
                 where p.Id == existingPolicyId
                 select p;
            IAccessPolicy policy = policyInstance.FirstOrDefault();

            policy.Delete();

        }
        #endregion

        #region Samples from Deliver Assets with the Media Services SDK for .NET
        // Download an asset that is the output of a job to a local folder.
        static IAsset DownloadAssetToLocal(string jobId, string outputFolder)
        {
            // This method illustrates how to download a single asset. 
            // However, you can iterate through the OutputAssets
            // collection, and download all assets if there are many. 

            // Get a reference to the job. 
            IJob job = GetJob(jobId);
            // Get a reference to the first output asset. If there were multiple 
            // output media assets you could iterate and handle each one.
            IAsset outputAsset = job.OutputMediaAssets[0];

            IAccessPolicy accessPolicy = _context.AccessPolicies.Create("File Download Policy", TimeSpan.FromDays(30), AccessPermissions.Read);
            ILocator locator = _context.Locators.CreateSasLocator(outputAsset, accessPolicy);
            BlobTransferClient blobTransfer = new BlobTransferClient
            {
                NumberOfConcurrentTransfers = 10,
                ParallelTransferThreadCount = 10
            };

            var downloadTasks = new List<Task>();
            foreach (IAssetFile outputFile in outputAsset.AssetFiles)
            {
                // Use the following event handler to check download progress.
                outputFile.DownloadProgressChanged += DownloadProgress;

                string localDownloadPath = Path.Combine(outputFolder, outputFile.Name);

                Console.WriteLine("File download path:  " + localDownloadPath);

                downloadTasks.Add(outputFile.DownloadAsync(Path.GetFullPath(localDownloadPath), blobTransfer, locator, CancellationToken.None));

                outputFile.DownloadProgressChanged -= DownloadProgress;
            }

            Task.WaitAll(downloadTasks.ToArray());

            return outputAsset;
        }

        // Create a list of SAS URLs to all files in an asset.
        static void BuildAndSaveAssetSasUrlList(IAsset outputAsset)
        {
            // Declare an access policy for permissions on the asset. 
            // You can call an async or sync create method. 
            IAccessPolicy policy =
                _context.AccessPolicies.Create("My 30 day readonly policy",
                    TimeSpan.FromDays(30),
                    AccessPermissions.Read);

            // Create a SAS locator to enable direct access to the asset 
            // in blob storage. You can call a sync or async create method.  
            // You can set the optional startTime param as 5 minutes 
            // earlier than Now to compensate for differences in time  
            // between the client and server clocks. 
            ILocator locator = _context.Locators.CreateLocator(LocatorType.Sas, outputAsset,
                policy,
                DateTime.UtcNow.AddMinutes(-5));


            // Declare a list to contain all the SAS URLs.
            List<String> fileSasUrlList = new List<String>();

            string outFilePath = Path.GetFullPath(_outputFilesFolder + @"\" + "FileSasUrlList.txt");

            // If the asset has files, build a list of URLs to 
            // each file in the asset and return. 
            foreach (IAssetFile file in outputAsset.AssetFiles)
            {
                string sasUrl = BuildFileSasUrl(file, locator);
                fileSasUrlList.Add(sasUrl);

                Console.WriteLine(sasUrl);

                // Write the URL list to a local file. You can use the saved 
                // SAS URLs to browse directly to the files in the asset.
                WriteToFile(outFilePath, sasUrl);
            }
        }

        // Create and return a SAS URL to a single file in an asset. 
        static string BuildFileSasUrl(IAssetFile file, ILocator locator)
        {
            // Take the locator path, add the file name, and build 
            // a full SAS URL to access this file. This is the only 
            // code required to build the full URL.
            var uriBuilder = new UriBuilder(locator.Path);
            Path.Combine(uriBuilder.Path, file.Name);

            // Optional:  print the locator.Path to the asset, and 
            // the full SAS URL to the file
            Console.WriteLine("Locator path: ");
            Console.WriteLine(locator.Path);
            Console.WriteLine();
            Console.WriteLine("Full URL to file: ");
            Console.WriteLine(uriBuilder.Uri.AbsoluteUri);
            Console.WriteLine();


            //Return the SAS URL.
            return uriBuilder.Uri.AbsoluteUri;
        }

        // Write method output to the output files folder.
        static void WriteToFile(string outFilePath, string fileContent)
        {
            StreamWriter sr = File.CreateText(outFilePath);
            sr.WriteLine(fileContent);
            sr.Close();
        }

        // Create a locator URL to a streaming media asset 
        // on an origin server.
        private static ILocator GetStreamingOriginLocator(string targetAssetID, MediaContentType contentType)
        {
            // Get a reference to the asset you want to stream.
            IAsset assetToStream = GetAsset(targetAssetID);

            // Get a reference to the streaming manifest file from the  
            // collection of files in the asset. 
            var theManifest =
                                from f in assetToStream.AssetFiles
                                where f.Name.EndsWith(".ism")
                                select f;

            // Cast the reference to a true IAssetFile type. 
            IAssetFile manifestFile = theManifest.First();

            // Create a 30-day readonly access policy. 
            IAccessPolicy policy = _context.AccessPolicies.Create("Streaming policy",
                TimeSpan.FromDays(30),
                AccessPermissions.Read);

            // Create a locator to the streaming content on an origin. 
            ILocator originLocator = _context.Locators.CreateLocator(LocatorType.OnDemandOrigin, assetToStream,
                policy,
                DateTime.UtcNow.AddMinutes(-5));

            // Display some useful values based on the locator.
            // Display the base path to the streaming asset on the origin server.
            Console.WriteLine("Streaming asset base path on origin: ");
            Console.WriteLine(originLocator.Path);
            Console.WriteLine();

            // Create a full URL to the manifest file. Use this for playback
            // in streaming media clients. 
            string urlForClientStreaming = originLocator.Path + manifestFile.Name + "/manifest";
            if (contentType == MediaContentType.HLS)
                urlForClientStreaming = String.Format("{0}{1}", urlForClientStreaming, "(format=m3u8-aapl)");

            Console.WriteLine("URL to manifest for client streaming: ");
            Console.WriteLine(urlForClientStreaming);
            Console.WriteLine();

            // Display the ID of the origin locator, the access policy, and the asset.
            Console.WriteLine("Origin locator Id: " + originLocator.Id);
            Console.WriteLine("Access policy Id: " + policy.Id);
            Console.WriteLine("Streaming asset Id: " + assetToStream.Id);

            // For convenience, write the URL to a local file. Use the saved 
            // streaming URL to browse directly to the asset in a smooth streaming client.  
            string outFilePath = Path.GetFullPath(_outputFilesFolder + @"\" + "StreamingUrl.txt");
            WriteToFile(outFilePath, urlForClientStreaming);


            // Return the locator. 
            return originLocator;
        }

        #endregion

        #region Samples from Live Streaming with the Media Services SDK for .NET

        /// <summary>
        /// Demonstrates how to setup a Live program together with it's prerequisites (Channel and Origin).
        /// </summary>
        static void SetupLiveStreaming()
        {
            IChannel channel = CreateLiveChannle();

            IAsset asset = _context.Assets.First();

            IProgram program = CreateLiveProgram(channel, asset);

            IOrigin origin = CreateOrigin();

            channel.Start();

            program.Start();

            origin.Start();
        }

        /// <summary>
        /// Creates a demo Origin.
        /// </summary>
        private static IOrigin CreateOrigin()
        {
            OriginSettings settings = MakeOriginSettings();
            IOrigin origin = _context.Origins.Create(
                name: "testorigin", 
                description: "test origin", 
                reservedUnits: 1, 
                settings: settings);
            return origin;
        }

        /// <summary>
        /// Creates a demo Program.
        /// </summary>
        private static IProgram CreateLiveProgram(IChannel channel, IAsset asset)
        {
            IProgram program = channel.Programs.Create(
                name: "testprogram",
                enableArchive: false,
                dvrWindowLength: StreamingConstants.InfiniteDvrLenth,
                estimatedDuration: TimeSpan.FromHours(1),
                assetId: asset.Id);

            return program;
        }

        /// <summary>
        /// Creates a demo Channel.
        /// </summary>
        /// <returns></returns>
        private static IChannel CreateLiveChannle()
        {
            ChannelSettings settings = MakeChannelSettings();
            IChannel channel = _context.Channels.Create("test", ChannelSize.Large, settings);
            return channel;
        }

        /// <summary>
        /// Creates minimalistic channel settings.
        /// </summary>
        private static ChannelSettings MakeChannelSettings()
        {
            var settings = new ChannelSettings
            {
                Ingest = new IngestEndpointSettings
                {
                    Security = new SecuritySettings
                    {
                        AkamaiG20Authentication = new List<G20Key> 
                        { 
                            new G20Key { Base64Key = "vUeuvDU3MIgHuFZCU3cX+24wWg6r4qho594cRcEr5fU=", Expiration = new DateTime(2018, 10, 30), Identifier = "id1" },
                        },
                    }
                },
            };

            return settings;
        }

        /// <summary>
        /// Creates minimalistic origin settings.
        /// </summary>
        private static OriginSettings MakeOriginSettings()
        {
            var settings = new OriginSettings
            {
                Playback = new PlaybackEndpointSettings
                {
                    Security = new SecuritySettings
                    {
                        Ipv4Whitelist = new List<Ipv4>
                        {
                            new Ipv4 { Name = "testName1", IP = "1.1.1.1" },
                            new Ipv4 { Name = "testName2", IP = "1.1.1.2" },
                        }
                    }
                },
            };

            return settings;
        }

        #endregion

        private static IJob CreatePlayReadyProtectionJob(string inputMediaFilePath, string configFilePath)
        {
            // Create a storage-encrypted asset and upload the mp4. 
            IAsset asset = CreateAssetAndUploadSingleFile(AssetCreationOptions.StorageEncrypted, inputMediaFilePath);

            // Declare a new job to contain the tasks
            IJob job = _context.Jobs.Create("My PlayReady Protection job");

            // Set up the first task. 

            // Read the task configuration data into a string
            string configMp4ToSmooth = File.ReadAllText(Path.GetFullPath(configFilePath + @"\MediaPackager_MP4ToSmooth.xml"));

            // Get a media processor reference
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Packager");

            // Create a task with the conversion details, using the configuration data
            ITask task = job.Tasks.AddNew("My Mp4 to Smooth Task",
                processor,
                configMp4ToSmooth,
                _clearConfig);

            // Specify the input asset to be converted.
            task.InputAssets.Add(asset);

            // Add an output asset to contain the results of the job.
            task.OutputAssets.AddNew("Streaming output asset", AssetCreationOptions.None);

            IAsset smoothOutputAsset = task.OutputAssets[0];

            // Set up the second task. 

            // Read the configuration data into a string. 
            string configPlayReady = File.ReadAllText(Path.GetFullPath(configFilePath + @"\MediaEncryptor_PlayReadyProtection.xml"));

            // Get a media processor reference
            IMediaProcessor playreadyProcessor = GetLatestMediaProcessorByName("Windows Azure Media Encryptor");

            // Create a second task. 
            ITask playreadyTask = job.Tasks.AddNew("My PlayReady Task",
                playreadyProcessor,
                configPlayReady,
                _protectedConfig);

            // Add the input asset, which is the smooth streaming output asset from the first task. 
            playreadyTask.InputAssets.Add(smoothOutputAsset);

            // Add an output asset to contain the results of the job.
            playreadyTask.OutputAssets.AddNew("PlayReady protected output asset",
                AssetCreationOptions.None);

            // Use the following event handler to check job progress. 
            job.StateChanged += new EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. 
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            // Get a refreshed job reference after waiting on a thread.
            job = GetJob(job.Id);

            // Check for errors
            if (job.State == JobState.Error)
            {
                Console.WriteLine("\nExiting method due to job error.");
            }

            return job;

        }

        #region HelperMethods

        static void DownloadProgress(object sender, Microsoft.WindowsAzure.MediaServices.Client.DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(string.Format("Asset File:{0}  {1}% download progress. ", ((IAssetFile)sender).Name, e.Progress));
        }

        static public void DeleteAssetFilesForAsset(IAsset asset)
        {
            foreach (IAssetFile file in asset.AssetFiles)
            {
                Console.WriteLine("Deleting asset file with id: {0} {1}", file.Id, file.Name);
                file.Delete();
            }
        }

        public static void DeleteLocatorsForAsset(IAsset asset)
        {
            string assetId = asset.Id;
            var locators = from a in _context.Locators
                           where a.AssetId == assetId
                           select a;
            foreach (var locator in locators)
            {
                Console.WriteLine("Deleting locator {0} for asset {1}", locator.Path, assetId);

                locator.Delete();
            }
        }

        static CloudMediaContext GetContext()
        {
            return new CloudMediaContext(_accountName, _accountKey);
        }

        private static void StateChanged(object sender, JobStateChangedEventArgs e)
        {
            Console.WriteLine("Job state changed event:");
            Console.WriteLine("  Previous state: " + e.PreviousState);
            Console.WriteLine("  Current state: " + e.CurrentState);

            switch (e.CurrentState)
            {
                case JobState.Finished:
                    Console.WriteLine();
                    Console.WriteLine("********************");
                    Console.WriteLine("Job is finished.");
                    Console.WriteLine("Please wait while local tasks or downloads complete...");
                    Console.WriteLine("********************");
                    Console.WriteLine();
                    Console.WriteLine();
                    break;
                case JobState.Canceling:
                case JobState.Queued:
                case JobState.Scheduled:
                case JobState.Processing:
                    Console.WriteLine("Please wait...\n");
                    break;
                case JobState.Canceled:
                case JobState.Error:
                    // Cast sender as a job.
                    IJob job = (IJob)sender;
                    // Display or log error details as needed.
                    LogJobStop(job.Id);
                    break;
                default:
                    break;
            }
        }

        private static void LogJobStop(string jobId)
        {
            StringBuilder builder = new StringBuilder();
            IJob job = GetJob(jobId);

            builder.AppendLine("\nThe job stopped due to cancellation or an error.");
            builder.AppendLine("***************************");
            builder.AppendLine("Job ID: " + job.Id);
            builder.AppendLine("Job Name: " + job.Name);
            builder.AppendLine("Job State: " + job.State.ToString());
            builder.AppendLine("Job started (server UTC time): " + job.StartTime.ToString());
            builder.AppendLine("Media Services account name: " + _accountName);
            // Log job errors if they exist.  
            if (job.State == JobState.Error)
            {
                builder.Append("Error Details: \n");
                foreach (ITask task in job.Tasks)
                {
                    foreach (ErrorDetail detail in task.ErrorDetails)
                    {
                        builder.AppendLine("  Task Id: " + task.Id);
                        builder.AppendLine("    Error Code: " + detail.Code);
                        builder.AppendLine("    Error Message: " + detail.Message + "\n");
                    }
                }
            }
            builder.AppendLine("***************************\n");

            // Write the output to a local file and to the console. The template 
            // for an error output file is:  JobStop-{JobId}.txt
            string outputFile = Path.Combine(_outputFilesFolder, @"JobStop-" + JobIdAsFileName(job.Id) + ".txt");
            WriteToFile(outputFile, builder.ToString());
            Console.Write(builder.ToString());
        }

        private static void LogJobDetails(string jobId)
        {
            StringBuilder builder = new StringBuilder();
            IJob job = GetJob(jobId);

            builder.AppendLine("\nJob ID: " + job.Id);
            builder.AppendLine("Job Name: " + job.Name);
            builder.AppendLine("Job submitted (client UTC time): " + DateTime.UtcNow.ToString());
            builder.AppendLine("Media Services account name: " + _accountName);

            // Write the output to a local file and to the console. The template 
            // for an error output file is:  JobDetails-{JobId}.txt
            string outputFile = Path.Combine(_outputFilesFolder, @"JobDetails-" + JobIdAsFileName(job.Id) + ".txt");
            WriteToFile(outputFile, builder.ToString());
            Console.Write(builder.ToString());
        }

        // Replace ":" with "_" in Job id values so they can 
        // be used as log file names.  
        private static string JobIdAsFileName(string jobID)
        {
            return jobID.Replace(":", "_");
        }

        #endregion

    } //class Program
} //namespace ConsoleApplication1
