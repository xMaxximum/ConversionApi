using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace AudioService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AudioController> _logger;

        public AudioController(ILogger<AudioController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("getaudio")]
        public async Task<IActionResult> GetAudio([FromForm] FileUploadModel model)
        {
            using var form = new MultipartFormDataContent
            {
                // Add the file to the form data
                {
                    new StreamContent(model.File.OpenReadStream())
                    {
                        Headers =
                {
                    ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "file",
                        FileName = model.File.FileName
                    }
                }
                    },
                    "file",
                    model.File.FileName
                }
            };

            var client = _httpClientFactory.CreateClient("audio");
            var response = await client.PostAsync("http://audioservice:80/Audio/getaudio", form);

            // Get the content type and file name from the response headers
            var contentType = response.Content.Headers.ContentType!.ToString();

            // Read the response stream to a byte array
            var content = await response.Content.ReadAsByteArrayAsync();

            // Return the file as a response
            return File(content, contentType);
        }

        public class FileUploadModel
        {
            [FromForm(Name = "file")]
            public IFormFile File { get; set; }
        }
    }
}