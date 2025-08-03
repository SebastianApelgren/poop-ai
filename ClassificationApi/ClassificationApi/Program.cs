using ClassificationApi.Services;
using ClassificationApi.Configuration;
using EasyReasy;
using EasyReasy.ByteShelfProvider;
using EasyReasy.EnvironmentVariables;
using System.Reflection;

namespace ClassificationApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Validate all environment variables at startup using EasyReasy.EnvironmentVariables
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            // Get ByteShelf configuration using EasyReasy.EnvironmentVariables
            string byteShelfBaseUrl = EnvironmentVariable.ByteShelfBaseUrl.GetValue(minLength: 10);
            
            string byteShelfApiKey = EnvironmentVariable.ByteShelfApiKey.GetValue(minLength: 20);
            
            string cacheDirectory = EnvironmentVariable.ByteShelfCacheDir.GetValue();

            // Create ByteShelf provider with caching
            PredefinedResourceProvider byteShelfProvider = ByteShelfResourceProvider.CreatePredefined(
                resourceCollectionType: typeof(ClassificationApi.Resources.Models),
                baseUrl: byteShelfBaseUrl,
                apiKey: byteShelfApiKey,
                cache: new FileSystemCache(cacheDirectory));

            // Validate all resources at startup using EasyReasy
            ResourceManager resourceManager = await ResourceManager.CreateInstanceAsync(
                assembly: Assembly.GetExecutingAssembly(),
                predefinedProviders: byteShelfProvider);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Register our services
            builder.Services.AddSingleton<IModelService, ModelService>();
            builder.Services.AddScoped<IStoolClassificationService, StoolClassificationService>();
            builder.Services.AddSingleton(resourceManager);

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
