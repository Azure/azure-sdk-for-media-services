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

namespace CopyBlobsIntoAnAsset
{
    class Program
    {
        // Make sure to add the appropriate values to the App.config file.
        private static readonly string MediaServicesAccountName = ConfigurationManager.AppSettings["MediaServicesAccountName"];
        private static readonly string MediaServicesAccountKey = ConfigurationManager.AppSettings["MediaServicesAccountKey"];
        private static readonly string MediaServicesStorageAccountName = ConfigurationManager.AppSettings["MediaServicesStorageAccountName"];
        private static readonly string MediaServicesStorageAccountKey = ConfigurationManager.AppSettings["MediaServicesStorageAccountKey"];
        private static readonly string ExternalStorageAccountName = ConfigurationManager.AppSettings["ExternalStorageAccountName"];
        private static readonly string ExternalStorageAccountKey = ConfigurationManager.AppSettings["ExternalStorageAccounKey"];


        static void Main(string[] args)
        {

            CopyBlobToAssetContainerInTheSameMediaServicesAccount();
            CopyBlobToAssetContainerNotInTheSameMediaServicesAccount();
        }

        static public void CopyBlobToAssetContainerInTheSameMediaServicesAccount()
        {
            CloudMediaContext context = new CloudMediaContext(MediaServicesAccountName, MediaServicesAccountKey);

            var storageAccount = new CloudStorageAccount(new StorageCredentials(MediaServicesStorageAccountName, MediaServicesStorageAccountKey), true);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var mediaBlobContainer = cloudBlobClient.GetContainerReference(cloudBlobClient.BaseUri + "mediafiles001");

            mediaBlobContainer.CreateIfNotExists();

            string StreamingFilesFolder = Path.GetFullPath(@"../..\SupportFiles\streamingfiles\");

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

        static public void CopyBlobToAssetContainerNotInTheSameMediaServicesAccount()
        {

            // Create Media Services context.
            CloudMediaContext context = new CloudMediaContext(MediaServicesAccountName, MediaServicesAccountKey);

            var externalStorageAccount = new CloudStorageAccount(new StorageCredentials(ExternalStorageAccountName, ExternalStorageAccountKey), true);
            var externalCloudBlobClient = externalStorageAccount.CreateCloudBlobClient();
            var externalMediaBlobContainer = externalCloudBlobClient.GetContainerReference(externalCloudBlobClient.BaseUri + "mediafiles002");

            externalMediaBlobContainer.CreateIfNotExists();

            string StreamingFilesFolder = Path.GetFullPath(@"../..\SupportFiles\streamingfiles\");

            // Upload some files to the blob container (for testing purposes). 
            DirectoryInfo uploadDirectory = new DirectoryInfo(StreamingFilesFolder);
            foreach (var file in uploadDirectory.EnumerateFiles())
            {
                CloudBlockBlob blob = externalMediaBlobContainer.GetBlockBlobReference(file.Name);
                var name = file.Name;
                using (var stream = System.IO.File.OpenRead(StreamingFilesFolder + name))
                    blob.UploadFromStream(stream);
            }

            // Get the SAS token to use for all blobs if dealing with multiple accounts
            string blobToken = externalMediaBlobContainer.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                // Specify the expiration time for the signature.
                SharedAccessExpiryTime = DateTime.Now.AddMinutes(30),
                // Specify the permissions granted by the signature.
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read
            });


            // Create a new asset.
            IAsset asset = context.Assets.Create("NewAsset_" + Guid.NewGuid(), AssetCreationOptions.None);
            IAccessPolicy writePolicy = context.AccessPolicies.Create("writePolicy",
                TimeSpan.FromMinutes(120), AccessPermissions.Write);
            ILocator destinationLocator = context.Locators.CreateLocator(LocatorType.Sas, asset, writePolicy);



            var destinationStorageAccount = new CloudStorageAccount(new StorageCredentials(MediaServicesStorageAccountName, MediaServicesStorageAccountKey), true);
            var destBlobStorage = destinationStorageAccount.CreateCloudBlobClient();

            // Get the asset container URI and Blob copy from mediaContainer to assetContainer.
            string destinationContainerName = (new Uri(destinationLocator.Path)).Segments[1];

            CloudBlobContainer assetContainer =
                destBlobStorage.GetContainerReference(destinationContainerName);



            foreach (var sourceBlob in externalMediaBlobContainer.ListBlobs())
            {
                string fileName = HttpUtility.UrlDecode(Path.GetFileName(sourceBlob.Uri.AbsoluteUri));

                var sourceCloudBlob = externalMediaBlobContainer.GetBlockBlobReference(fileName);
                sourceCloudBlob.FetchAttributes();

                if (sourceCloudBlob.Properties.Length > 0)
                {
                    assetContainer.CreateIfNotExists();
                    var destinationBlob = assetContainer.GetBlockBlobReference(fileName);

                    destinationBlob.StartCopyFromBlob(new Uri(sourceBlob.Uri.AbsoluteUri + blobToken));

                    var assetFile = asset.AssetFiles.Create(fileName);
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
    }
}
