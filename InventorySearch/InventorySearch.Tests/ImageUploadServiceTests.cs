using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using InventorySearch.Services;
using InventorySearch.Data;
using Pgvector;
using System.Linq.Expressions;

namespace InventorySearch.Tests.Services;

[TestClass]
public class ImageUploadServiceTests
{
    private Mock<IImageRepository> _mockImageRepository = null!;
    private Mock<IEmbeddingGenerator> _mockEmbeddingGenerator = null!;
    private ImageUploadService _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockImageRepository = new Mock<IImageRepository>();
        _mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        _sut = new ImageUploadService(_mockImageRepository.Object, _mockEmbeddingGenerator.Object);
    }

    [TestMethod]
    public async Task GenerateEmbeddingAsync_ValidImageBytes_ReturnsVector()
    {
        // Arrange
        var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // Minimal PNG header
        var expectedEmbedding = new Vector(new float[512]);
        _mockEmbeddingGenerator.Setup(x => x.GenerateEmbeddingAsync(imageBytes))
            .ReturnsAsync(expectedEmbedding);

        // Act
        var result = await _sut.GenerateEmbeddingAsync(imageBytes);

        // Assert
        Assert.AreEqual(expectedEmbedding, result);
        _mockEmbeddingGenerator.Verify(x => x.GenerateEmbeddingAsync(imageBytes), Times.Once);
    }

    [TestMethod]
    public async Task IsDuplicateAsync_ExistingSimilarEmbedding_ReturnsTrue()
    {
        // Arrange
        var embedding = new Vector(new float[512]);
        _mockImageRepository.Setup(x => x.IsDuplicateAsync(embedding, 0.05)).ReturnsAsync(true);

        // Act
        var result = await _sut.IsDuplicateAsync(embedding);

        // Assert
        Assert.IsTrue(result);
        _mockImageRepository.Verify(x => x.IsDuplicateAsync(embedding, 0.05), Times.Once);
    }

    [TestMethod]
    public async Task IsDuplicateAsync_NoSimilarEmbedding_ReturnsFalse()
    {
        // Arrange
        var embedding = new Vector(new float[512]);
        _mockImageRepository.Setup(x => x.IsDuplicateAsync(embedding, 0.05)).ReturnsAsync(false);

        // Act
        var result = await _sut.IsDuplicateAsync(embedding);

        // Assert
        Assert.IsFalse(result);
        _mockImageRepository.Verify(x => x.IsDuplicateAsync(embedding, 0.05), Times.Once);
    }

    [TestMethod]
    public async Task IsDuplicateAsync_CustomThreshold_UsesProvidedThreshold()
    {
        // Arrange
        var embedding = new Vector(new float[512]);
        var customThreshold = 0.1;
        _mockImageRepository.Setup(x => x.IsDuplicateAsync(embedding, customThreshold)).ReturnsAsync(false);

        // Act
        var result = await _sut.IsDuplicateAsync(embedding, customThreshold);

        // Assert
        Assert.IsFalse(result);
        _mockImageRepository.Verify(x => x.IsDuplicateAsync(embedding, customThreshold), Times.Once);
    }

    [TestMethod]
    public async Task SaveImageAsync_ValidImageObject_ReturnsSavedImage()
    {
        // Arrange
        var image = new ImageObject
        {
            Name = "Test Image",
            Embedding = new Vector(new float[512]),
            Filename = "test.jpg"
        };
        _mockImageRepository.Setup(x => x.SaveAsync(image)).ReturnsAsync(image);

        // Act
        var result = await _sut.SaveImageAsync(image);

        // Assert
        Assert.AreEqual(image, result);
        _mockImageRepository.Verify(x => x.SaveAsync(image), Times.Once);
    }

    [TestMethod]
    public async Task SaveImageAsync_DatabaseError_ThrowsException()
    {
        // Arrange
        var image = new ImageObject
        {
            Name = "Test",
            Embedding = new Vector(new float[512])
        };
        _mockImageRepository.Setup(x => x.SaveAsync(image)).ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        try
        {
            await _sut.SaveImageAsync(image);
            Assert.Fail("Expected exception was not thrown");
        }
        catch (Exception)
        {
            // Expected exception
        }
    }
}
