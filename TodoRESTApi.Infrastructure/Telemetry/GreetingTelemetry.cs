using System.Diagnostics;
using System.Diagnostics.Metrics;
using TodoRESTApi.Core.TelemetryInterface;

namespace TodoRESTApi.Infrastructure.Telemetry;

public class GreetingTelemetry: IGreetingTelemetry
{
    private static readonly Meter _meter = new("MyApp.Telemetry.Greeting", "1.0.0");
    private static readonly Counter<int> _greetingCounter = _meter.CreateCounter<int>("greetings.count");

    private static readonly ActivitySource _activitySource = new("MyApp.Tracing.Greeting");

    public void TrackGreeting()
    {
        _greetingCounter.Add(1);
    }

    public IDisposable? StartGreetingActivity(string name)
    {
        var activity = _activitySource.StartActivity(name);
        activity?.SetTag("greeting", "Hello!");
        return activity;
    }
}