using ClassificationApi.Models;
using ClassificationApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ClassificationApiTests
{
    [TestClass]
    public class StoolClassificationServiceTests
    {
        private Mock<IModelService> _mockModelService = null!;
        private StoolClassificationService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockModelService = new Mock<IModelService>();
            _service = new StoolClassificationService(_mockModelService.Object);
        }

        [TestMethod]
        public async Task ClassifyImageAsync_WithNullFile_ReturnsError()
        {
            // Arrange
            IFormFile? nullFile = null;

            // Act
            ClassificationResponse result = await _service.ClassifyImageAsync(nullFile!);

            // Assert
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error!.Contains("No image file provided"));
            Assert.IsNull(result.PredictedType);
            Assert.AreEqual(0, result.Confidence);
        }

        [TestMethod]
        public async Task ClassifyImageAsync_WithEmptyFile_ReturnsError()
        {
            // Arrange
            Mock<IFormFile> mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            ClassificationResponse result = await _service.ClassifyImageAsync(mockFile.Object);

            // Assert
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error!.Contains("No image file provided"));
            Assert.IsNull(result.PredictedType);
            Assert.AreEqual(0, result.Confidence);
        }

        [TestMethod]
        public void StoolType_Enum_ContainsAllTypes()
        {
            // Arrange & Act
            StoolType[] types = Enum.GetValues<StoolType>();

            // Assert
            Assert.AreEqual(7, types.Length);
            Assert.IsTrue(types.Contains(StoolType.Type1));
            Assert.IsTrue(types.Contains(StoolType.Type2));
            Assert.IsTrue(types.Contains(StoolType.Type3));
            Assert.IsTrue(types.Contains(StoolType.Type4));
            Assert.IsTrue(types.Contains(StoolType.Type5));
            Assert.IsTrue(types.Contains(StoolType.Type6));
            Assert.IsTrue(types.Contains(StoolType.Type7));
        }

        [TestMethod]
        public void ClassificationResponse_Properties_AreAccessible()
        {
            // Arrange
            ClassificationResponse response = new ClassificationResponse
            {
                PredictedType = "type-1",
                Confidence = 0.95,
                Error = null
            };

            // Act & Assert
            Assert.AreEqual("type-1", response.PredictedType);
            Assert.AreEqual(0.95, response.Confidence);
            Assert.IsNull(response.Error);
        }

        [TestMethod]
        public void ClassificationResponse_WithError_PropertiesAreSet()
        {
            // Arrange
            ClassificationResponse response = new ClassificationResponse
            {
                PredictedType = null,
                Confidence = 0.0,
                Error = "Test error message"
            };

            // Act & Assert
            Assert.IsNull(response.PredictedType);
            Assert.AreEqual(0.0, response.Confidence);
            Assert.AreEqual("Test error message", response.Error);
        }
    }
}