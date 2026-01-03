using Pgvector;
using InventorySearch.Data;

namespace InventorySearch.Services;

public interface IImageRepository
{
    Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.05);
    Task<ImageObject> SaveAsync(ImageObject image);
}