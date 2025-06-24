using ClassificationApi.Models;
using Microsoft.AspNetCore.Http;

namespace ClassificationApi.Services
{
    public interface IStoolClassificationService
    {
        Task<ClassificationResponse> ClassifyImageAsync(IFormFile imageFile);
    }
} 