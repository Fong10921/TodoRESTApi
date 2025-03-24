using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.Repository;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service;
using TodoRESTApi.ServiceContracts;

namespace TodoRESTApi.WebAPI.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add services to the container.
        serviceCollection.AddControllersWithViews();

        // Add Controllers to the Dependency Injection, Add Json to Serialize Enums as Strings
        serviceCollection.AddControllers();
        
        // Use the app.db as Db when environment is not Test
        if (environment.IsEnvironment("Test") == false)
        {
            serviceCollection.AddDbContext<TodoDbContext>(optionsBuilder =>
                optionsBuilder.UseSqlite("Data Source=app.db")
            );
        }
        
        // Add Identity to the App
        serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;
            }).AddEntityFrameworkStores<TodoDbContext>().AddDefaultTokenProviders()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, TodoDbContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, TodoDbContext, Guid>>();
        
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

        // Add the Swagger Gen Configuration to the Dependency Injection
        serviceCollection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        // Register Swagger generator for API documentation
        serviceCollection.AddSwaggerGen();

        // Add Razor Page 
        serviceCollection.AddRazorPages();

        // Add HttpClient to the Dependency Injection
        serviceCollection.AddHttpClient();
        
        // Add the TodoService and TodoRepository to the Dependency Injection
        serviceCollection.AddScoped<ITodoRepository, TodoRepository>();
        serviceCollection.AddScoped<ITodoService, TodoService>();
        
        return serviceCollection;
    }
}