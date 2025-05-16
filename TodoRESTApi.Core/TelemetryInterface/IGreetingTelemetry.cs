namespace TodoRESTApi.Core.TelemetryInterface;

public interface IGreetingTelemetry
{
    void TrackGreeting();
    IDisposable? StartGreetingActivity(string name);
}