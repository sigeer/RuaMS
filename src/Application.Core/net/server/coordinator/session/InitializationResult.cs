using Application.Shared.Sessions;

namespace net.server.coordinator.session;

public class InitializationResult
{
    public static readonly InitializationResult SUCCESS = new(AntiMulticlientResult.SUCCESS);
    public static readonly InitializationResult ALREADY_INITIALIZED = new(AntiMulticlientResult.REMOTE_PROCESSING);
    public static readonly InitializationResult TIMED_OUT = new(AntiMulticlientResult.COORDINATOR_ERROR);
    public static readonly InitializationResult ERROR = new(AntiMulticlientResult.COORDINATOR_ERROR);

    private AntiMulticlientResult antiMulticlientResult;

    InitializationResult(AntiMulticlientResult antiMulticlientResult)
    {
        this.antiMulticlientResult = antiMulticlientResult;
    }

    public AntiMulticlientResult getAntiMulticlientResult()
    {
        return antiMulticlientResult;
    }
}
