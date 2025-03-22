using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoRESTApi.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add services to the container.
        serviceCollection.AddControllersWithViews();

        // Add Controllers to the Dependency Injection
        serviceCollection.AddControllers();

        // Add Api Versioning to the Dependency Injection
        serviceCollection.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add support for discovering API endpoints (including minimal APIs)
        serviceCollection.AddEndpointsApiExplorer();

        // Add the 
        serviceCollection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();


        // Register Swagger generator for API documentation
        serviceCollection.AddSwaggerGen();


        return serviceCollection;
    }
}