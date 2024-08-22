namespace net.server.coordinator.session;



/**
 * Manages session initialization using remote host (ip address).
 */
public class SessionInitialization
{
    private static ILogger log = LogFactory.GetLogger(LogType.Session);
    private static int MAX_INIT_TRIES = 2;
    private const int RETRY_DELAY_MILLIS = 1777;

    private HashSet<string> remoteHostsInInitState = new();
    private List<object> locks = new(100);

    public SessionInitialization()
    {
        for (int i = 0; i < 100; i++)
        {
            locks.Add(new object());
        }
    }

    private object getLock(string remoteHost)
    {
        return locks.get(Math.Abs(remoteHost.GetHashCode()) % 100);
    }

    /**
     * Try to initialize a session. Should be called <em>before</em> any session initialization procedure.
     *
     * @return InitializationResult.SUCCESS if initialization was successful.
     * If it was successful, finalize() needs to be called shortly after,
     * or else the initialization will be left hanging in a bad state,
     * which means any subsequent initialization from the same remote host will fail.
     */
    public InitializationResult initialize(string remoteHost)
    {
        var lockObj = getLock(remoteHost);
        try
        {
            int tries = 0;
            while (true)
            {
                if (Monitor.TryEnter(lockObj))
                {
                    try
                    {
                        if (remoteHostsInInitState.Contains(remoteHost))
                        {
                            return InitializationResult.ALREADY_INITIALIZED;
                        }

                        remoteHostsInInitState.Add(remoteHost);
                    }
                    finally
                    {
                        Monitor.Exit(lockObj);
                    }

                    break;
                }
                else
                {
                    if (tries++ == MAX_INIT_TRIES)
                    {
                        return InitializationResult.TIMED_OUT;
                    }

                    Thread.Sleep(RETRY_DELAY_MILLIS);
                }
            }
        }
        catch (Exception e)
        {
            log.Error(e, "Failed to initialize session.");
            return InitializationResult.ERROR;
        }

        return InitializationResult.SUCCESS;
    }

    /**
     * Finalize an initialization. Should be called <em>after</em> any session initialization procedure.
     */
    public void finalize(string remoteHost)
    {
        var lockObj = getLock(remoteHost);
        Monitor.Enter(lockObj);
        try
        {
            remoteHostsInInitState.Remove(remoteHost);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }
}
