using Pgvector;
using InventorySearch.Data;

namespace InventorySearch.Services;

public interface IImageUploadService
{
    Task<Vector> GenerateEmbeddingAsync(byte[] imageBytes);
    Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.05);
    Task<ImageObject> SaveImageAsync(ImageObject image);
}