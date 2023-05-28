using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AudioService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        private readonly ILogger<AudioController> _logger;

        public AudioController(ILogger<AudioController> logger)
        {
            _logger = logger;
        }

        [HttpPost("getaudio")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest("Please select a file to upload.");
            }

            using (var stream = model.File.OpenReadStream())
            {
                var output = new MemoryStream();

                var process = new Process
                {
                    StartInfo =
                {
                    FileName = "ffmpeg",
                    Arguments = "-i pipe:0 -f mp3 -codec:a libmp3lame -",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
                };

                process.Start();

                var stdin = process.StandardInput.BaseStream;

                new Thread(_ =>
                {
                    stream.CopyTo(stdin);
                    stdin.Close();
                }).Start();

                new Thread(_ =>
                {
                    process.StandardOutput.BaseStream.CopyTo(output);
                }).Start();

                process.WaitForExit();

                var data = output.ToArray();

                return File(data, "audio/mpeg");
            }
        }

        public class FileUploadModel
        {
            [FromForm(Name = "file")]
            public IFormFile File { get; set; }
        }
    }
}