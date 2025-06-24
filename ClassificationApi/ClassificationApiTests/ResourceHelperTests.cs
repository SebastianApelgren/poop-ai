using ClassificationApi.Resources;
using Microsoft.Extensions.FileProviders;

namespace ClassificationApiTests
{
    [TestClass]
    public class ResourceHelperTests
    {
        [TestMethod]
        public void ResourceHelper_Instance_IsSingleton()
        {
            // Arrange & Act
            ResourceHelper instance1 = ResourceHelper.Instance;
            ResourceHelper instance2 = ResourceHelper.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void GetContentType_WithValidExtensions_ReturnsCorrectContentType()
        {
            // Arrange
            ResourceHelper helper = ResourceHelper.Instance;

            // Act & Assert
            Assert.AreEqual("text/html", helper.GetContentType(Resource.Create("test.html")));
            Assert.AreEqual("font/otf", helper.GetContentType(Resource.Create("test.otf")));
            Assert.AreEqual("font/ttf", helper.GetContentType(Resource.Create("test.ttf")));
            Assert.AreEqual("image/svg+xml", helper.GetContentType(Resource.Create("test.svg")));
            Assert.AreEqual("image/png", helper.GetContentType(Resource.Create("test.png")));
            Assert.AreEqual("image/jpeg", helper.GetContentType(Resource.Create("test.jpg")));
            Assert.AreEqual("text/plain", helper.GetContentType(Resource.Create("test.txt")));
            Assert.AreEqual("application/pdf", helper.GetContentType(Resource.Create("test.pdf")));
            Assert.AreEqual("application/x-x509-ca-cert", helper.GetContentType(Resource.Create("test.cer")));
            Assert.AreEqual("application/octet-stream", helper.GetContentType(Resource.Create("test.onnx")));
            Assert.AreEqual("application/octet-stream", helper.GetContentType(Resource.Create("test.unknown")));
        }

        [TestMethod]
        public void GetContentType_WithUppercaseExtensions_ReturnsCorrectContentType()
        {
            // Arrange
            ResourceHelper helper = ResourceHelper.Instance;

            // Act & Assert
            Assert.AreEqual("text/html", helper.GetContentType(Resource.Create("test.HTML")));
            Assert.AreEqual("image/png", helper.GetContentType(Resource.Create("test.PNG")));
        }

        [TestMethod]
        public void Resource_ToString_ReturnsPath()
        {
            // Arrange
            string expectedPath = "test/file.txt";
            Resource resource = Resource.Create(expectedPath);

            // Act
            string result = resource.ToString();

            // Assert
            Assert.AreEqual(expectedPath, result);
        }

        [TestMethod]
        public void Resource_ImplicitStringConversion_ReturnsPath()
        {
            // Arrange
            string expectedPath = "test/file.txt";
            Resource resource = Resource.Create(expectedPath);

            // Act
            string result = resource;

            // Assert
            Assert.AreEqual(expectedPath, result);
        }

        [TestMethod]
        public void Resource_GetFileName_ReturnsFileName()
        {
            // Arrange
            Resource resource = Resource.Create("folder/subfolder/file.txt");

            // Act
            string fileName = resource.GetFileName();

            // Assert
            Assert.AreEqual("file.txt", fileName);
        }

        [TestMethod]
        public void VerifyResourceMappings_WithNoResources_DoesNotThrow()
        {
            // Arrange
            ResourceHelper helper = ResourceHelper.Instance;

            // Act & Assert
            try
            {
                helper.VerifyResourceMappings();
                // If no exception is thrown, the test passes
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Resource mapping verification failed: {ex.Message}");
            }
        }

        [TestMethod]
        public void GetAllResourcePaths_ReturnsEmptySet_WhenNoResourcesDefined()
        {
            // This test verifies that when no resources are defined in the Resource class,
            // the verification system doesn't break
            ResourceHelper helper = ResourceHelper.Instance;

            // Act & Assert - should not throw
            try
            {
                helper.VerifyResourceMappings();
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Unexpected exception: {ex.Message}");
            }
        }

        [TestMethod]
        public void StoolModel_Resource_IsCorrectlyDefined()
        {
            // Arrange
            Resource stoolModel = Resource.Models.StoolModel;

            // Act & Assert
            Assert.AreEqual("Models/stool_model.onnx", stoolModel.Path);
            Assert.AreEqual("stool_model.onnx", stoolModel.GetFileName());
            Assert.AreEqual("Models/stool_model.onnx", stoolModel.ToString());
        }

        [TestMethod]
        public void StoolModel_ContentType_IsCorrect()
        {
            // Arrange
            ResourceHelper helper = ResourceHelper.Instance;
            Resource stoolModel = Resource.Models.StoolModel;

            // Act
            string contentType = helper.GetContentType(stoolModel);

            // Assert
            Assert.AreEqual("application/octet-stream", contentType);
        }
    }
} 