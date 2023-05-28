using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AudioService.Controllers
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

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            //using var memoryStream = new MemoryStream();
            //await HttpContext.Request.Body.CopyToAsync(memoryStream);

            byte[] fileData = System.IO.File.ReadAllBytes(@"C:\Users\maxim\source\repos\ConversionApi\AudioService\test.mov");
            //byte[] fileData = System.IO.File.ReadAllBytes(@"C:\Users\maxim\Desktop\max\4.mp4");

            // Create a new MemoryStream with the file data
            MemoryStream memoryStream = new(fileData);
            var outputStream = new MemoryStream();

            ExtractAudio(memoryStream, outputStream);
            Console.WriteLine(outputStream.Capacity.ToString());

            byte[] data = outputStream.ToArray();
            System.IO.File.WriteAllBytes("myfile.mp3", data);
            return Ok();
        }

        // this took way too many fucking hours to figure out...
        private async void ExtractAudio(Stream input, Stream output)
        {
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
                input.CopyTo(stdin);
                stdin.Close();
            }).Start();

            new Thread(_ =>
            {
                process.StandardOutput.BaseStream.CopyTo(output);
            }).Start();

            process.WaitForExit();
        }
    }
}