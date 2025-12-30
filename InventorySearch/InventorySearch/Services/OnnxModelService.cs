using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;

namespace InventorySearch.Services;

public interface IOnnxModelService
{
    /// <summary>
    /// Gets the InferenceSession for the ONNX model, downloading if necessary
    /// </summary>
    Task<InferenceSession> GetSessionAsync();
    
    /// <summary>
    /// Ensures the model file exists, downloading if configured and necessary
    /// </summary>
    Task EnsureModelExistsAsync();
}

public class OnnxModelService : IOnnxModelService, IDisposable
{
    private readonly OnnxModelOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OnnxModelService> _logger;
    private InferenceSession? _session;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

    public OnnxModelService(
        IOptions<OnnxModelOptions> options,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<OnnxModelService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<InferenceSession> GetSessionAsync()
    {
        if (_session is not null)
            return _session;

        await _lock.WaitAsync();
        try
        {
            if (_session is not null)
                return _session;

            await EnsureModelExistsAsync();
            
            var modelPath = GetFullModelPath();
            _logger.LogInformation("Loading ONNX model from {ModelPath}", modelPath);
            _session = new InferenceSession(modelPath);
            
            return _session;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task EnsureModelExistsAsync()
    {
        var modelPath = GetFullModelPath();
        
        if (File.Exists(modelPath))
        {
            _logger.LogInformation("ONNX model found at {ModelPath}", modelPath);
            return;
        }
        _logger.LogError("ONNX model NOT found at {ModelPath}", modelPath);

        if (!_options.AutoDownload)
        {
            throw new FileNotFoundException(
                $"ONNX model not found at '{modelPath}' and AutoDownload is disabled. " +
                $"Please download the model manually or enable AutoDownload in configuration.",
                modelPath);
        }

        _logger.LogInformation("ONNX model not found. Downloading from {Url}", _options.DownloadUrl);
        await DownloadModelAsync(modelPath);
    }

    private async Task DownloadModelAsync(string destinationPath)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(30); // Large file download timeout

        try
        {
            _logger.LogInformation("Starting download of ONNX model...");
            
            using var response = await client.GetAsync(_options.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            _logger.LogInformation("Model size: {Size} MB", totalBytes / 1024 / 1024);

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            long downloadedBytes = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;

                if (totalBytes.HasValue && downloadedBytes % (10 * 1024 * 1024) < 8192) // Log every ~10MB
                {
                    var progress = (double)downloadedBytes / totalBytes.Value * 100;
                    _logger.LogInformation("Download progress: {Progress:F1}%", progress);
                }
            }

            _logger.LogInformation("ONNX model downloaded successfully to {Path}", destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download ONNX model from {Url}", _options.DownloadUrl);
            
            // Clean up partial download
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            
            throw;
        }
    }

    private string GetFullModelPath()
    {
        if (Path.IsPathRooted(_options.ModelPath))
            return _options.ModelPath;

        return Path.Combine(_environment.WebRootPath, _options.ModelPath);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _session?.Dispose();
        _lock.Dispose();
        _disposed = true;
    }
}
