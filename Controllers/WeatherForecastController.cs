using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi;

namespace space
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
         "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ImageController> _logger;
        private readonly IConfiguration _config;
        private readonly AzureStorageConfig _configBlob; 
        private readonly string _blobConfigAccountKey;
        private readonly string _uploadCon;
        private readonly string _receiveCon;
        private readonly ServiceBusService _serviceBus;


        public ImageController(ILogger<ImageController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _configBlob = new AzureStorageConfig()
            {
                AccountKey = _config.GetValue<String>("AzureStorageConfig:AccountKey"),
                AccountName = _config.GetValue<String>("AzureStorageConfig:AccountName"),
                ImageContainer = _config.GetValue<String>("AzureStorageConfig:ImageContainer"),
                ThumbnailContainer = _config.GetValue<String>("AzureStorageConfig:ThumbnailContainer")
            };

            _serviceBus = new ServiceBusService(_config);
        }

       /* [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult> Get()
        {
            var serviceBus = new ServiceBusService(_config);
            await serviceBus.method();
            return Ok();
        }*/

        [HttpPost(Name = "sendimage")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };
            var client = new SecretClient(new Uri("https://kv-image-processing.vault.azure.net/"), new DefaultAzureCredential(),options);

            KeyVaultSecret secret = client.GetSecret("secret");

            string secretValue = secret.Value;
            _logger.LogInformation("Value: ", secretValue);

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (_configBlob.AccountKey == string.Empty || _configBlob.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (_configBlob.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                var uploadedPhotoId = "";
                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                uploadedPhotoId = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, _configBlob);
                                await _serviceBus.method(uploadedPhotoId);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (uploadedPhotoId == "")
                {
                    return StatusCode(500);
                }
                //if (isUploaded)
                //{
                  //  if (_configBlob.ThumbnailContainer != string.Empty)
                return Ok(uploadedPhotoId);
               //     else
                 //       return new AcceptedResult();
                //}
                //lse
                   // return BadRequest("Look like the image couldnt upload to the storage");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("signin-oidc")]
        public void SignIn(String photoId)
        {
            Console.WriteLine("Signed in");
        }

        [HttpGet("receiveimage")]
        public async Task<IActionResult> GetThumbNails(String photoId)
        {
            try
            {
                if (_configBlob.AccountKey == string.Empty || _configBlob.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (_configBlob.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                var data = await StorageHelper.GetThumbNailUrls(photoId, _configBlob);
                return Ok(data);            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    
}