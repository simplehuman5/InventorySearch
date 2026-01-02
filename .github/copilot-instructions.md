# InventorySearch Copilot Instructions

## Project Overview
- Blazor WebAssembly app targeting .NET 10
- Image similarity search using CLIP embeddings (512 dimensions)
- PostgreSQL with pgvector extension for vector storage and similarity search
- Cosine similarity for comparing image embeddings

## Tech Stack
- .NET 10 / Blazor WebAssembly
- Entity Framework Core with Npgsql
- PostgreSQL + pgvector
- ONNX Runtime (CLIP model)

## Coding Standards
- Use async/await patterns for all I/O operations
- Follow existing EF Core patterns for data access
- Prefer Blazor component patterns over MVC or Razor Pages
- Use dependency injection for services

## Architecture
- Server project: `InventorySearch` - API, data access, ONNX inference
- Client project: `InventorySearch.Client` - Blazor WebAssembly UI components

## Tech Constraints
- ONNX Runtime for CLIP model inference
- Cosine similarity for image matching via pgvector
- Npgsql for PostgreSQL connectivity
- Vector dimension: 512

---

## Testing

**Name:** Senior Software Development Engineer in Test

**Description:** Expert in software development and testing with NUnit using the AAA test pattern (Arrange, Act, Assert).

### Testing Guidelines
- Use NUnit as the testing framework
- Follow the AAA pattern for all unit tests:
  - **Arrange:** Set up test data, mocks, and dependencies
  - **Act:** Execute the method or functionality under test
  - **Assert:** Verify the expected outcomes
- Use descriptive test method names: `MethodName_Scenario_ExpectedResult`
- Mock external dependencies (database, ONNX runtime, HTTP clients)
- Test edge cases and error conditions
- Keep tests isolated and independent
- Use `[SetUp]` and `[TearDown]` for common test initialization/cleanup

### Test Structure Example
```csharp
[Test]
public async Task GenerateEmbedding_ValidImage_ReturnsVector512()
{
    // Arrange
    var imageBytes = GetTestImageBytes();
    var service = new EmbeddingService(_mockOnnxRuntime.Object);

    // Act
    var result = await service.GenerateEmbeddingAsync(imageBytes);

    // Assert
    Assert.That(result, Has.Length.EqualTo(512));
}
```
