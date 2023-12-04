using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi;

namespace space
{
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<String> UploadFileToStorage(Stream fileStream,
                                                            string connectionString
                                                            )
        {
            var filename2 = Guid.NewGuid().ToString();
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to a container
            var blobContainerClient = blobServiceClient.GetBlobContainerClient("scuserfile");
            await blobContainerClient.CreateIfNotExistsAsync();

            // Get a reference to a blob
            var blobClient = blobContainerClient.GetBlobClient(filename2 + ".png");

            // Upload the image file to the blob
            using (fileStream)
            {
                await blobClient.UploadAsync(fileStream, true);
            }

            return filename2;
        }

        public static async Task<string> GetThumbNailUrls(String id, AzureStorageConfig _storageConfig)
        {
            /*List<string> thumbnailUrls = new List<string>();

            // Create a URI to the storage account
            Uri accountUri = new Uri("https://" + _storageConfig.AccountName + ".blob.core.windows.net/");

            // Create BlobServiceClient from the account URI
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

            if (container.Exists())
            {
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    thumbnailUrls.Add(container.Uri + "/" + blobItem.Name);
                }
            }

            return await Task.FromResult(thumbnailUrls);*/
            id = id + "_result.jpg";
            Uri blobUri = new Uri("https://pa200hw04storageaccount.blob.core.windows.net/blobstoragecontainer/" + id);

            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential("pa200hw04storageaccount", "SU1ZgpCxb2Zz+BIoXIHEAJBANbfehFd7EXd0Z/dnZVlr1U9C2mo8ODqg/ldFq9dhLLUfXt2SyksZ+AStesxpWw==");

            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            var content = await blobClient.DownloadContentAsync();

            return blobUri.AbsoluteUri;
        }
    }
}
