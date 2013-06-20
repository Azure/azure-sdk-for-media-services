using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;
using System.Web;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml.Linq;
using System.Threading;

namespace Microsoft.Samples.RESTAPI
{
    class Program
    {
        #region AppSettings
        private static Uri serviceURI = new Uri(ConfigurationManager.AppSettings["serviceURI"]);
        private static readonly string accessControlURI = ConfigurationManager.AppSettings["accessControlURI"];
        private static readonly string clientSecret = ConfigurationManager.AppSettings["accountKey"];
        private static readonly string clientId  = ConfigurationManager.AppSettings["accountName"];
        private static readonly string scope = ConfigurationManager.AppSettings["scope"];
        #endregion

        #region storage account credentials
        // WAMS storage account credentials used for file upload example.
        private static readonly string _storageAccountName = ConfigurationManager.AppSettings["WamsStorageAccountName"];
        private static readonly string _storageAccountKey = ConfigurationManager.AppSettings["WamsStorageAccountKey"];
        #endregion

        #region Paths
        private static readonly string _supportFiles = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\supportFiles";
        private static readonly string _singleInputFilePath = Path.GetFullPath(_supportFiles + @"\multifile\interview2.wmv");
        private static readonly string _singleInputMp4Path = Path.GetFullPath(_supportFiles + @"\multifile\BigBuckBunny.mp4");
        private static readonly string _primaryFilePath = Path.GetFullPath(_supportFiles + @"\multifile\interview1.wmv");
        private static readonly string _inputFilesFolder = Path.GetFullPath(_supportFiles + @"\multifile");
        private static readonly string _streamingInputFilesFolder = Path.GetFullPath(_supportFiles + @"\streamingfiles");
        private static readonly string _configFilePath = Path.GetFullPath(_supportFiles + @"\configuration");
        private static readonly string _outputFilesFolder = Path.GetFullPath(_supportFiles + @"\outputfiles");
        private static readonly string _multibitrateFilesFolder = Path.GetFullPath(_supportFiles + @"\multibitratefiles");
        private static readonly string _primaryMultibitrateFile = Path.GetFullPath(_supportFiles + @"\multibitratefiles\interview1_384p_H264_350Kbps_AAC.mp4");
        #endregion

        private static string token = null;
        private static int TimeOuts = 0;

        static void Main(string[] args)
        {
            token = GetACSToken(accessControlURI, clientId, clientSecret, scope);
           // Call the method(s) of your choice here
          
        }
   
        #region  Samples from Ingest Assets with the Media Services REST API

        private static XmlDocument CreateEmptyAsset(string name, string options = null)
        {
            XmlDocument assetXmlResponse = null;

            string requestbody;

            if (options != null)
            {
                requestbody = "{ \"Name\" : \"" + name + "\", \"Options\" : " + options + " }";
            }
            else
            {
                requestbody = "{ \"Name\" : \"" + name + "\" }";
            }

            // Generate HTTP POST request to create an Asset    
            assetXmlResponse = GenerateRequestAndGetResponse("POST", "Assets", null, requestbody);

            // Display results
            Console.WriteLine("\nAsset Id: {0}", assetXmlResponse.GetElementsByTagName("Id")[0].InnerText);
            Console.WriteLine("Name: {0}", assetXmlResponse.GetElementsByTagName("Name")[0].InnerText);
            Console.WriteLine("State: {0}", assetXmlResponse.GetElementsByTagName("State")[0].InnerText);
            Console.WriteLine("Options: {0}", assetXmlResponse.GetElementsByTagName("Options")[0].InnerText);

            return assetXmlResponse;
        }

        static public string CreateAssetAndUploadSingleFile(string token, string singleFilePath)
        {
            // Get the name of the video file to upload without the extension
            // this will be used to name the Asset
            string videoFileName = Path.GetFileNameWithoutExtension(singleFilePath);

            // Create the Asset
            Console.WriteLine("=== Create Asset ===");
            XmlDocument assetXmlResponse = CreateEmptyAsset(videoFileName + "_Asset");
            string assetId = assetXmlResponse.GetElementsByTagName("Id")[0].InnerText;
            string assetName = assetXmlResponse.GetElementsByTagName("Name")[0].InnerText;

            // Create the AssetFile
            Console.WriteLine("=== Create AssetFile ===");
            // Find size of the actual file
            var fs = new FileStream(singleFilePath, FileMode.Open);
            long length = fs.Length;
            fs.Close();

            XmlDocument assetFileXmlResponse = CreateAssetFile(videoFileName + ".mp4", assetId, length);
            string assetFileId = assetXmlResponse.GetElementsByTagName("Id")[0].InnerText;
            string assetFileName = assetXmlResponse.GetElementsByTagName("Name")[0].InnerText;

            // Get the AssetFile's meta data
            Console.WriteLine("=== Get File Metadata ===");
            XmlDocument getFileXmlResponse = GenerateRequestAndGetResponse("GET", "Files()", "$filter=Id eq '" + assetFileId + "'&$top=1", null);

            // Create an AccessPolicy, this will be used to create a locator to upload the file
            Console.WriteLine("=== Create AccessPolicy ===");
            XmlDocument accessPolicyXmlResponse = CreateAccessPolicy(40, 2, videoFileName + "_AccessPolicy");
            string accessPolicyId = accessPolicyXmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Create a locator, the start time is set to 5 minutes before current according to the doc recommendation
            Console.WriteLine("=== Create Locator ===");
            XmlDocument locatorXmlResponse = CreateLocator(assetId, accessPolicyId, DateTime.UtcNow - TimeSpan.FromMinutes(5.0), 1);
            string locatorId = locatorXmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Upload the file to Azure Storage
            Console.WriteLine("=== Upload Asset Files ===");
            UploadBlobFileToSASContainer(locatorId, _singleInputMp4Path);

            // Delete the locator
            Console.WriteLine("=== Delete Locator ===");
            DeleteEntity("Locators('" + locatorId + "')");

            return assetId;
        }

        static public string CreateAssetAndUploadMultipleFiles(string mediaFilesPath)
        {
            var filePaths = Directory.EnumerateFiles(mediaFilesPath);

            // Create the Asset
            Console.WriteLine("=== Create Asset ===");
            XmlDocument assetXmlResponse = CreateEmptyAsset("MultiFile_Asset");
            string assetId = assetXmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Create an AssetFile for each file in the media files path
             Console.WriteLine("=== Create AssetFiles ===");
             foreach (var filePath in filePaths)
             {
                 var fs = new FileStream(filePath, FileMode.Open);
                 long length = fs.Length;
                 fs.Close();

                 XmlDocument assetFileXmlResponse = CreateAssetFile(Path.GetFileName(filePath), assetId, length);
                 string assetFileId = assetXmlResponse.GetElementsByTagName("Id")[0].InnerText;

                 // Get the file metadata
                 Console.WriteLine("Get File Metadata for: " + filePath);
                 XmlDocument getFileXmlResponse = GenerateRequestAndGetResponse("GET", "Files()", "$filter=Id eq '" + assetFileId + "'&$top=1", null);

                 // Create an AccessPolicy which will be used to create a locator. The locator is used to upload the files
                 Console.WriteLine("=== Create AccessPolicy ===");
                 XmlDocument accessPolicyXmlResponse = CreateAccessPolicy(40, 2, Path.GetFileNameWithoutExtension(filePath) + "_AccessPolicy");
                 string accessPolicyId = accessPolicyXmlResponse.GetElementsByTagName("Id")[0].InnerText;

                 // Create the locator, the start time is set to 5 minutes before current time according to the doc recommendation
                 Console.WriteLine("=== Create Locator ===");
                 XmlDocument locatorXmlResponse = CreateLocator(assetId, accessPolicyId, DateTime.UtcNow - TimeSpan.FromMinutes(5.0), 1);
                 string locatorId = locatorXmlResponse.GetElementsByTagName("Id")[0].InnerText;

                 // Upload the files to Azure Storage
                 Console.WriteLine("=== Upload Asset Files ===");
                 UploadBlobFileToSASContainer(locatorId, filePath);

                 // Delete the Locator
                 Console.WriteLine("=== Delete Locator ===");
                 DeleteEntity("Locators('" + locatorId + "')");
             }

            return assetId;
        }

        private static XmlDocument CreateAssetFile(string name, string parentAssetId, long length)
        {
            XmlDocument assetXmlResponse = null;

            // Generate an HTTP POST request to create an AssetFile
            string requestbody = "{ \"Name\" : \"" + name + "\", " + "\"ContentFileSize\" : \"" + length + "\", \"ParentAssetId\" : \"" + parentAssetId + "\" }";               
            assetXmlResponse = GenerateRequestAndGetResponse("POST", "Files", null, requestbody);

            Console.WriteLine("\nAssetFile Id: {0}", assetXmlResponse.GetElementsByTagName("Id")[0].InnerText);
            Console.WriteLine("Name: {0}", assetXmlResponse.GetElementsByTagName("Name")[0].InnerText);

            return assetXmlResponse;
        }
        #endregion

        #region Samples from Process Assets with REST API

        private static XmlDocument CreateEncodingJob(string token, string inputMedia)
        {
            XmlDocument xmlResponse = null;

            // Create an Asset and upload a single file
            string assetId = CreateAssetAndUploadSingleFile(token, inputMedia);
            XmlDocument xmlInputAsset = GetEntity("Assets('" + assetId + "')");
            if (xmlInputAsset == null)
                return null;

            // Get an instance of the Windows Azure Media Encoder
            string encoderId = GetLatestMediaProcessorId("Windows Azure Media Encoder");

            if (encoderId == "")
                return null;

            // Generate the HTTP POST request that specifies the encoding Job, it's tasks and input & output Assets
            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + xmlInputAsset.GetElementsByTagName("uri")[0].InnerText + "\"}}],"
                + "\"Tasks\" : [" + "{\"Configuration\" : \"H264 Adaptive Bitrate MP4 Set 720p\", \"MediaProcessorId\" : \"" + encoderId + "\", " +
                "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"}]}";

            xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);
            string jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;
            
            // Call helper method to display job details
            LogJobDetails(jobId);

            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");
            
                Console.WriteLine("outputAsset: " +  xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }
          
            return xmlResponse;
        }

        private static XmlDocument CreateChainedTaskEncodingJob(string token, string inputMediaFilePath)
        {
            XmlDocument xmlResponse = null;

            // Create an Asset and upload a single file
            string assetId = CreateAssetAndUploadSingleFile(token, inputMediaFilePath);
            XmlDocument xmlInputAsset = GetEntity("Assets('" + assetId + "')");
            if (xmlInputAsset == null)
                return null;

            // Get an instance of the Windows Media Encoder
            string encoderId = GetLatestMediaProcessorId("Windows Azure Media Encoder");

            // Get an instance of the Windows Media Storage Decryptor
            string decryptorId = GetLatestMediaProcessorId("Storage Decryption");

            if (encoderId == "" || decryptorId == "")
                return null;

            // Create a chain Task Job. The first task encodes the input file to H264 Broadband 720p. The second task decrypts the output from the first task
            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + xmlInputAsset.GetElementsByTagName("uri")[0].InnerText + "\"}}],"
                + "\"Tasks\" : [" + "{\"Configuration\" : \"H264 Broadband 720p\", \"MediaProcessorId\" : \"" + encoderId + "\", "
                + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"},"
                + "{\"Configuration\" : \"\", \"MediaProcessorId\" : \"" + decryptorId + "\", "
                + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobOutputAsset(0)</inputAsset><outputAsset>JobOutputAsset(1)</outputAsset></taskBody>\"}]}";

            xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);

            string jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Display job details
            LogJobDetails(jobId);
            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                // Display information about the output asset
                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");

                Console.WriteLine("outputAsset: " + xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }
            return xmlResponse;
        }

        private static XmlDocument EncodeToSmoothStreaming(string token, string inputMediaFilePath)
        {
            // Create an Asset and upload a single file
            string assetId = CreateAssetAndUploadSingleFile(token, inputMediaFilePath);

            // Verify the asset was created 
            XmlDocument xmlInputAsset = GetEntity("Assets('" + assetId + "')");
            if (xmlInputAsset == null)
                return null;

            // Get an instance of the Windows Azure Media Encoder
            string encoderId = GetLatestMediaProcessorId("Windows Azure Media Encoder");

            if (encoderId == "")
                return null;

            // Create an encoding job to encode the input asset to Smooth Streaming
            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + xmlInputAsset.GetElementsByTagName("uri")[0].InnerText + "\"}}],"
                + "\"Tasks\" : [" + "{\"Configuration\" : \"H264 Smooth Streaming 720p\", \"MediaProcessorId\" : \"" + encoderId + "\", " +
                "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"}]}";

            XmlDocument xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);
            string jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Display job information
            LogJobDetails(jobId);
            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                // Display information about the output asset
                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");

                Console.WriteLine("outputAsset: " + xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }

            return xmlResponse;

        }
       
        private static XmlDocument EncodeToAdaptiveBitrateAndConvertToSmooth(string token, string inputMediaFilePath)
        {
            XmlDocument xmlResponse = null;

            // Create an asset and upload a single file
            string assetId = CreateAssetAndUploadSingleFile(token, inputMediaFilePath);

            // Verify the asset was created
            XmlDocument xmlInputAsset = GetAsset(assetId);
            if (xmlInputAsset == null)
                return null;

            // Get an instance of the Windows Media Encoder
            string encoderId = GetLatestMediaProcessorId("Windows Azure Media Encoder");

            // Get an instance of the Windows Media Packager
            string packagerId = GetLatestMediaProcessorId("Windows Azure Media Packager");

            if (encoderId == "" || packagerId == "")
                return null;

            // Create a job that encodes the input asset to a set of adaptive bitrate MP4 files
            // Convert the set of adaptive bitrate MP4 files to Smooth Streaming format
            // Windows Media Packager does not accept string presets, so load xml configuration
            string smoothConfig = "<taskDefinition xmlns=\\\"http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#\\\"><name>MP4 to Smooth Streams</name><id>5e1e1a1c-bba6-11df-8991-0019d1916af0</id><description xml:lang=\\\"en\\\">Converts MP4 files encoded with H.264 (AVC) video and AAC-LC audio codecs to Smooth Streams.</description><inputFolder /><properties namespace=\\\"http://schemas.microsoft.com/iis/media/V4/TM/MP4ToSmooth#\\\" prefix=\\\"mp4\\\"><property name=\\\"keepSourceNames\\\" required=\\\"false\\\" value=\\\"false\\\" helpText=\\\"This property tells the MP4 to Smooth task to keep the original file name rather than add the bitrate bitrate information.\\\" /></properties><taskCode><type>Microsoft.Web.Media.TransformManager.MP4toSmooth.MP4toSmooth_Task, Microsoft.Web.Media.TransformManager.MP4toSmooth, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type></taskCode></taskDefinition>";

            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + xmlInputAsset.GetElementsByTagName("uri")[0].InnerText + "\"}}],"
    + "\"Tasks\" : [" + "{\"Configuration\" : \"H264 Adaptive Bitrate MP4 Set 720p\", \"MediaProcessorId\" : \"" + encoderId + "\", "
    + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"},"
    + "{\"Configuration\" : \"" + smoothConfig + "\", \"MediaProcessorId\" : \"" + packagerId + "\", "
    + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobOutputAsset(0)</inputAsset><outputAsset>JobOutputAsset(1)</outputAsset></taskBody>\"}]}";

            xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);

            string jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Display Job information
            LogJobDetails(jobId);
            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                // Display information about the output asset
                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");

                Console.WriteLine("outputAsset: " + xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }
            return xmlResponse;

        }

        private static XmlDocument ConvertMultipleBitrateToSmoothStreaming(string token, string inputMediaFilePath)
        {
            XmlDocument xmlResponse = null;

            // Create and asset and upload multiple bitrate MP4 files
            string assetId = CreateAssetAndUploadMultipleFiles(inputMediaFilePath);
            XmlDocument xmlInputAsset = GetEntity("Assets('" + assetId + "')");
                     
            if (xmlInputAsset == null)
                return null;

            // Find the AssetFile that contains the .ism file
            XmlDocument result = GetEntity("Assets('" + assetId + "')/Files", "$filter=endswith(Name, 'ism')", null);
            string assetFileId = result.GetElementsByTagName("Id")[0].InnerText;

            // Set the .ism file to primary
            string assetFileRequestBody = "{\"IsPrimary\" : \"true\"}";
            XmlDocument  response = GenerateRequestAndGetResponse("Merge", "Files('" + assetFileId + "')", null, assetFileRequestBody);
                        
            // Get an instance of the Windows Azure Media Packager
            string packagerId = GetLatestMediaProcessorId("Windows Azure Media Packager");

            if (packagerId == "")
                return null;

            // Create a job to convert the input asset to Smooth Streaming format
            // Windows Media Packager does not accept string presets, so load xml configuration
            string smoothConfig = "<taskDefinition xmlns=\\\"http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#\\\"><name>MP4 to Smooth Streams</name><id>5e1e1a1c-bba6-11df-8991-0019d1916af0</id><description xml:lang=\\\"en\\\">Converts MP4 files encoded with H.264 (AVC) video and AAC-LC audio codecs to Smooth Streams.</description><inputFolder /><properties namespace=\\\"http://schemas.microsoft.com/iis/media/V4/TM/MP4ToSmooth#\\\" prefix=\\\"mp4\\\"><property name=\\\"keepSourceNames\\\" required=\\\"false\\\" value=\\\"false\\\" helpText=\\\"This property tells the MP4 to Smooth task to keep the original file name rather than add the bitrate bitrate information.\\\" /></properties><taskCode><type>Microsoft.Web.Media.TransformManager.MP4toSmooth.MP4toSmooth_Task, Microsoft.Web.Media.TransformManager.MP4toSmooth, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type></taskCode></taskDefinition>";

            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + xmlInputAsset.GetElementsByTagName("uri")[0].InnerText + "\"}}],"
    + "\"Tasks\" : [" + "{\"Configuration\" : \"" + smoothConfig + "\", \"MediaProcessorId\" : \"" + packagerId + "\", "
    + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"}]}";

            xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);

            string jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Display job information
            LogJobDetails(jobId);
            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");

                Console.WriteLine("outputAsset: " + xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }
            return xmlResponse;

        }

        // Encode an asset to Smooth Streaming and convert to HLS format
        private static XmlDocument ConvertToHls(string token, string inputMediaFilePath)
        {
            // Upload an asset and encode to Smooth Streaming
            XmlDocument xmlJob = EncodeToSmoothStreaming(token, inputMediaFilePath);

            // Get the output asset from the encoding job
            string jobId = xmlJob.GetElementsByTagName("Id")[0].InnerText;
            XmlDocument xmlOutputAssetFile = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");
            string outputAssetId = xmlOutputAssetFile.GetElementsByTagName("Id")[0].InnerText;
            string url = xmlOutputAssetFile.GetElementsByTagName("uri")[0].InnerText;

            // Find the AssetFile that contains the .ism file
            XmlDocument result = GetEntity("Assets('" + outputAssetId + "')/Files", "$filter=endswith(Name, 'ism')", null);
            string assetFileId = result.GetElementsByTagName("Id")[0].InnerText;

            // Set the .ism file to primary
            string assetFileRequestBody = "{\"IsPrimary\" : \"true\"}";
            XmlDocument response = GenerateRequestAndGetResponse("Merge", "Files('" + assetFileId + "')", null, assetFileRequestBody);

            // Get an instance of the Windows Media Packager
            string packagerId = GetLatestMediaProcessorId("Windows Azure Media Packager");

            if (packagerId == "")
                return null;

            // Create a job to convert the Smooth Streaming asset to HLS format
            // Windows Media Packager does not accept string presets, so load xml configuration
            string hlsConfig = "<taskDefinition xmlns=\\\"http://schemas.microsoft.com/iis/media/v4/TM/TaskDefinition#\\\"><name>MP4 to Smooth Streams</name><id>5e1e1a1c-bba6-11df-8991-0019d1916af0</id><description xml:lang=\\\"en\\\">Converts MP4 files encoded with H.264 (AVC) video and AAC-LC audio codecs to Smooth Streams.</description><inputFolder /><properties namespace=\\\"http://schemas.microsoft.com/iis/media/V4/TM/MP4ToSmooth#\\\" prefix=\\\"mp4\\\"><property name=\\\"keepSourceNames\\\" required=\\\"false\\\" value=\\\"false\\\" helpText=\\\"This property tells the MP4 to Smooth task to keep the original file name rather than add the bitrate bitrate information.\\\" /></properties><taskCode><type>Microsoft.Web.Media.TransformManager.MP4toSmooth.MP4toSmooth_Task, Microsoft.Web.Media.TransformManager.MP4toSmooth, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</type></taskCode></taskDefinition>";
            string requestBody = "{\"Name\" : \"MyRESTJob\", \"InputMediaAssets\" : [{\"__metadata\" : {\"uri\" : \"" + url + "\"}}],"
    + "\"Tasks\" : [" + "{\"Configuration\" : \"" + hlsConfig + "\", \"MediaProcessorId\" : \"" + packagerId + "\", "
    + "\"TaskBody\" : \"<?xml version=\\\"1.0\\\" encoding=\\\"utf-8\\\"?><taskBody><inputAsset>JobInputAsset(0)</inputAsset><outputAsset>JobOutputAsset(0)</outputAsset></taskBody>\"}]}";

            // Submit the HTTP POST request to create and run the conversion job
            XmlDocument xmlResponse = GenerateRequestAndGetResponse("POST", "Jobs", null, requestBody);

            jobId = xmlResponse.GetElementsByTagName("Id")[0].InnerText;

            // Display job information
            LogJobDetails(jobId);
            if (xmlResponse != null)
            {
                // Display job progress and wait until completion
                MonitorAndWaitOnJob(xmlResponse);

                // Display information about the output asset
                XmlDocument xmlOutputAssets = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");

                Console.WriteLine("outputAsset: " + xmlOutputAssets.GetElementsByTagName("Name")[0].InnerText);
                Console.WriteLine("     Id:" + xmlOutputAssets.GetElementsByTagName("Id")[0].InnerText);
                Console.WriteLine("     Options:" + xmlOutputAssets.GetElementsByTagName("Options")[0].InnerText);
                Console.WriteLine("     State:" + xmlOutputAssets.GetElementsByTagName("State")[0].InnerText);
            }
            return xmlResponse;

        }
        #endregion  

        #region Samples from Manage Assets with REST API

        // Gets the specified Asset
        static XmlDocument GetAsset(string assetId)
        {
            return GetEntity("Asset('" + assetId + "')");
        }

        // Gets the specified Job
        static XmlDocument GetJob(string jobId)
        {
            return GetEntity("Asset('" + jobId + "')");
        }

        // Displays a list of all assets associated with the current Windows Azure
        // Media Services account
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

            // Get a list of Assets including the Name, Id, and Files elements
            XmlDocument xmlEntities = GetEntity("Assets", "$select=Name,Id,Files");

            // Convert to XElement for ease of querying
            XElement xmlAssets = XElement.Parse(xmlEntities.InnerXml);

            // Get all item elements with their Name and Id values
            var assets = from item in xmlAssets.Descendants("item")
                       select new
                       {
                           Name = item.Element("Name").Value,
                           Id = item.Element("Id").Value
                           
                       };
            
            // Iterate through the assets
            foreach (var asset in assets)
            {
               // Display the collection of assets.
               builder.AppendLine("");
               builder.AppendLine("******ASSET******");
               builder.AppendLine("Asset ID: " + asset.Id);
               builder.AppendLine("Name: " + asset.Name);
               builder.AppendLine("==============");
               builder.AppendLine("******ASSET FILES******");

               XmlDocument xmlFiles = GetEntity("Assets('" + asset.Id + "')/Files");
               XElement xmlElementFiles = XElement.Parse(xmlFiles.InnerXml);

                // Query for files associated with the current asset
               var files = from file in xmlElementFiles.Descendants("item")
                           select new
                           {
                               Name = file.Element("Name").Value,
                               ContentFileSize = file.Element("ContentFileSize").Value
                           };

                foreach (var file in files)
               {
                   builder.AppendLine("Name: " + file.Name);
                   builder.AppendLine("Size: " + file.ContentFileSize);
                   builder.AppendLine("==============");
               }
            }

            // Display output in console.
            Console.Write(builder.ToString());
           
        }

        // Displays a list of all Jobs associated with the current
        // Windows Azure Media Services account
        static void ListJobs()
        {
            string waitMessage = "Building the list. This may take a few "
     + "seconds to a few minutes depending on how many jobs "
     + "you have."
     + Environment.NewLine + Environment.NewLine
     + "Please wait..."
     + Environment.NewLine;

            Console.Write(waitMessage);

            // Create a Stringbuilder to store the list that we build. 
            StringBuilder builder = new StringBuilder();

            // Get a list of Jobs including the Name, Id, and Files elements
            XmlDocument xmlEntities = GetEntity("Jobs","$select=Id, Name, Priority, State");

            // Convert to XElement for ease of querying
            XElement xmlJobs = XElement.Parse(xmlEntities.InnerXml);

            // Get all item elements with their Name and Id values
            var jobs = from item in xmlJobs.Descendants("item")
                         select new
                         {
                             Id = item.Element("Id").Value,
                             Name = item.Element("Name").Value,
                             Priority = item.Element("Priority").Value,
                             State = item.Element("State").Value
                         };

            // Iterate through the assets
            foreach (var job in jobs)
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

            // Gets the specified job
            XmlDocument xmlTasks = GetEntity("Jobs('" + job.Id + "')/Tasks", "$select=Id, Name, State, Configuration");
            XElement xmlElementTasks = XElement.Parse(xmlTasks.InnerXml);

                // Query for tasks associated with the current job
                var tasks = from task in xmlElementTasks.Descendants("item")
                            select new
                            {
                                Id = task.Element("Id").Value,
                                Name = task.Element("Name").Value,
                                Progress = task.Element("State").Value,
                                Configuration = task.Element("Configuration").Value
                                //ErrorDetails = task.Element("ErrorDetails").Value
                            };

                foreach (var task in tasks)
                {
                    builder.AppendLine("Task Id: " + task.Id);
                    builder.AppendLine("Name: " + task.Name);
                    builder.AppendLine("Progress: " + task.Progress);
                    builder.AppendLine("Configuration: " + task.Configuration);
                    builder.AppendLine("==============");
                }
                
                // Gets the input assets of the specified job
                XmlDocument xmlInputMediaAssets = GetEntity("Jobs('" + job.Id + "')/InputMediaAssets", "$select=Id, Name");

                // Convert to XElement for ease of querying
                XElement xmlElementInputAssets = XElement.Parse(xmlInputMediaAssets.InnerXml);

                //Query for input media assets
                var inputAssets = from inputAsset in xmlElementInputAssets.Descendants("item")
                                  select new
                                  {
                                      Id = inputAsset.Element("Id").Value,
                                      Name = inputAsset.Element("Id").Value
                                  };

                // For each job, display the list of input media assets.
                builder.AppendLine("******JOB INPUT MEDIA ASSETS*******");
                foreach (var inputAsset in inputAssets)
                {

                    if (inputAsset != null)
                    {
                        builder.AppendLine("Input Asset Id: " + inputAsset.Id);
                        builder.AppendLine("Name: " + inputAsset.Name);
                        builder.AppendLine("==============");
                    }

                }

                // Get the output assets of the specified job
                XmlDocument xmlOutputMediaAssets = GetEntity("Jobs('" + job.Id + "')/OutputMediaAssets", "$select=Id, Name");

                // Convert to XElement for ease of querying
                XElement xmlElementOutputAssets = XElement.Parse(xmlInputMediaAssets.InnerXml);

                //Query for output media assets
                var outputAssets = from outputAsset in xmlElementOutputAssets.Descendants("item")
                                  select new
                                  {
                                      Id = outputAsset.Element("Id").Value,
                                      Name = outputAsset.Element("Id").Value
                                  };

                // For each job, display the list of input media assets.
                builder.AppendLine("******JOB OUTPUT MEDIA ASSETS*******");
                foreach (var inputAsset in inputAssets)
                {

                    if (inputAsset != null)
                    {
                        builder.AppendLine("Input Asset Id: " + inputAsset.Id);
                        builder.AppendLine("Name: " + inputAsset.Name);
                        builder.AppendLine("==============");
                    }
                }
            }
            
            // Display output in console.
            Console.Write(builder.ToString());
           
        }

        // Display all locators associated with the current
        // Windows Azure Media Services account
        static void ListAllLocators()
        {
            // Get a list of Assets including the Name, Id, and Files elements
            XmlDocument xmlEntities = GetEntity("Locators", "$select=Id, AssetId, AccessPolicyId, ExpirationDateTime, Path");

            // Convert to XElement for ease of querying
            XElement xmlLocators = XElement.Parse(xmlEntities.InnerXml);

            // Get all item elements with their Name and Id values
            var locators = from item in xmlLocators.Descendants("item")
                         select new
                         {
                            Id = item.Element("Id").Value,
                            AssetId = item.Element("AssetId").Value,
                            AccessPolicyId = item.Element("AccessPolicyId").Value,
                            ExpirationDateTime = item.Element("ExpirationDateTime").Value,
                            Path = item.Element("Path").Value
                        };

            // Iterate through the Locators
            foreach (var locator in locators)
            {
                Console.WriteLine("***********");
                Console.WriteLine("Locator Id: " + locator.Id);
                Console.WriteLine("Locator asset Id: " + locator.AssetId);
                Console.WriteLine("Locator access policy Id: " + locator.AccessPolicyId);
                XmlDocument xmlPolicy = GetEntity("Locators('" + locator.Id + "')/AccessPolicy");
                string permissions = xmlPolicy.GetElementsByTagName("Permissions")[0].InnerText;
                Console.WriteLine("Access policy permissions: " + permissions);
                Console.WriteLine("Locator expiration: " + locator.ExpirationDateTime);
                // The locator path is the base or parent path (with included permissions) to access  
                // the media content of an asset. To create a full URL to a specific media file, take 
                // the locator path and then append a file name and info as needed.  
                Console.WriteLine("Locator base path: " + locator.Path);
                Console.WriteLine("");

            }

        }
        
        #endregion    

        #region Samples from Deliver Assets with REST API

        // Download the output asset of the specified job to the specified directory
        static void DownloadAssetToLocal(string token, string jobId, string outputFolder)
        {
            // Get the output asset from the specified job
            XmlDocument xmlJob = GetEntity("Jobs('" + jobId + "')/OutputMediaAssets");
            string assetId = xmlJob.GetElementsByTagName("Id")[0].InnerText;

            // Get the AssetFiles collection from the Asset
            XmlDocument xmlFiles = GetEntity("Assets('" + assetId + "')/Files");

            // Convert to XElement for ease of querying
            XElement xElementFiles = XElement.Parse(xmlFiles.InnerXml);

            // Create an AccessPolicy which will be used to create a Locator to access the files
            XmlDocument xmlAccessPolicy = CreateAccessPolicy(120, 1 /* read */, "File Download Policy");
            string accessPolicyId = xmlAccessPolicy.GetElementsByTagName("Id")[0].InnerText;

            // Create a locator to access the files
            XmlDocument xmlLocator = CreateLocator(assetId, accessPolicyId, DateTime.UtcNow - TimeSpan.FromMinutes(5.0), 1);
            string locatorId = xmlLocator.GetElementsByTagName("Id")[0].InnerText;
            string baseUri = xmlLocator.GetElementsByTagName("BaseUri")[0].InnerText;
            string sasSig = xmlLocator.GetElementsByTagName("ContentAccessComponent")[0].InnerText;

            // Query for the files associated with the current asset
            var files = from file in xElementFiles.Descendants("item")
                        select new
                        {
                            Id = file.Element("Id").Value,
                            Name = file.Element("Name").Value
                        };

            // Iterate through the files collection
            foreach (var file in files)
            {
                Console.WriteLine("Downloading " + file.Name);

                // Download each file in a new thread, see FileDownloadThread below
                FileDownloadThread downloadThread = new FileDownloadThread(assetId, file.Name, outputFolder, baseUri, sasSig);
                Thread t = new Thread(new ThreadStart(downloadThread.ThreadProc));
                t.Start();
              
            }

            DeleteEntity("AccessPolicies('" + accessPolicyId + "')");
            DeleteEntity("Locators('" + locatorId + "')");

        }

        // Build a list of SAS Locators for all files contained in the specified Asset
        static void BuildAndSaveAssetSasUrlList(string assetId)
        {
            // Get the specified Asset
            XmlDocument xmlAsset = GetAsset(assetId);

            // Create an Access Policy
            XmlDocument xmlAccessPolicy = CreateAccessPolicy(120, 1 /* read */, "File Download Policy");
            string accessPolicyId = xmlAccessPolicy.GetElementsByTagName("Id")[0].InnerText;

            // Create the Locater
            XmlDocument xmlLocator = CreateLocator(assetId, accessPolicyId, DateTime.UtcNow - TimeSpan.FromMinutes(5.0), 1);
            string locatorId = xmlLocator.GetElementsByTagName("Id")[0].InnerText;

            // Get a list of all AssetFiles contained in the Asset
            XmlDocument xmlFiles = GetEntity("Assets('" + assetId + "')/Files");
            XElement xElementFiles = XElement.Parse(xmlFiles.InnerXml);

            // Declare a list to contain all the SAS URLs.
            List<String> fileSasUrlList = new List<String>();

            string outFilePath = Path.GetFullPath(_outputFilesFolder + @"\" + "FileSasUrlList.txt");

            // Query for the files associated with the current Asset
            var files = from file in xElementFiles.Descendants("item")
                        select new
                        {
                            Id = file.Element("Id").Value,
                            Name = file.Element("Name").Value
                        };

            // Iterate through the collection of AssetFiles
            foreach (var file in files)
            {
                // Create a SAS URL for the current file
                string sasUrl = BuildFileSasUrl(file.Name, locatorId);
                fileSasUrlList.Add(sasUrl);

                Console.WriteLine(sasUrl);

                // Write the URL list to a local file. You can use the saved 
                // SAS URLs to browse directly to the files in the asset.
                WriteToFile(outFilePath, sasUrl);
            }

        }

        // Get a SAS URL for the specified file
        static string BuildFileSasUrl(string filename, string locatorId)
        {
            // Get the specified Locator
            XmlDocument xmlLocator = GetEntity("Locators('" + locatorId + "')");
            string baseUri = xmlLocator.GetElementsByTagName("BaseUri")[0].InnerText;
            string sasSig = xmlLocator.GetElementsByTagName("ContentAccessComponent")[0].InnerText;

            // Optional:  print the locator.Path to the asset, and 
            // the full SAS URL to the file
            Console.WriteLine("Locator baseUri: ");
            Console.WriteLine(baseUri);
            Console.WriteLine();

            // Create the full URL to the file by combining the baseURI of the locator
            // with the filename and the Shared Access Signature
            Console.WriteLine("Full URL to file: ");
            string fullUrl =  baseUri + "/" + filename + sasSig;
            Console.WriteLine(fullUrl);
            Console.WriteLine();
            return fullUrl;
            
        }

        // Write method output to the output files folder.
        static void WriteToFile(string outFilePath, string fileContent)
        {
            StreamWriter sr = File.CreateText(outFilePath);
            sr.WriteLine(fileContent);
            sr.Close();
        }

        #endregion

        #region Helper Methods

        // Generates an HTTP request to create an AccessPolicy
        private static XmlDocument CreateAccessPolicy(int duration, int rwperms, string name)
        {
          XmlDocument xmlResponse = null;
          string requestBody = "{\"Name\": \"" + name + "\", \"DurationInMinutes\" : \"" + duration + "\", \"Permissions\" : " + rwperms + "}";

          xmlResponse = GenerateRequestAndGetResponse("POST", "AccessPolicies", null, requestBody);

          Console.WriteLine("\nAccessPolicy Id: {0}", xmlResponse.GetElementsByTagName("Id")[0].InnerText);
          Console.WriteLine("Name: {0}", xmlResponse.GetElementsByTagName("Name")[0].InnerText);
          Console.WriteLine("Duration (in minutes): {0}", xmlResponse.GetElementsByTagName("DurationInMinutes")[0].InnerText);
          Console.WriteLine("Permissions: {0}", xmlResponse.GetElementsByTagName("Permissions")[0].InnerText);

          return xmlResponse;
        }

        // Generates an HTTP request to get a ACS token which will be used for all subsequent HTTP requests, the 
        // token is returned as a string
        private static string GetACSToken(string accessControlUri, string clientId, string clientSecret, string scope)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(accessControlUri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = true;
            string token = null;

            //Note: You need to insert your client Id and secret into this string in order for it to work.
            var requestBytes = Encoding.ASCII.GetBytes("grant_type=client_credentials&client_id=" + clientId + "&client_secret=" + HttpUtility.UrlEncode(clientSecret) + "&scope=urn%3a" + scope);
            request.ContentLength = requestBytes.Length;

            // Write the request data to the request stream
            var requestStream = request.GetRequestStream();
            requestStream.Write(requestBytes, 0, requestBytes.Length);
            requestStream.Close();

            // Get the response back from the server
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Get the response stream and read the response data
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader stream = new StreamReader(responseStream))
                    {
                        string responseString = stream.ReadToEnd();
                        var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(responseString), new XmlDictionaryReaderQuotas());

                        // Read through the response stream until we find the access token
                        while (reader.Read())
                        {
                            if ((reader.Name == "access_token") && (reader.NodeType == XmlNodeType.Element))
                            {
                                if (reader.Read())
                                {
                                    token = reader.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return token;
        }
       
        // Generate an HTTP request with the specified HTTP verb, resource path, query, and request body
        private static XmlDocument GenerateRequestAndGetResponse(string verb, string resourcePath, string query, string requestbody)
        {
            // Create the URI
            var uriBuilder = new UriBuilder(serviceURI);
            uriBuilder.Path += resourcePath;
            if (query != null)
            {
                uriBuilder.Query = query;
            }

            // Create the HTTP request object
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uriBuilder.Uri);
            request.Method = verb;
            request.AllowAutoRedirect = false;

            //=====================================================================================
            //=== http://www.odata.org/documentation/operations#Retrievingthemetadatadocument   ===
            //=== Section 2.3 : Since only an XML serialization of EDM schemas exist currently, ===
            //=== no content-type negotiation is supported for this resource.                   ===
            //=====================================================================================
            if (resourcePath == "$metadata")
                request.MediaType = "application/xml";
            
            // Set the required header values
            request.ContentType = "application/json;odata=verbose";
            request.Accept = "application/json;odata=verbose";
            request.Headers.Add("Accept-Charset", "UTF-8");
            request.Headers.Add("DataServiceVersion", "3.0;NetFx");
            request.Headers.Add("MaxDataServiceVersion", "3.0;NetFx");
            request.Headers.Add("x-ms-version", "2.0");
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

            // If a request body was passed in, read it in and write it to the
            // request stream
            if (requestbody != null)
            {
                var requestBytes = Encoding.ASCII.GetBytes(requestbody);
                request.ContentLength = requestBytes.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
            }
            else
            {
                request.ContentLength = 0;
            }

            XmlDocument xmlResponse = null;

            try
            {
                // Generate HTTP request and pass in JSON defining the Asset's name.    
                var response = (HttpWebResponse)request.GetResponse();

                // Respond to HTTP response
                switch (response.StatusCode)
                {
                    // We recieved a redirect, send the request to the new URI
                    case HttpStatusCode.MovedPermanently:
                        serviceURI = new Uri(response.Headers["Location"]);
                        response.Close();
                        xmlResponse = GenerateRequestAndGetResponse(verb, resourcePath, query, requestbody);
                        break;
                    // An entity was created or the operation succeeded, read in the response
                    case HttpStatusCode.Created:
                    case HttpStatusCode.OK:
                        // Get the response stream
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            // Read the response stream
                            using (StreamReader stream = new StreamReader(responseStream))
                            {
                                string responseString = stream.ReadToEnd();

                                // If metadata was requested read in the XML from the response
                                if (resourcePath == "$metadata")
                                {
                                    //request.MediaType = "application/xml" for $metadata requests.
                                    xmlResponse = new XmlDocument();
                                    xmlResponse.LoadXml(responseString);
                                }
                                else
                                {
                                    // The response is in JSON so create a JSON reader to read it 
                                    // and convert it to XML for further processing
                                    var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(responseString), new XmlDictionaryReaderQuotas());
                                    xmlResponse = new XmlDocument();

                                    reader.Read();
                                    xmlResponse.LoadXml(reader.ReadInnerXml());
                                }
                            }
                        }
                        response.Close();
                        break;

                    // We received a timeout, display an error message
                    case HttpStatusCode.RequestTimeout:
                        TimeOuts++;
                        response.Close();
                        if (TimeOuts < 10)
                        {
                            Console.WriteLine("Timed out. Retrying in 10 sec...");
                            System.Threading.Thread.Sleep(10000);
                            xmlResponse = GenerateRequestAndGetResponse(verb, resourcePath, query, requestbody);
                            TimeOuts = 0;
                        }
                        break;

                    // Something else happend write out the HTTP status
                    default:
                        Console.WriteLine("HTTP Status ({0}) : {1}", response.StatusCode, response.StatusDescription);
                        response.Close();
                        break;
                }

            }

            // A WebException was thrown, display an error message
            catch (WebException webEx)
            {
                if (webEx.Status == WebExceptionStatus.Timeout)
                {
                    // Retry 10 times
                    TimeOuts++;
                    if (TimeOuts < 10)
                    {
                        Console.WriteLine("Timed out. Retrying in 10 sec...");
                        System.Threading.Thread.Sleep(10000);

                        // If this call succeeds, reset TimeOuts to 0, otherwise
                        // another timeout exception will be thrown (and TimeOuts won't be reset to 0)
                        xmlResponse = GenerateRequestAndGetResponse(verb, resourcePath, query, requestbody);
                        TimeOuts = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return xmlResponse;
        } 

        // Create a Locator for the specified asset, with the specified AccessPolicy, start time, and access type
        private static XmlDocument CreateLocator(string assetId, string accessPolicyId, DateTime startTime, int accessType)
        {
          XmlDocument xmlResponse = null;
          string requestBody = "{\"AccessPolicyId\": \"" + accessPolicyId + "\", \"AssetId\" : \"" + assetId + 
                                 "\", \"StartTime\" : \"" + startTime + "\", \"Type\":" + accessType + "}";

          // Generate HTTP request to create the locator
          xmlResponse = GenerateRequestAndGetResponse("POST", "Locators", null, requestBody);

            return xmlResponse;
        }

        // Get the specified Entity
        private static XmlDocument GetEntity(string resourcePath, string query = null, string requestBody = null)
        {
          XmlDocument xmlResponse = null;
          
          // Generate HTTP request to get the specified Entity
          xmlResponse = GenerateRequestAndGetResponse("GET", resourcePath, query, requestBody);

          return xmlResponse;
        }

        // Delete the specified Entity
        private static XmlDocument DeleteEntity(string resourcePath, string query = null, string requestBody = null)
        {
          XmlDocument xmlResponse = null;

          // Generate HTTP request to delete the specified Entity
          xmlResponse = GenerateRequestAndGetResponse("DELETE", resourcePath, query, requestBody);

          return xmlResponse;
        }

        // Upload a file using the specified locator
        static void UploadBlobFileToSASContainer(string locatorId, string filename)
        {

          // Get the locator
          XmlDocument xmlLocator = GetEntity("Locators('" + locatorId + "')");
          
          // Verify the specified locator exists
          if (xmlLocator == null)
            return;

          // Get the base filename (the fully qualified name was passed in)
          string[] toks = filename.Split('\\');
          string blobFilename = toks[toks.Length-1];

          // Get the fully qualified URI for the file by combining the base URI,
          // the file name, and the Shared Access Signature
          string uploadURI = xmlLocator.GetElementsByTagName("BaseUri")[0].InnerText + "/" + 
                             blobFilename + xmlLocator.GetElementsByTagName("ContentAccessComponent")[0].InnerText;

          Console.WriteLine("\nUpload Uri : {0}\n", uploadURI);

          // Create the HTTP PUT request to upload the file
          HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(uploadURI));
          request.Method = "PUT";
          request.ContentType = "application/octet-stream";
          request.Headers.Add("x-ms-version", "2011-08-18");
          request.Headers.Add("x-ms-date", "2011-01-17");
          request.Headers.Add("x-ms-blob-type", "BlockBlob");

          // Read the file into a stream
          var fs = new FileStream(filename, FileMode.Open);
          var buffer = new byte[fs.Length];
          fs.Read(buffer, 0, (int)fs.Length);
          fs.Flush();
          fs.Close();

          request.ContentLength = buffer.Length;

          // Write the stream to the request stream
          var requestStream = request.GetRequestStream();
          requestStream.Write(buffer, 0, buffer.Length);
          requestStream.Close();

          // Get the response from the server
          try
          {
            var response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine("Upload Response : " + response.StatusCode + "-" + response.StatusDescription);
          }
          catch(Exception ex)
          {
            Console.WriteLine("Upload Exception : " + ex.Message);
          }                                                     
        }

        // Get the latest media processor ID by name
        private static string GetLatestMediaProcessorId(string mediaProcessorName)
        {
            XmlDocument xml = GetEntity("MediaProcessors","$filter=Name eq '" + mediaProcessorName + "'");
            return xml.GetElementsByTagName("Id")[0].InnerText;
        }
       
        // Displays job status information and waits until completion
        private static int MonitorAndWaitOnJob(XmlDocument xmlJobDoc)
        {
          string jobId = xmlJobDoc.GetElementsByTagName("Id")[0].InnerText;
          string jobState = "";

          bool bWaitForJob = true;
          while (bWaitForJob)
          {
            System.Threading.Thread.Sleep(2000);
            XmlDocument xmlResponse = GetEntity("Jobs('" + jobId + "')");
            jobState = xmlResponse.GetElementsByTagName("State")[0].InnerText;

            switch (jobState)
            {
              case "0":
                Console.WriteLine("Job State : Queued. Waiting...");
                break;

              case "1":
                Console.WriteLine("Job State : Scheduled. Waiting...");
                break;

              case "2":
                Console.WriteLine("Job State : Processing. Waiting...");
                break;

              case "3":
                Console.WriteLine("Job State : Finished");
                bWaitForJob = false;
                break;

              case "4":
                Console.WriteLine("Job State : Error");
                bWaitForJob = false;
                break;

              case "5":
                Console.WriteLine("Job State : Canceled");
                bWaitForJob = false;
                break;

              case "6":
                Console.WriteLine("Job State : Canceling");
                bWaitForJob = false;
                break;
            }
          }

          return System.Convert.ToInt32(jobState);
        }

        // Display information about the specified job
        private static void LogJobDetails(string jobId)
        {
            StringBuilder builder = new StringBuilder();
            XmlDocument xmlJob = GetEntity("Jobs('" + jobId + "')");

            builder.AppendLine("\nJob ID: " + xmlJob.GetElementsByTagName("Id")[0].InnerText);
            builder.AppendLine("Job Name: " + xmlJob.GetElementsByTagName("Name")[0].InnerText);
            builder.AppendLine("Job submitted (client UTC time): " + DateTime.UtcNow.ToString());
            builder.AppendLine("Media Services account name: " + clientId);

            Console.Write(builder.ToString());
        }
 #endregion
      
    }

    // A class used to hold state for downloading files within threads
    class FileDownloadThread 
    {
        string m_assetId; 
        string m_fileName;
        string m_outputFolder;
        string m_baseUri;
        string m_sasSig;

        // Save state
        public FileDownloadThread(string assetId, string filename, string outputFolder, string baseUri, string sasSig)
        {
            m_assetId = assetId;
            m_fileName = filename;
            m_outputFolder = outputFolder;
            m_baseUri = baseUri;
            m_sasSig = sasSig;
        }

        // Downloads a file from Azure Storage
        // This method is called on a separate thread, one for each file to download
        public void ThreadProc()
        {
            // Create the fully qualified URL for the file
            string url = m_baseUri + "/" + m_fileName + m_sasSig;

            // Submit the HTTP GET request for the specified file
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.Method = "GET";
            request.Headers.Add("x-ms-version", "2009-09-19");
            request.UserAgent = "WA-Storage/6.0.6002.18312";

            try
            {
                // Get the response stream
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                // Createa a binary reader on the stream
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                BinaryReader reader = new BinaryReader(responseStream, encode);

                byte[] buffer = new byte[1024];

                // read in the file from storage and write it to the output file.
                var fs = new FileStream(m_outputFolder + "\\" + m_fileName, FileMode.Create);
                int count = reader.Read(buffer, 0, 1024);
                while (count > 0)
                {
                    fs.Write(buffer, 0, count);
                    count = reader.Read(buffer, 0, 1024);
                }

                fs.Flush();
                fs.Close();
                responseStream.Close();

                Console.WriteLine("Download Response : " + response.StatusCode + "-" + response.StatusDescription);              
            }
            catch (Exception ex)
            {
                Console.WriteLine("Upload Exception : " + ex.Message);

            }  

        }
    }
}
