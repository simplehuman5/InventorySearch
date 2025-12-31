

using Pgvector;

namespace InventorySearch.Data
{
    public class ImageObject
    {
        public int Id { get; set; }
        public required string Name { get; set; }       // User-provided name (UNIQUE in DB)
        public required Vector Embedding { get; set; }  // 512-dim vector
        public string? Filename { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
