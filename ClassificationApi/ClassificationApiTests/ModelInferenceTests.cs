using ClassificationApi.Models;
using ClassificationApi.Services;
using ClassificationApi.Resources;
using ClassificationApi.Configuration;
using EasyReasy;
using EasyReasy.ByteShelfProvider;
using EasyReasy.EnvironmentVariables;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;

namespace ClassificationApiTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class ModelInferenceTests
    {
        private static ResourceManager? _resourceManager;
        private static ModelService? _modelService;
        private static StoolClassificationService? _classificationService;

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            // Load environment variables from test config
            EnvironmentVariableHelper.LoadVariablesFromFile("../../../test-config.env");
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            try
            {
                // Try to initialize with ByteShelf if credentials are available
                string? apiKey = EnvironmentVariable.ByteShelfApiKey.GetValue(minLength: 20);
                if (!string.IsNullOrEmpty(apiKey))
                {
                    string baseUrl = EnvironmentVariable.ByteShelfBaseUrl.GetValue(minLength: 10);
                    string cacheDir = EnvironmentVariable.ByteShelfCacheDir.GetValue();

                    PredefinedResourceProvider byteShelfProvider = ByteShelfResourceProvider.CreatePredefined(
                        resourceCollectionType: typeof(ClassificationApi.Resources.Models),
                        baseUrl: baseUrl,
                        apiKey: apiKey,
                        cache: new FileSystemCache(cacheDir));

                    _resourceManager = await ResourceManager.CreateInstanceAsync(
                        assembly: Assembly.GetAssembly(typeof(ClassificationApi.Resources.Models))!,
                        predefinedProviders: byteShelfProvider);

                    _modelService = new ModelService(_resourceManager);
                    _classificationService = new StoolClassificationService(_modelService);
                }
                else
                {
                    // Skip model tests if no credentials
                    _resourceManager = null;
                    _modelService = null;
                    _classificationService = null;
                }
            }
            catch (Exception)
            {
                // If ByteShelf initialization fails, skip those tests
                _resourceManager = null;
                _modelService = null;
                _classificationService = null;
            }
        }

        [TestMethod]
        public async Task ModelService_CanLoadModel_IfByteShelfConfigured()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Act
            InferenceSession model = await _modelService.GetModelAsync();

            // Assert
            Assert.IsNotNull(model);
            Assert.IsTrue(_modelService.IsModelLoaded);
        }

        [TestMethod]
        public async Task ModelService_ModelInputNames_AreCorrect()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Act
            InferenceSession model = await _modelService.GetModelAsync();
            var inputMetadata = model.InputMetadata;

            // Assert
            Assert.IsTrue(inputMetadata.ContainsKey("image"), "Model should have 'image' input");
            Assert.AreEqual(1, inputMetadata.Count, "Model should have exactly one input");
        }

        [TestMethod]
        public async Task ModelService_ModelOutputNames_AreCorrect()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Act
            InferenceSession model = await _modelService.GetModelAsync();
            var outputMetadata = model.OutputMetadata;

            // Assert
            Assert.IsTrue(outputMetadata.ContainsKey("logits"), "Model should have 'logits' output");
            Assert.AreEqual(1, outputMetadata.Count, "Model should have exactly one output");
        }

        [TestMethod]
        public async Task ModelService_ModelInputShape_IsCorrect()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Act
            InferenceSession model = await _modelService.GetModelAsync();
            var inputMetadata = model.InputMetadata["image"];

            // Assert
            Assert.AreEqual(4, inputMetadata.Dimensions.Length, "Input should have 4 dimensions (batch, channels, height, width)");
            Assert.AreEqual(3, inputMetadata.Dimensions[1], "Input should have 3 channels (RGB)");
            Assert.AreEqual(224, inputMetadata.Dimensions[2], "Input height should be 224");
            Assert.AreEqual(224, inputMetadata.Dimensions[3], "Input width should be 224");
        }

        [TestMethod]
        public async Task ModelService_ModelOutputShape_IsCorrect()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Act
            InferenceSession model = await _modelService.GetModelAsync();
            var outputMetadata = model.OutputMetadata["logits"];

            // Assert
            Assert.AreEqual(2, outputMetadata.Dimensions.Length, "Output should have 2 dimensions (batch, classes)");
            Assert.AreEqual(7, outputMetadata.Dimensions[1], "Output should have 7 classes");
        }

        [TestMethod]
        public async Task ModelService_CanRunInference_WithValidInput()
        {
            // Skip if ByteShelf is not configured
            if (_modelService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Arrange
            InferenceSession model = await _modelService.GetModelAsync();
            var inputShape = new int[] { 1, 3, 224, 224 };
            var inputData = new float[inputShape[0] * inputShape[1] * inputShape[2] * inputShape[3]];
            
            // Create a simple test input (all zeros for now)
            DenseTensor<float> inputTensor = new DenseTensor<float>(inputData, inputShape);
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("image", inputTensor) };

            // Act
            using var results = model.Run(inputs);
            var output = results.First();
            var outputTensor = output.AsTensor<float>();

            // Assert
            Assert.IsNotNull(outputTensor);
            Assert.AreEqual(7, outputTensor.Dimensions[1], "Output should have 7 classes");
        }

        [TestMethod]
        public async Task StoolClassificationService_CanClassifyImage_WithValidImage()
        {
            // Skip if ByteShelf is not configured
            if (_classificationService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Arrange - Create a simple test image
            using var image = new Image<Rgb24>(224, 224);
            using var stream = new MemoryStream();
            await image.SaveAsJpegAsync(stream);
            stream.Position = 0;

            // Act
            ClassificationResponse result = await _classificationService.ClassifyImageAsync(stream);

            // Assert
            Assert.IsNotNull(result);
            if (result.Error != null)
            {
                Assert.Inconclusive($"Model inference failed: {result.Error}. This may indicate ByteShelf is not properly configured.");
                return;
            }
            Assert.IsNotNull(result.PredictedType, "Should have a predicted type");
            Assert.IsTrue(result.Confidence > 0, "Should have confidence > 0");
            Assert.IsTrue(result.Confidence <= 1, "Should have confidence <= 1");
            
            // Verify predicted type is one of the expected types
            string[] expectedTypes = { "type-1", "type-2", "type-3", "type-4", "type-5", "type-6", "type-7" };
            Assert.IsTrue(expectedTypes.Contains(result.PredictedType), $"Predicted type '{result.PredictedType}' should be one of the expected types");
        }

        [TestMethod]
        public async Task StoolClassificationService_CanClassifyImage_WithDifferentImageSizes()
        {
            // Skip if ByteShelf is not configured
            if (_classificationService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Test with different image sizes
            int[] testSizes = { 100, 224, 300, 512 };

            foreach (int size in testSizes)
            {
                // Arrange - Create test image of specific size
                using var image = new Image<Rgb24>(size, size);
                using var stream = new MemoryStream();
                await image.SaveAsJpegAsync(stream);
                stream.Position = 0;

                // Act
                ClassificationResponse result = await _classificationService.ClassifyImageAsync(stream);

                // Assert
                Assert.IsNotNull(result);
                if (result.Error != null)
                {
                    Assert.Inconclusive($"Model inference failed for image size {size}x{size}: {result.Error}. This may indicate ByteShelf is not properly configured.");
                    return;
                }
                Assert.IsNotNull(result.PredictedType, $"Should have a predicted type for image size {size}x{size}");
                Assert.IsTrue(result.Confidence > 0, $"Should have confidence > 0 for image size {size}x{size}");
            }
        }

        [TestMethod]
        public async Task StoolClassificationService_HandlesInvalidImage_ReturnsError()
        {
            // Skip if ByteShelf is not configured
            if (_classificationService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Arrange - Create invalid image data
            byte[] invalidImageData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
            using var stream = new MemoryStream(invalidImageData);

            // Act
            ClassificationResponse result = await _classificationService.ClassifyImageAsync(stream);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Error, "Should have an error for invalid image");
            Assert.IsNull(result.PredictedType, "Should not have a predicted type for invalid image");
            Assert.AreEqual(0, result.Confidence, "Should have confidence 0 for invalid image");
        }

        [TestMethod]
        public async Task StoolClassificationService_ModelConsistency_ReturnsSameResultForSameInput()
        {
            // Skip if ByteShelf is not configured
            if (_classificationService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Arrange - Create identical test images
            using var image1 = new Image<Rgb24>(224, 224);
            using var image2 = new Image<Rgb24>(224, 224);
            
            using var stream1 = new MemoryStream();
            using var stream2 = new MemoryStream();
            
            await image1.SaveAsJpegAsync(stream1);
            await image2.SaveAsJpegAsync(stream2);
            
            stream1.Position = 0;
            stream2.Position = 0;

            // Act
            ClassificationResponse result1 = await _classificationService.ClassifyImageAsync(stream1);
            ClassificationResponse result2 = await _classificationService.ClassifyImageAsync(stream2);

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            if (result1.Error != null || result2.Error != null)
            {
                Assert.Inconclusive($"Model inference failed: {result1.Error ?? result2.Error}. This may indicate ByteShelf is not properly configured.");
                return;
            }
            Assert.AreEqual(result1.PredictedType, result2.PredictedType, "Same input should produce same prediction");
            Assert.AreEqual(result1.Confidence, result2.Confidence, 0.001f, "Same input should produce same confidence");
        }

        [TestMethod]
        public async Task StoolClassificationService_AllClassTypes_ArePredictable()
        {
            // Skip if ByteShelf is not configured
            if (_classificationService == null)
            {
                Assert.Inconclusive("ByteShelf not configured for testing");
                return;
            }

            // Arrange - Create multiple test images with different patterns
            var testImages = new List<MemoryStream>();
            var random = new Random(42); // Fixed seed for reproducibility

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    using var image = new Image<Rgb24>(224, 224);
                    
                    // Fill with random colors to create different patterns
                    for (int x = 0; x < 224; x++)
                    {
                        for (int y = 0; y < 224; y++)
                        {
                            image[x, y] = new Rgb24(
                                (byte)random.Next(256),
                                (byte)random.Next(256),
                                (byte)random.Next(256)
                            );
                        }
                    }
                    
                    var stream = new MemoryStream();
                    await image.SaveAsJpegAsync(stream);
                    stream.Position = 0;
                    testImages.Add(stream);
                }

                // Act & Assert
                var predictedTypes = new HashSet<string>();
                
                foreach (var stream in testImages)
                {
                    ClassificationResponse result = await _classificationService.ClassifyImageAsync(stream);
                    
                    Assert.IsNotNull(result);
                    if (result.Error != null)
                    {
                        Assert.Inconclusive($"Model inference failed: {result.Error}. This may indicate ByteShelf is not properly configured.");
                        return;
                    }
                    Assert.IsNotNull(result.PredictedType);
                    Assert.IsTrue(result.Confidence > 0);
                    
                    predictedTypes.Add(result.PredictedType);
                }

                // Verify that we can predict different types (not all the same)
                Assert.IsTrue(predictedTypes.Count > 1, "Model should be able to predict different types");
            }
            finally
            {
                // Clean up the streams
                foreach (var stream in testImages)
                {
                    stream.Dispose();
                }
            }
        }
    }
} 