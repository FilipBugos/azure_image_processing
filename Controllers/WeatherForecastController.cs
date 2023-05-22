using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi;

namespace space
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _config;
        private readonly AzureStorageConfig _configBlob; 
        private readonly string _blobConfigAccountKey;
        private readonly string _uploadCon;
        private readonly string _receiveCon;
        private readonly ServiceBusService _serviceBus;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
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
            bool isUploaded = false;

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if (_configBlob.AccountKey == string.Empty || _configBlob.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (_configBlob.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                var uploadedPhotoId = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, _configBlob);
                                await _serviceBus.method(uploadedPhotoId);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                //if (isUploaded)
                //{
                  //  if (_configBlob.ThumbnailContainer != string.Empty)
                        return Ok();
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

        [HttpGet("receiveimage")]
        public async Task<IActionResult> GetThumbNails()
        {
            try
            {
                if (_configBlob.AccountKey == string.Empty || _configBlob.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.");

                if (_configBlob.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in Azure blob storage.");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(_configBlob);
                return new ObjectResult(thumbnailUrls);            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    
}