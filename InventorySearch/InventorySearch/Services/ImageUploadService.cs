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
    private readonly AppDbContext _db;
    private readonly InferenceSession _onnxSession;

    public ImageUploadService(AppDbContext db, InferenceSession onnxSession)
    {
        _db = db;
        _onnxSession = onnxSession;
    }

    public async Task<Vector> GenerateEmbeddingAsync(byte[] imageBytes)
    {
        const int TargetSize = 224;

        // CLIP mean and std (RGB order)
        var mean = new float[] { 0.48145466f, 0.4578275f, 0.40821073f };
        var std = new float[] { 0.26862954f, 0.26130258f, 0.27577711f };

        using var image = await Image.LoadAsync<Rgb24>(new MemoryStream(imageBytes));

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(TargetSize, TargetSize),
            Mode = ResizeMode.Crop // center crop to maintain aspect ratio
        }));

        var tensor = new DenseTensor<float>(new[] { 1, 3, TargetSize, TargetSize });

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < TargetSize; y++)
            {
                Span<Rgb24> pixelRow = accessor.GetRowSpan(y);

                for (int x = 0; x < TargetSize; x++)
                {
                    var pixel = pixelRow[x];
                    tensor[0, 0, y, x] = (pixel.R / 255.0f - mean[0]) / std[0];
                    tensor[0, 1, y, x] = (pixel.G / 255.0f - mean[1]) / std[1];
                    tensor[0, 2, y, x] = (pixel.B / 255.0f - mean[2]) / std[2];
                }
            }
        });

        var inputName = _onnxSession.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, tensor)
        };

        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _onnxSession.Run(inputs);

        // Extract the embedding (last layer pooled output)
        var output = results[0].AsTensor<float>();

        // Debug: Log the output shape to understand tensor dimensions
        var dimensions = output.Dimensions.ToArray();
        Console.WriteLine($"Output tensor shape: [{string.Join(", ", dimensions)}]");

        // Flatten the output tensor to get the embedding
        var embeddingArray = new float[512];
        var flatOutput = output.ToArray();

        // Copy first 512 values (or less if output is smaller)
        var copyLength = Math.Min(512, flatOutput.Length);
        Array.Copy(flatOutput, embeddingArray, copyLength);

        // L2 normalize (standard practice for CLIP embeddings)
        float norm = 0f;
        for (int i = 0; i < embeddingArray.Length; i++)
            norm += embeddingArray[i] * embeddingArray[i];

        norm = MathF.Sqrt(norm);
        if (norm > 0f)
        {
            for (int i = 0; i < embeddingArray.Length; i++)
                embeddingArray[i] /= norm;
        }

        return new Vector(embeddingArray);
    }

    public async Task<bool> IsDuplicateAsync(Vector embedding, double threshold = 0.05)
    {
        return await _db.Images
            .AnyAsync(i => i.Embedding.CosineDistance(embedding) < threshold);
    }

    public async Task<ImageObject> SaveImageAsync(ImageObject image)
    {
        _db.Images.Add(image);
        await _db.SaveChangesAsync();
        return image;
    }
}