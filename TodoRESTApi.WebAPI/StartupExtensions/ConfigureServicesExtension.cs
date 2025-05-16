using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RedisRateLimiting;
using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using TodoRESTApi.Core.ExternalHelperInterface;
using TodoRESTApi.Core.TelemetryInterface;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;
using TodoRESTApi.Infrastructure.ExternalHelper;
using TodoRESTApi.Infrastructure.Service;
using TodoRESTApi.Infrastructure.Telemetry;
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
                .Build(); //enforces authorize policy (user must be authenticated) for all the action methods

            options.AddPolicy("HealthCheckPolicy", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    var httpContext = context.Resource as HttpContext;
                    if (httpContext is null)
                    {
                        throw new Exception("Http Context is null when trying to assign HealthCheckPolicy");
                    }

                    var authHeader = httpContext.Request.Headers["Authorization"].ToString();
                    return authHeader == "Bearer supertoken"; // match token
                });
            });
        });

        // Finish Add Authentication to the App
        serviceCollection.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = configuration["Authentication:LoginPath"];
                options.AccessDeniedPath = configuration["Authentication:AccessDenialPath"];
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.None; // Need for Social Login to work
                options.Cookie.IsEssential = true;

                if (environment.IsProduction())
                {
                    options.ExpireTimeSpan =
                        TimeSpan.FromMinutes(30); // Auto Logout in 30 min after login and refresh when user do action
                    options.SlidingExpiration = true;
                }
            });


        serviceCollection.AddAuthentication().AddGoogle(options =>
        {
            IConfigurationSection googleConfigurationSection = configuration.GetSection("Authentication:Google");
            options.ClientId = googleConfigurationSection["ClientId"]
                               ?? throw new Exception("Missing Google ClientId in configuration.");
            options.ClientSecret = googleConfigurationSection["ClientSecret"] ??
                                   throw new Exception("Missing Google ClientSecret in configuration.");
            options.SignInScheme = IdentityConstants.ExternalScheme; // Need this to work
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

        // Auto Add CSRF
        serviceCollection.AddAntiforgery();

        // Setup Redis for Hangfire
        var redis = ConnectionMultiplexer.Connect(configuration["Db Connection:Redis1"]!);

        /*serviceCollection.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect("localhost:6380")); */ // Secondary Redis

        var redis2 = ConnectionMultiplexer.Connect(configuration["Db Connection:Redis2"]!);

        serviceCollection.AddRateLimiter(options =>
        {
            // Setup Policy Based Limit Rate Limit
            options.AddRedisSlidingWindowLimiter("demo_sliding_window", (opt) =>
            {
                opt.ConnectionMultiplexerFactory = () => redis2;
                opt.PermitLimit = 1;
                opt.Window = TimeSpan.FromSeconds(2);
            });

            // Setup Global Rate Limit Based on IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RedisRateLimitPartition.GetSlidingWindowRateLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new RedisSlidingWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis2,
                        PermitLimit = 150,
                        Window = TimeSpan.FromMinutes(1)
                    }));


            options.OnRejected = async (context, cancellationToken) =>
            {
                // Show 429 instead of 503
                /*context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers["Retry-After"] = "60";

                await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken);*/

                var request = context.HttpContext.Request;

                // If it's an API call (expects JSON), return 429
                if (request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers["Retry-After"] = "60";

                    await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.",
                        cancellationToken);
                    return;
                }

                // Otherwise, assume it's a browser and redirect to a nice page
                context.HttpContext.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                context.HttpContext.Response.Headers["Location"] = "/RateLimitExceeded";
            };
        });

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
        serviceCollection.AddHeaderPropagation(options => { options.Headers.Add("Cookie"); });

        // Add WithCookies as a HttpClientFactory options
        serviceCollection.AddHttpClient("WithCookies")
            .AddHeaderPropagation()
            .AddHttpMessageHandler<RateLimitRedirectHandler>();

        // Setup Lockout
        serviceCollection.Configure<IdentityOptions>(options =>
        {
            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(5); // How long it will take to lockout the user for
            options.Lockout.MaxFailedAccessAttempts = 50; // Lock after 50 failed attempts
            options.Lockout.AllowedForNewUsers = true; // Enable Lockout for new users
        });

        // Export metrics from all HTTP clients registered in services
        /*serviceCollection.UseHttpClientMetrics();*/

        serviceCollection.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddCheck("fake-db", () => HealthCheckResult.Unhealthy("Simulated DB failure"))
            .AddCheck("external-api", () => HealthCheckResult.Degraded("Simulated slow API"))
            .AddRedis(redis, name: "redis", tags: new[] { "db", "redis" })
            .AddRedis(redis2!, name: "redis2", tags: new[] { "db", "redis2" })
            .AddUrlGroup(new Uri("https://localhost:7286/api/v1/GetTodo"));

        // Add HttpContext to dependency injection
        serviceCollection.AddHttpContextAccessor();

        serviceCollection
            .AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(15); // optional
                options.MaximumHistoryEntriesPerEndpoint(60); // optional
                options.SetApiMaxActiveRequests(1); // optional
                options.AddHealthCheckEndpoint("Everything Monitor", "https://localhost:7286/health");
                options.AddHealthCheckEndpoint("Todo", "https://localhost:7286/Account");
                options.AddHealthCheckEndpoint("Todo2", "https://localhost:7286/Account/Login");
                options.ConfigureApiEndpointHttpclient((sp, client) =>
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", "supertoken");
                });
            }).AddSqliteStorage("Data Source=app.db");

        serviceCollection.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: environment.ApplicationName))
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("MyApp.Tracing.Greeting")
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = "localhost";
                        options.AgentPort = 6831;
                    });

                tracingBuilder.AddOtlpExporter(otlp => { otlp.Endpoint = new Uri("http://localhost:4317"); });
            })
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddMeter("MyApp.Telemetry.Greeting")
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("System.Net.NameResolution")
                    .AddPrometheusExporter()
            );

        // Add the TodoService and TodoRepository to the Dependency Injection
        serviceCollection.AddScoped<ITodoRepository, TodoRepository>();
        serviceCollection.AddScoped<ITodoService, TodoService>();
        serviceCollection.AddScoped<INodaTimeHelper, NodaTimeHelper>();
        serviceCollection.AddSingleton<ICustomEmailSender, EmailService>();
        serviceCollection.AddScoped<IRoleService, RoleService>();
        serviceCollection.AddScoped<IMetaRoleRepository, MetaRoleRepository>();
        serviceCollection.AddScoped<ISignInRepository, SignInRepository>();
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IRoleRepository, RoleRepository>();
        // Register the custom dynamic policy provider
        serviceCollection.AddSingleton<IAuthorizationPolicyProvider, CompositeAuthorizationPolicyProvider>();

        serviceCollection.AddSingleton<MyResourceDetector>();
        serviceCollection.AddSingleton<IGreetingTelemetry, GreetingTelemetry>();

        // RateLimiter
        serviceCollection.AddTransient<RateLimitRedirectHandler>();

        // Register the custom authorization handler
        serviceCollection.AddScoped<IAuthorizationHandler, DynamicPermissionHandler>();
        serviceCollection.AddScoped<IAuthorizationHandler, OrAuthorizationHandler>();

        return serviceCollection;
    }
}