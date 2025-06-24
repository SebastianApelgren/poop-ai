using ClassificationApi.Models;
using ClassificationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClassificationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClassificationController : ControllerBase
    {
        private readonly IStoolClassificationService _classificationService;

        public ClassificationController(IStoolClassificationService classificationService)
        {
            _classificationService = classificationService;
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