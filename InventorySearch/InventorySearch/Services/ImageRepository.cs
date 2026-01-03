using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventorySearch.Data;

namespace InventorySearch.Services;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _db;

    public ImageRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.05)
    {
        return await _db.Images
            .AnyAsync(i => i.Embedding.CosineDistance(embedding) < threshold);
    }

    public async Task<ImageObject> SaveAsync(ImageObject image)
    {
        _db.Images.Add(image);
        await _db.SaveChangesAsync();
        return image;
    }
}