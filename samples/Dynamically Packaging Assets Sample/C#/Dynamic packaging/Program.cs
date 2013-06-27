using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MediaServices.Client;
 
namespace Dynamic_packaging
{
    public enum MediaContentType
    {
        SmoothStreaming,
        HLS
    }

    class Program
    {
        // Base support files path.  Update this field to point to the base path  
        // for the local support files folder that you create. 
        private static readonly string _supportFiles =
            Path.GetFullPath(@"../..\supportFiles");

        // Paths to support files (within the above base path). You can use 
        // the provided sample media files from the "supportFiles" folder, or 
        private static readonly string _singleInputMp4Path =
            Path.GetFullPath(_supportFiles + @"\MP4files\BigBuckBunny.mp4");

        private static readonly string _multibitrateFilesFolder =
            Path.GetFullPath(_supportFiles + @"\multibitratefiles");

        private static readonly string _configFilePath =
            Path.GetFullPath(_supportFiles + @"\configuration");

        private static readonly string _outputFilesFolder =
            Path.GetFullPath(_supportFiles + @"\outputfiles");

        // ********************************
        // Authentication and connection settings.  These settings are pulled from 
        // the App.Config file and are required to connect to Media Services, 
        // authenticate, and get a token so that you can access the server context.
        // These values are also used in the LogJobStop and LogJobDetails methods. 

        private static readonly string _accountName = ConfigurationManager.AppSettings["MediaServicesAccountName"];
        private static readonly string _accountKey = ConfigurationManager.AppSettings["MediaServicesAccountKey"];

        // Class-level field used to keep a reference to the service context.
        private static CloudMediaContext _context = null;

        static void Main(string[] args)
        {
            // Get server context.    
            _context = new CloudMediaContext(_accountName, _accountKey);

            // The EncodeFilesAndGetOriginLocator function does the following: 
            // 1. Creates an asset and uploads a single file.
            // 2. Runs an encoding job to produce H.264 MP4 adaptive bitrate set. 
            // 3. Creates an Origin locator and composes URLs.

            EncodeFilesAndCreateOriginLocator();

            // The ValidateFilesAndGetOriginLocator function does the following: 
            // 1. Creates an asset and uploads existing MP4 bitrate set.
            // 2. Validates the uploaded files.  
            // 3. Creates an Origin locator and composes URL string.
            ValidateFilesAndCreateOriginLocator();
         }

        static public void EncodeFilesAndCreateOriginLocator()
        {
            IAsset assetSingleFile = CreateAssetAndUploadSingleFile(AssetCreationOptions.None, 
                _singleInputMp4Path);
            IJob job = CreateEncodingJob(assetSingleFile);
            if (job.State != JobState.Error)
            {
                // Get the locator for Smooth Streaming
                GetStreamingOriginLocator(job.OutputMediaAssets[0], MediaContentType.SmoothStreaming);

                // This function downloads all the files in the asset 
                // to the local output folder. 
                DownloadAssetToLocal(job.Id, _outputFilesFolder);
            }
        }

        static public void ValidateFilesAndCreateOriginLocator()
        {
            IAsset assetMP4Files = CreateAssetAndUploadMultipleFiles(AssetCreationOptions.None, 
                _multibitrateFilesFolder);

            IJob job = ValidateMP4Asset(assetMP4Files, _configFilePath);

            if (job.State != JobState.Error)
            {
                // Get the locator for Smooth Streaming
                GetStreamingOriginLocator(job.OutputMediaAssets[0], MediaContentType.SmoothStreaming);

                // This function downloads all the files in the asset 
                // to the local output folder. 
                DownloadAssetToLocal(job.Id, _outputFilesFolder);
            }
        }
        static public IAsset CreateAssetAndUploadSingleFile(AssetCreationOptions assetCreationOptions, string singleFilePath)
        {
            // For the AssetCreationOptions you can specify 
            // encryption options.
            //      None:  no encryption. By default, storage encryption is used. If you want to 
            //        create an unencrypted asset, you must set this option.
            //      StorageEncrypted:  storage encryption. Encrypts a clear input file 
            //        before it is uploaded to Azure storage. This is the default if not specified
            //      CommonEncryptionProtected:  for Common Encryption Protected (CENC) files. An 
            //        example is a set of files that are already PlayReady encrypted. 

            var assetName = "UploadSingleFile_" + DateTime.UtcNow.ToString();
            var asset = CreateEmptyAsset(assetName, assetCreationOptions);

            var fileName = Path.GetFileName(singleFilePath);

            var assetFile = asset.AssetFiles.Create(fileName);

            Console.WriteLine("Created assetFile {0}", assetFile.Name);

            var accessPolicy = _context.AccessPolicies.Create(assetName, TimeSpan.FromDays(30),
                                                              AccessPermissions.Write | AccessPermissions.List);

            var locator = _context.Locators.CreateLocator(LocatorType.Sas, asset, accessPolicy);


            Console.WriteLine("Upload {0}", assetFile.Name);

            assetFile.Upload(singleFilePath);
            Console.WriteLine("Done uploading of {0}", assetFile.Name);

            locator.Delete();
            accessPolicy.Delete();

            return asset;
        }

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

        static public IJob CreateEncodingJob(IAsset asset)
        {
            // Declare a new job.
            IJob job = _context.Jobs.Create("My encoding job");

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task.
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Encoder");

            // Create a task with the encoding details, using a string preset.
            // In this case "H264 Adaptive Bitrate MP4 Set 720p" preset is used.
            ITask task = job.Tasks.AddNew("My encoding task",
                processor,
                "H264 Adaptive Bitrate MP4 Set 720p",
                TaskOptions.None);

            // Specify the input asset to be encoded.
            task.InputAssets.Add(asset);

            // Add an output asset to contain the results of the job. 
            // This output is specified as AssetCreationOptions.None, which 
            // means the output asset is in the clear (unencrypted). 
            task.OutputAssets.AddNew("Output asset",
                AssetCreationOptions.None);

            // Use the following event handler to check job progress.  
            job.StateChanged += new
                    EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. This displays basic job details
            // to the console and saves them to a JobDetails-{JobId}.txt file 
            // in your output folder.
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            // Get an updated job reference.
            job = GetJob(job.Id);

            return job;
        }

        static public IJob ValidateMP4Asset(IAsset asset, string configFilePath)
        {
            // The .ism file must be set as a primary file.
            SetISMFileAsPrimary(asset);

            // Declare a new job to contain the tasks.
            IJob job = _context.Jobs.Create("MP4 validation job.");

            // Read the task configuration data into a string. 
            string configMp4Validation = File.ReadAllText(Path.GetFullPath(configFilePath + @"\MediaPackager_ValidateTask.xml"));

            // Get a media processor reference, and pass to it the name of the 
            // processor to use for the specific task.
            IMediaProcessor processor = GetLatestMediaProcessorByName("Windows Azure Media Packager");

            // Create a task with the conversion details, using the configuration data. 
            ITask task = job.Tasks.AddNew("Mp4 Validation Task",
                processor,
                configMp4Validation,
                TaskOptions.None);

            // Specify the input asset to be validated.
            task.InputAssets.Add(asset);
            // Add an output asset to contain the results of the job. 
            // This output is specified as AssetCreationOptions.None, which 
            // means the output asset is in the clear (unencrypted). 

            task.OutputAssets.AddNew("Output asset",
                AssetCreationOptions.None);

            // Use the following event handler to check job progress. 
            job.StateChanged += new
                    EventHandler<JobStateChangedEventArgs>(StateChanged);

            // Launch the job.
            job.Submit();

            // Optionally log job details. This displays basic job details
            // to the console and saves them to a JobDetails-{JobId}.txt file 
            // in your output folder.
            LogJobDetails(job.Id);

            // Check job execution and wait for job to finish. 
            Task progressJobTask = job.GetExecutionProgressTask(CancellationToken.None);
            progressJobTask.Wait();

            // Get an updated job reference.
            job = GetJob(job.Id);

            return job;
        }

        // Create a locator URL to a streaming media asset 
        // on an origin server.
        static public ILocator GetStreamingOriginLocator(IAsset assetToStream, MediaContentType contentType)
        {
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

            // For convenience, write the URL to a local file. Use the saved 
            // streaming URL to browse directly to the asset in a smooth streaming client.  
            string outFilePath = Path.GetFullPath(_outputFilesFolder + @"\" + "StreamingUrl.txt");
            WriteToFile(outFilePath, urlForClientStreaming);


            // Return the locator. 
            return originLocator;
        }

        static public IAsset DownloadAssetToLocal(string jobId, string outputFolder)
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
            ILocator locator = _context.Locators.CreateLocator(LocatorType.Sas, outputAsset, accessPolicy);
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


        static private IAsset CreateEmptyAsset(string assetName, AssetCreationOptions assetCreationOptions)
        {
            var asset = _context.Assets.Create(assetName, assetCreationOptions);

            Console.WriteLine("Asset name: " + asset.Name);
            Console.WriteLine("Time created: " + asset.Created.Date.ToString());

            return asset;
        }

        static private void blobTransferClient_TransferProgressChanged(object sender, BlobTransferProgressChangedEventArgs e)
        {
            Console.WriteLine("{0}% upload competed for {1}.", e.ProgressPercentage, e.LocalFile);
        }

        static private void SetISMFileAsPrimary(IAsset asset)
        {
            var ismAssetFiles = asset.AssetFiles.ToList().Where(f => f.Name.EndsWith(".ism", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (ismAssetFiles.Count() != 1)
                throw new ArgumentException("The asset should have only one, .ism file");

            ismAssetFiles.First().IsPrimary = true;
            ismAssetFiles.First().Update();
        }


        static private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(string.Format("{0} % download progress. ", e.Progress));
        }

        static private IMediaProcessor GetLatestMediaProcessorByName(string mediaProcessorName)
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

        // This method is a handler for events that track job progress.   
        static private void StateChanged(object sender, JobStateChangedEventArgs e)
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

        static private void LogJobStop(string jobId)
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
            string outputFile = _outputFilesFolder + @"\JobStop-" + JobIdAsFileName(job.Id) + ".txt";
            WriteToFile(outputFile, builder.ToString());
            Console.Write(builder.ToString());
        }

        static private void LogJobDetails(string jobId)
        {
            StringBuilder builder = new StringBuilder();
            IJob job = GetJob(jobId);

            builder.AppendLine("\nJob ID: " + job.Id);
            builder.AppendLine("Job Name: " + job.Name);
            builder.AppendLine("Job submitted (client UTC time): " + DateTime.UtcNow.ToString());
            builder.AppendLine("Media Services account name: " + _accountName);

            // Write the output to a local file and to the console. The template 
            // for an error output file is:  JobDetails-{JobId}.txt
            string outputFile = _outputFilesFolder + @"\JobDetails-" + JobIdAsFileName(job.Id) + ".txt";
            WriteToFile(outputFile, builder.ToString());
            Console.Write(builder.ToString());
        }

        // Replace ":" with "_" in Job id values so they can 
        // be used as log file names.  
        static private string JobIdAsFileName(string jobID)
        {
            return jobID.Replace(":", "_");
        }

        // Write method output to the output files folder.
        static private void WriteToFile(string outFilePath, string fileContent)
        {
            StreamWriter sr = File.CreateText(outFilePath);
            sr.WriteLine(fileContent);
            sr.Close();
        }

        static private IJob GetJob(string jobId)
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

        static private IAsset GetAsset(string assetId)
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
    }
}
