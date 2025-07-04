using ClassificationApi.Resources;
using Microsoft.ML.OnnxRuntime;

namespace ClassificationApi.Services
{
    public class ModelService : IModelService
    {
        private InferenceSession? _model;
        private readonly object _lockObject = new object();
        private bool _isLoading = false;

        public bool IsModelLoaded => _model != null;

        public async Task<InferenceSession> GetModelAsync()
        {
            if (_model != null)
                return _model;

            lock (_lockObject)
            {
                if (_model != null)
                    return _model;

                if (_isLoading)
                {
                    // Wait for the loading to complete
                    Monitor.Wait(_lockObject);
                    return _model!;
                }

                _isLoading = true;
            }

            try
            {
                await LoadModelAsync();
                return _model!;
            }
            finally
            {
                lock (_lockObject)
                {
                    _isLoading = false;
                    Monitor.PulseAll(_lockObject);
                }
            }
        }

        private async Task LoadModelAsync()
        {
            try
            {
                // Get the model resource
                Resource modelResource = Resource.Models.StoolModel;

                // Load the model from the embedded resource
                using Stream modelStream = ResourceHelper.Instance.GetFileStream(modelResource);
                using MemoryStream memoryStream = new MemoryStream();

                await modelStream.CopyToAsync(memoryStream);

                // Create inference session from the stream
                _model = new InferenceSession(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load ONNX model: {ex.Message}", ex);
            }
        }
    }
}