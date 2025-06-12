namespace Application.Shared.Servers
{
    public abstract class AbstractNettyServer
    {
        protected int port;

        protected AbstractNettyServer(int port)
        {
            this.port = port;
        }

        public abstract Task Start();
        public abstract Task Stop();
    }
}
