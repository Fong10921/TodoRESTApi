using Asp.Versioning.ApiExplorer;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Hangfire;
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

// Add Background Jobs Dashboard
app.UseHangfireDashboard("/hangfire"); 

app.MapRazorPages();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHeaderPropagation();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();