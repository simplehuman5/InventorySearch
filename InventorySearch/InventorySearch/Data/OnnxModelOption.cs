namespace InventorySearch.Data;

public class OnnxModelOption
{
    //section name in appsettings.json
    public const string SectionName = "OnnxModel";
    
    /// <summary>
    /// Local path to the ONNX model file (relative to wwwroot or absolute)
    /// </summary>
    public string ModelPath { get; set; } = "models/clip-ViT-B-32-vision.onnx";
    
    /// <summary>
    /// URL to download the model from if it doesn't exist locally
    /// </summary>
    public string DownloadUrl { get; set; } = "https://huggingface.co/openai/clip-vit-base-patch32/resolve/main/onnx/vision_model.onnx";
    
    /// <summary>
    /// If true, automatically download the model if it doesn't exist
    /// </summary>
    public bool AutoDownload { get; set; } = false; //don't want to download. Let it fail
}
