namespace net.netty;

public abstract class AbstractServer
{
    protected int port;

    protected AbstractServer(int port)
    {
        this.port = port;
    }

    public abstract Task Start();
    public abstract Task Stop();
}
