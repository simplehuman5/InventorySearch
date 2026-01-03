using InventorySearch.Client.Pages;
using InventorySearch.Components;
using InventorySearch.Data;
using InventorySearch.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;

namespace InventorySearch
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
            
            // Register dbcontext with pgvector support
            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("InventoryDb"),
                    o => o.UseVector()));  // Enable pgvector support

            // Register HttpClient for model downloads
            builder.Services.AddHttpClient();
            
            var modelPath = Path.Combine(builder.Environment.WebRootPath,
                builder.Configuration.GetSection("OnnxModel")["ModelPath"] ?? "models/clip.onnx");
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("ONNX CLIP model not found. Place it in wwwroot/models/", modelPath);
            }
            var session = new InferenceSession(modelPath);
            
            // Log input/output names for debugging
            Console.WriteLine("ONNX Model Inputs:");
            foreach (var input in session.InputMetadata)
            {
                Console.WriteLine($"  - Name: {input.Key}, Shape: [{string.Join(", ", input.Value.Dimensions)}], Type: {input.Value.ElementType}");
            }
            Console.WriteLine("ONNX Model Outputs:");
            foreach (var output in session.OutputMetadata)
            {
                Console.WriteLine($"  - Name: {output.Key}, Shape: [{string.Join(", ", output.Value.Dimensions)}], Type: {output.Value.ElementType}");
            }
            
            builder.Services.AddSingleton(session);

            // Register application services
            builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
            builder.Services.AddScoped<IImageRepository, ImageRepository>();
            builder.Services.AddSingleton<IEmbeddingGenerator, OnnxEmbeddingGenerator>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
