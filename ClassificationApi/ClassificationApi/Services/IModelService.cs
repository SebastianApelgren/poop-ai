using Microsoft.ML.OnnxRuntime;

namespace ClassificationApi.Services
{
    public interface IModelService
    {
        Task<InferenceSession> GetModelAsync();
        bool IsModelLoaded { get; }
    }
} 