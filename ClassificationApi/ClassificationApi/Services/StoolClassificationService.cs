using ClassificationApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace ClassificationApi.Services
{
    public class StoolClassificationService : IStoolClassificationService
    {
        private readonly IModelService _modelService;
        private const int ImageSize = 224;
        private const int NumClasses = 7;

        // Class labels in the same order as the Python code
        private static readonly string[] ClassLabels = {
            "type-1", "type-2", "type-3", "type-4", "type-5", "type-6", "type-7"
        };

        // ImageNet normalization values (same as Python code)
        private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
        private static readonly float[] Std = { 0.229f, 0.224f, 0.225f };

        public StoolClassificationService(IModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<ClassificationResponse> ClassifyImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validate input
                if (imageFile == null || imageFile.Length == 0)
                {
                    return new ClassificationResponse
                    {
                        Error = "No image file provided or file is empty."
                    };
                }

                // Load and preprocess image
                float[] preprocessedImage = await PreprocessImageAsync(imageFile);

                // Get model and run inference
                InferenceSession model = await _modelService.GetModelAsync();
                ClassificationResponse result = await RunInferenceAsync(model, preprocessedImage);

                return result;
            }
            catch (Exception ex)
            {
                return new ClassificationResponse
                {
                    Error = $"Classification failed: {ex.Message}"
                };
            }
        }

        private async Task<float[]> PreprocessImageAsync(IFormFile imageFile)
        {
            using Stream imageStream = imageFile.OpenReadStream();
            using Image<Rgb24> image = await Image.LoadAsync<Rgb24>(imageStream);

            // Resize image to 224x224 (same as Python code)
            image.Mutate(x => x.Resize(ImageSize, ImageSize));

            // Convert to tensor and normalize
            float[] tensor = new float[3 * ImageSize * ImageSize];
            int index = 0;

            for (int y = 0; y < ImageSize; y++)
            {
                for (int x = 0; x < ImageSize; x++)
                {
                    Rgb24 pixel = image[x, y];

                    // Convert to float and normalize (same as Python transforms)
                    float r = (pixel.R / 255.0f - Mean[0]) / Std[0];
                    float g = (pixel.G / 255.0f - Mean[1]) / Std[1];
                    float b = (pixel.B / 255.0f - Mean[2]) / Std[2];

                    // CHW format (Channel, Height, Width)
                    tensor[index] = r;                    // Red channel
                    tensor[index + ImageSize * ImageSize] = g;  // Green channel
                    tensor[index + 2 * ImageSize * ImageSize] = b; // Blue channel
                    index++;
                }
            }

            return tensor;
        }

        private async Task<ClassificationResponse> RunInferenceAsync(InferenceSession model, float[] preprocessedImage)
        {
            await Task.CompletedTask;

            // Create input tensor
            DenseTensor<float> inputTensor = new DenseTensor<float>(preprocessedImage, new int[] { 1, 3, ImageSize, ImageSize });
            List<NamedOnnxValue> inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", inputTensor) };

            // Run inference
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = model.Run(inputs);

            // Get output
            DisposableNamedOnnxValue output = results.First();
            Tensor<float> outputTensor = output.AsTensor<float>();

            // Convert to probabilities (softmax)
            float[] logits = outputTensor.ToArray();
            float[] probabilities = Softmax(logits);

            // Find the class with highest probability
            int predictedClass = 0;
            float maxProbability = probabilities[0];

            for (int i = 1; i < NumClasses; i++)
            {
                if (probabilities[i] > maxProbability)
                {
                    maxProbability = probabilities[i];
                    predictedClass = i;
                }
            }

            return new ClassificationResponse
            {
                PredictedType = ClassLabels[predictedClass],
                Confidence = maxProbability
            };
        }

        private static float[] Softmax(float[] logits)
        {
            // Find max for numerical stability
            float maxLogit = logits.Max();

            // Compute exp(x - max) for each logit
            float[] expLogits = logits.Select(x => (float)Math.Exp(x - maxLogit)).ToArray();

            // Compute sum
            float sum = expLogits.Sum();

            // Normalize to get probabilities
            return expLogits.Select(x => x / sum).ToArray();
        }
    }
}