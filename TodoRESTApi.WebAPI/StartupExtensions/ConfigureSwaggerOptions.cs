using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TodoRESTApi.WebAPI.StartupExtensions;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Constructor that initializes the API version description provider.
    /// </summary>
    /// <param name="provider">The API version description provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    /// <summary>
    /// Configures Swagger to generate separate documents for each API version.
    /// </summary>
    /// <param name="options">SwaggerGen options to modify.</param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName, 
                new OpenApiInfo()
                {
                    Title = $"My API {description.ApiVersion}", 
                    Version = description.ApiVersion.ToString() 
                });
        }
    }
}
