using Pgvector;

namespace InventorySearch.Services;

public interface IEmbeddingGenerator
{
    Task<Vector> GenerateEmbeddingAsync(byte[] imageBytes);
}