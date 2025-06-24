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
                
                // Get the current request URL and replace the default API URL in the HTML
                string requestUrl = $"{Request.Scheme}://{Request.Host}";
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
            return await _classificationService.ClassifyImageAsync(imageFile);
        }
    }
} 