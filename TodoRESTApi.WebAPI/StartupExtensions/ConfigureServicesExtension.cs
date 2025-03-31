using System.Net;
using System.Net.Mail;
using Asp.Versioning;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoRESTApi.Core.ExternalHelperInterface;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.identity.ServiceContracts;
using TodoRESTApi.Infrastructure.ExternalHelper;
using TodoRESTApi.Infrastructure.Service;
using TodoRESTApi.Repository;
using TodoRESTApi.RepositoryContracts;
using TodoRESTApi.Service;
using TodoRESTApi.ServiceContracts;
using TodoRESTApi.WebAPI.Provider;
using TodoRESTApi.WebAPI.Requirement;

namespace TodoRESTApi.WebAPI.StartupExtensions;

public static class ConfigureServicesExtension
{
    public static IServiceCollection ConfigureServices(this IServiceCollection serviceCollection,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Add services to the container.
        serviceCollection.AddControllersWithViews();

        // Add Controllers to the Dependency Injection
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
        }).AddEntityFrameworkStores<TodoDbContext>().AddDefaultTokenProviders();
        
        // Add Authorization
        serviceCollection.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build(); //enforces authoriation policy (user must be authenticated) for all the action methods
        });
        
        // Finish Add Authentication to the App
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Error";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

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

        // Include Http Request and Response to the Serilog
        serviceCollection.AddHttpLogging(options =>
        {
            options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });

        // Add support for discovering API endpoints (including minimal APIs)
        serviceCollection.AddEndpointsApiExplorer();

        // Setup Redis for Hangfire
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        
        serviceCollection.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect("localhost:6380"));  // Secondary Redis

        // Use Hangfire for Background Jobs
        serviceCollection.AddHangfire(config =>
            config.UseRedisStorage(redis)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings());

        // Add Hangfire Server
        serviceCollection.AddHangfireServer();

        // Add the Swagger Gen Configuration to the Dependency Injection
        serviceCollection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        // Register Swagger generator for API documentation
        serviceCollection.AddSwaggerGen();

        // Add Razor Page 
        serviceCollection.AddRazorPages();

        // Add HttpClient to the Dependency Injection
        serviceCollection.AddHeaderPropagation(options =>
        {
            options.Headers.Add("Cookie");
        });

        serviceCollection.AddHttpClient("WithCookies")
            .AddHeaderPropagation();
        
        // Add the TodoService and TodoRepository to the Dependency Injection
        serviceCollection.AddScoped<ITodoRepository, TodoRepository>();
        serviceCollection.AddScoped<ITodoService, TodoService>();
        serviceCollection.AddScoped<INodaTimeHelper, NodaTimeHelper>();
        serviceCollection.AddSingleton<ICustomEmailSender, EmailService>();
        serviceCollection.AddScoped<IRoleService, RoleService>();
        serviceCollection.AddScoped<IMetaRoleRepository, MetaRoleRepository>();
            
        // Register the custom dynamic policy provider
        serviceCollection.AddSingleton<IAuthorizationPolicyProvider, CompositeAuthorizationPolicyProvider>();

        // Register the custom authorization handler
        serviceCollection.AddScoped<IAuthorizationHandler, DynamicPermissionHandler>();
        serviceCollection.AddSingleton<IAuthorizationHandler, OrAuthorizationHandler>();

        return serviceCollection;
    }
}