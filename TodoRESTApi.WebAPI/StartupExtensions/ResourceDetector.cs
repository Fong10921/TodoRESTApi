using OpenTelemetry.Resources;

namespace TodoRESTApi.WebAPI.StartupExtensions;

public class MyResourceDetector : IResourceDetector
{
    private readonly IWebHostEnvironment webHostEnvironment;

    public MyResourceDetector(IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
    }

    public Resource Detect()
    {
        return ResourceBuilder.CreateEmpty()
            .AddService(serviceName: this.webHostEnvironment.ApplicationName)
            .AddAttributes(new Dictionary<string, object> { ["host.environment"] = this.webHostEnvironment.EnvironmentName })
            .Build();
    }
}
