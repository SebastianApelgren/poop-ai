using ClassificationApi.Models;
using ClassificationApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ClassificationApiTests
{
    [TestClass]
    [TestCategory("Unit")]
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
        public async Task ClassifyImageAsync_WithNullStream_ReturnsError()
        {
            // Arrange
            Stream? nullStream = null;

            // Act
            ClassificationResponse result = await _service.ClassifyImageAsync(nullStream!);

            // Assert
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error!.Contains("Classification failed"));
            Assert.IsNull(result.PredictedType);
            Assert.AreEqual(0, result.Confidence);
        }

        [TestMethod]
        public async Task ClassifyImageAsync_WithEmptyStream_ReturnsError()
        {
            // Arrange
            Stream emptyStream = new MemoryStream();

            // Act
            ClassificationResponse result = await _service.ClassifyImageAsync(emptyStream);

            // Assert
            Assert.IsNotNull(result.Error);
            Assert.IsTrue(result.Error!.Contains("Classification failed"));
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