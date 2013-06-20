using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure;
using System.Web;

namespace CopyFromExistingBlobToAsset
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateAssetFromBlobsInDifferentStorageAccount();
        }


        public static void CreateAssetFromBlobsInDifferentStorageAccount()
        {
            // Read values from the App.config file.
            string accountName = ConfigurationManager.AppSettings["accountName"];
            string accountKey = ConfigurationManager.AppSettings["accountKey"];
            string storageAccountName = ConfigurationManager.AppSettings["MediaServicesStorageAccountName"];
            string storageAccountKey = ConfigurationManager.AppSettings["MediaServicesStorageAccountKey"];
            string externalStorageAccountName = ConfigurationManager.AppSettings["ExternalStorageAccountName"];
            string externalStorageAccountKey = ConfigurationManager.AppSettings["ExternalStorageAccountKey"];

            // Create Media Services context.
            CloudMediaContext context = new CloudMediaContext(accountName, accountKey); ;

            // Create a blob container in a storage account different from Media Services associated storage account.
            StorageCredentialsAccountAndKey externalStorageCredentials =
                new StorageCredentialsAccountAndKey(externalStorageAccountName, externalStorageAccountKey);
            CloudStorageAccount externalStorageAccount = new CloudStorageAccount(externalStorageCredentials, true);
            CloudBlobClient externalCloudBlobClient = externalStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer externalMediaBlobContainer =
                externalCloudBlobClient.GetContainerReference(externalCloudBlobClient.BaseUri + "mediafiles1");

            externalMediaBlobContainer.CreateIfNotExist();

            // Upload files to the blob container. 
            string localMediaDir = @"C:\supportFiles\streamingfiles";
            DirectoryInfo uploadDirectory = new DirectoryInfo(localMediaDir);
            foreach (var file in uploadDirectory.EnumerateFiles())
            {
                CloudBlockBlob blob = externalMediaBlobContainer.GetBlockBlobReference(file.Name);

                blob.UploadFile(file.FullName);
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

            // Create a blob container in a storage account associated with Media Services account. 
            StorageCredentialsAccountAndKey mediaServicesStorageCredentials = new StorageCredentialsAccountAndKey(storageAccountName, storageAccountKey);
            CloudStorageAccount destinationStorageAccount = new CloudStorageAccount(mediaServicesStorageCredentials, true);
            CloudBlobClient destBlobStorage = destinationStorageAccount.CreateCloudBlobClient();

            // Get the asset container URI and Blob copy from mediaContainer to assetContainer.
            string destinationContainerName = (new Uri(destinationLocator.Path)).Segments[1];

            CloudBlobContainer assetContainer =
                destBlobStorage.GetContainerReference(destinationContainerName);

            foreach (var sourceBlob in externalMediaBlobContainer.ListBlobs())
            {
                string fileName = HttpUtility.UrlDecode(Path.GetFileName(sourceBlob.Uri.AbsoluteUri));
                CloudBlob sourceCloudBlob = externalMediaBlobContainer.GetBlobReference(fileName);
                sourceCloudBlob.FetchAttributes();

                if (sourceCloudBlob.Properties.Length > 0)
                {
                    CloudBlob destinationBlob = assetContainer.GetBlockBlobReference(fileName);

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
