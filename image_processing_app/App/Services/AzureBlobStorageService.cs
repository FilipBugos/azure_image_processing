using Azure.Storage.Blobs;

namespace space
{
    public class AzureBlobStorageService : IAzureBlobStorageService {

        public AzureBlobStorageService() {

        }

        public async Task<String> UploadFileToStorage(Stream fileStream, string connectionString, string containerName)
        {
            var filename = Guid.NewGuid().ToString();
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to a container
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            // Get a reference to a blob
            // TODO: it should not be only .png file
            var blobClient = blobContainerClient.GetBlobClient(filename + ".png");

            // Upload the image file to the blob
            using (fileStream)
            {
                await blobClient.UploadAsync(fileStream, true);
            }

            return filename;
        }

        public String GetProcessedImage(String id, string connectionString, string containerName)
        {
            //id = id + "_result.jpg";
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(id);

            return blobClient.Uri.AbsoluteUri;
        }
    }
}