using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.EntityFrameworkCore;
using InventorySearch.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace InventorySearch.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly IImageRepository _imageRepository;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public ImageUploadService(IImageRepository imageRepository, IEmbeddingGenerator embeddingGenerator)
    {
        _imageRepository = imageRepository;
        _embeddingGenerator = embeddingGenerator;
    }

    public async Task<Vector> GenerateEmbeddingAsync(byte[] imageBytes)
    {
        return await _embeddingGenerator.GenerateEmbeddingAsync(imageBytes);
    }

    public async Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.05)
    {
        return await _imageRepository.IsDuplicateAsync(embedding, threshold);
    }

    public async Task<ImageObject> SaveImageAsync(ImageObject image)
    {
        return await _imageRepository.SaveAsync(image);
    }
}