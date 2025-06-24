using ClassificationApi.Services;

namespace ClassificationApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Register our services
            builder.Services.AddSingleton<IModelService, ModelService>();
            builder.Services.AddScoped<IStoolClassificationService, StoolClassificationService>();

            WebApplication app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
