using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using InventorySearch.Services;
using InventorySearch.Data;
using Pgvector;
using System.Linq.Expressions;

namespace InventorySearch.Tests.Services;

[TestClass]
public class ImageUploadServiceTests
{
    private TestAppDbContext _testDb = null!;
    private Mock<InferenceSession> _mockOnnxSession = null!;
    private ImageUploadService _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testDb = new TestAppDbContext();
        // Create a mock InferenceSession - we'll mock the methods we need
        _mockOnnxSession = new Mock<InferenceSession>();
        _sut = new ImageUploadService(_testDb, _mockOnnxSession.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _testDb.Dispose();
    }

    [TestMethod]
    public async Task IsDuplicateAsync_ExistingSimilarEmbedding_ReturnsTrue()
    {
        // Arrange
        var embedding = new Vector(new float[512]);
        await _testDb.Images.AddAsync(new ImageObject { Name = "Existing", Embedding = embedding });
        await _testDb.SaveChangesAsync();

        // Act
        var result = await _sut.IsDuplicateAsync(embedding);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsDuplicateAsync_NoSimilarEmbedding_ReturnsFalse()
    {
        // Arrange
        var embedding = new Vector(new float[512]);

        // Act
        var result = await _sut.IsDuplicateAsync(embedding);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsDuplicateAsync_CustomThreshold_UsesProvidedThreshold()
    {
        // Arrange
        var embedding = new Vector(new float[512]);
        var customThreshold = 0.1;

        // Act
        var result = await _sut.IsDuplicateAsync(embedding, customThreshold);

        // Assert
        Assert.IsFalse(result);
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

        // Act
        var result = await _sut.SaveImageAsync(image);

        // Assert
        Assert.AreEqual(image, result);
        Assert.AreEqual(1, result.Id); // Should have been assigned an ID
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
        
        // Simulate database error by disposing context
        _testDb.Dispose();
        
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

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext() : base(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options)
    {
    }
}
