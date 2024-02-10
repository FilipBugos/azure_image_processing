using Microsoft.AspNetCore.Mvc;

namespace space
{
    [ApiController]
    [Route("[controller]")]
    public class ImageProcessingController : ControllerBase
    {
        private readonly ILogger<ImageProcessingController> _logger;
        private readonly IAzureServiceBusQueue _serviceBusQueue;
        private readonly IAzureKeyVaultService _azureKeyVaultService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;

        public ImageProcessingController(ILogger<ImageProcessingController> logger,
        IAzureKeyVaultService azureKeyVaultService, IAzureBlobStorageService azureBlobStorageService,
        IAzureServiceBusQueue serviceBusQueue)
        {
            _logger = logger;
            _azureKeyVaultService = azureKeyVaultService;
            _azureBlobStorageService = azureBlobStorageService;
            _serviceBusQueue = serviceBusQueue;
        }

        [HttpPost(Name = "sendimage")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null) {
                return BadRequest("No file received from the upload");
            }
        
            try
            {
                var imageId = await UploadImage(file);
                if (imageId == null) {
                    return Problem("File is not valid");
                }

                await SendMessage(imageId);
                
                return Ok(imageId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<string?> UploadImage(IFormFile imageStream) 
        {
            var blobStorageConnectionString = _azureKeyVaultService.GetKeyVaultSecret("secret", "https://kv-image-processing.vault.azure.net/");

            if (String.IsNullOrEmpty(blobStorageConnectionString)) {
                _logger.LogError("Missing connection string for azure storage");
                return null;
            }

            var uploadedPhotoId = "";
            
            if (StorageHelper.ValidateFile(imageStream))
            {
                using (Stream stream = imageStream.OpenReadStream())
                {
                    uploadedPhotoId = await _azureBlobStorageService.UploadFileToStorage(stream, blobStorageConnectionString, "scuserfile");
                }
            }
            
            return uploadedPhotoId;
        } 

        private async Task SendMessage(string imageId) {
            var serviceBusConnectionString = _azureKeyVaultService.GetKeyVaultSecret("serviceBusSecret", "https://kv-image-processing.vault.azure.net/");
            await _serviceBusQueue.SendMessage(imageId, serviceBusConnectionString, "ServiceBusQueue");                
        }

        [HttpGet("receiveimage")]
        public IActionResult GetThumbNails(String photoId)
        {
            try
            {
                var secretValue = _azureKeyVaultService.GetKeyVaultSecret("secret", "https://kv-image-processing.vault.azure.net/");
                var data = _azureBlobStorageService.GetProcessedImage(photoId, secretValue, "scuserfile");
                return Ok(data);            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    
}