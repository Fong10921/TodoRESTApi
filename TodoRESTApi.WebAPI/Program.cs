using Asp.Versioning.ApiExplorer;
using Hangfire;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using TodoRESTApi.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Offload the Dependency Injection to the ConfigureServicesExtension
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

// Use Serilog as a Host
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services,
    LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add Swagger to the Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"My API {description.GroupName}");
        }
    });
}

app.UseSerilogRequestLogging(); // Use the Serilog to log the request as a middleware

// Enable Content Security Policy (CSP) to block inline scripts:
/*app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'");
    await next();
});*/


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint != null)
    {
        // Check for the DisableRateLimiting attribute
        var disableRateLimit = endpoint.Metadata.Any(m => m is DisableRateLimitingAttribute);

        // Log the endpoint information along with whether rate limiting is disabled
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("EndpointDebugLogger");
        logger.LogInformation("Endpoint: {EndpointDisplayName}, RateLimiting Disabled: {DisableRateLimit}",
            endpoint.DisplayName, disableRateLimit);
    }

    await next();
});


// Need to be after UseRouting for DisableRateLimiting to work
app.UseRateLimiter();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();

// Add Background Jobs Dashboard
app.UseHangfireDashboard("/hangfire");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("Rate limit exceeded"))
    {
        // The redirect has already been issued in the handler.
        // Optionally, you can also end the response here.
        context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
        await context.Response.CompleteAsync();
    }
});

app.MapHealthChecksUI();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true, // run ALL checks
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).RequireAuthorization("HealthCheckPolicy");

// Configure the Prometheus scraping endpoint
app.MapPrometheusScrapingEndpoint().AllowAnonymous();

app.Run();

// Require for integration test
public partial class Program { }