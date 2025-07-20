using ClassificationApi.Models;
using ClassificationApi.Resources;
using ClassificationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClassificationApi.Controllers
{
    [ApiController]
    [Route("")]
    public class ClassificationController : ControllerBase
    {
        private readonly IStoolClassificationService _classificationService;

        public ClassificationController(IStoolClassificationService classificationService)
        {
            _classificationService = classificationService;
        }

        [HttpGet]
        public IActionResult GetFrontend()
        {
            try
            {
                string htmlContent = ResourceHelper.Instance.ReadAsStringAsync(Resource.Frontend.StoolClassificationFrontend).Result;

                // Get the external URL from forwarded headers, but hardcode https
                string scheme = "https"; // Hardcoded to fix mixed content
                string host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.Value;
                string prefix = Request.Headers["X-Forwarded-Prefix"].FirstOrDefault() ?? "";

                string requestUrl = $"{scheme}://{host}{prefix}";
                htmlContent = htmlContent.Replace("http://localhost:5001", requestUrl);

                return Content(htmlContent, "text/html");
            }
            catch (Exception ex)
            {
                return NotFound($"Frontend not found: {ex.Message}");
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API is running", timestamp = DateTime.UtcNow });
        }

        [HttpPost("predict")]
        public async Task<ClassificationResponse> Predict(IFormFile imageFile)
        {
            // Validate input
            if (imageFile == null || imageFile.Length == 0)
            {
                return new ClassificationResponse
                {
                    Error = "No image file provided or file is empty."
                };
            }

            return await _classificationService.ClassifyImageAsync(imageFile.OpenReadStream());

        }
    }
}

