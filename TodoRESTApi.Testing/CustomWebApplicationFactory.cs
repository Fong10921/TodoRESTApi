using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Serilog;
using TodoRESTApi.Entities.Context;
using TodoRESTApi.identity.Identity;

namespace TodoRESTApi.Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // 1. Remove the app's TodoDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TodoDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 2. Add a new TodoDbContext using InMemory
            services.AddDbContext<TodoDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestingDB");
            });
            
            /*
            // Mock authentication service
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()
            )).Returns(Task.CompletedTask);
            
            services.AddSingleton(authServiceMock.Object);
            */
            
            // 5. Optional: build the service provider and create the database
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<TodoDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            }
            
            // Create and configure the HttpContext to be mock
            var httpContext = new DefaultHttpContext
            {
                RequestServices = sp
            };
            
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);

            
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Remove all other loggers (optional)
                logging.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Seq("http://localhost:5341")
                    .CreateLogger());
            });
        });
    }
}
