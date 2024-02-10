using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FunctionApp1
{
    public static class Function1
    {
        // [FunctionName("Function1")]
        //  public static async Task RunAsync([ServiceBusTrigger("pa200-hw04-queu", Connection = "connectionString")]string myQueueItem, ILogger log)
        // {
        //     log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        //     log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        //     var body = myQueueItem;
        //     var unescapedbody = Regex.Unescape(body.TrimStart('"').TrimEnd('"'));
        //     try
        //     {
        //         var reciveMessage = JsonSerializer.Deserialize<ObjectRecieve>(unescapedbody);
        //         // get photo from blob storage + process imagee
        //         log.LogInformation($"Received message: {reciveMessage.MessageId} with id: {reciveMessage.PhotoId}");

        //         // get photo from blobt
        //         var a = 54;
        //         var content = await GetPhoto(reciveMessage.PhotoId);

        //         var blackAndWhite = ConvertToBlackAndWhite(content.ToArray());
        //         // upload it to blob storoagewwaa
        //         await uploadBlob(reciveMessage.PhotoId, ConvertToBinary(blackAndWhite));
        //     }
        //     catch (Exception ex)
        //     {
        //         log.LogInformation($"Error when deserializing message: {unescapedbody} with ex: {ex.Message}");
        //     }
        // }

        // public class ObjectRecieve
        // {
        //     public String MessageId { get; set; }
        //     public String PhotoId { get; set; }
        // }

        // public static async Task<BinaryData> GetPhoto(String id)
        // { 
        //     Uri blobUri = new Uri("https://pa200hw04storageaccount.blob.core.windows.net/blobstoragecontainer/" + id);

        //     StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential("pa200hw04storageaccount", "SU1ZgpCxb2Zz+BIoXIHEAJBANbfehFd7EXd0Z/dnZVlr1U9C2mo8ODqg/ldFq9dhLLUfXt2SyksZ+AStesxpWw==");

        //     BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

        //     var content = await blobClient.DownloadContentAsync();

        //     return content.Value.Content;

        // }

        // public static async Task uploadBlob(String photoId, byte[] data)
        // {

        //     // Use the 'stream' object here
        //     // For example, you can pass it to a method that expects a Stream parameter
        //     // or perform any other operations on the stream

        //     // Create a URI to the blob
        //     var filename2 = photoId + "_result.jpg";
        //     Uri blobUri = new Uri("https://" + "pa200hw04storageaccount" + ".blob.core.windows.net/" + "blobstoragecontainer" + "/" + filename2);

        //     // Create StorageSharedKeyCredentials object by reading
        //     // the values from the configuration (appsettings.json)
        //     StorageSharedKeyCredential storageCredentials =
        //         new StorageSharedKeyCredential("pa200hw04storageaccount", "SU1ZgpCxb2Zz+BIoXIHEAJBANbfehFd7EXd0Z/dnZVlr1U9C2mo8ODqg/ldFq9dhLLUfXt2SyksZ+AStesxpWw==");

        //     // Create the blob client.
        //     BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
        //     using (MemoryStream stream = new MemoryStream(data))
        //     {
        //         // Upload the file
        //         await blobClient.UploadAsync(stream);
        //     }
        // }

        // public static Bitmap ConvertToBlackAndWhite(byte[] binaryData)
        // {
        //     // Create a MemoryStream from the binary data
        //     using (MemoryStream stream = new MemoryStream(binaryData))
        //     {
        //         // Load the image from the MemoryStream
        //         using (Bitmap originalImage = new Bitmap(stream))
        //         {
        //             // Create a new bitmap with the same size as the original image
        //             Bitmap blackAndWhiteImage = new Bitmap(originalImage.Width, originalImage.Height);

        //             // Loop through each pixel in the original image
        //             for (int y = 0; y < originalImage.Height; y++)
        //             {
        //                 for (int x = 0; x < originalImage.Width; x++)
        //                 {
        //                     Color pixelColor = originalImage.GetPixel(x, y);

        //                     // Calculate the average intensity of the pixel's RGB values
        //                     int averageIntensity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

        //                     // Create a new grayscale color using the average intensity
        //                     Color newPixelColor = Color.FromArgb(averageIntensity, averageIntensity, averageIntensity);

        //                     // Set the pixel color in the black and white image
        //                     blackAndWhiteImage.SetPixel(x, y, newPixelColor);
        //                 }
        //             }

        //             return blackAndWhiteImage;
        //         }
        //     }
        // }

        // public static byte[] ConvertToBinary(Bitmap image)
        // {
        //     using (MemoryStream stream = new MemoryStream())
        //     {
        //         // Retrieve the binary data from the memory stream
        //         return stream.ToArray();
        //     }
        // }

    }
}
