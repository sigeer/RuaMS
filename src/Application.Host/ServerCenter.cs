using Application.Core.Game.TheWorld;
using Application.Shared.Net;
using Application.Shared.Servers;
using net.netty;
using System.Net;

namespace Application.Host
{
    public class ServerCenter : IWorldLogin
    {
        public int Port { get; }

        public AbstractServer NettyServer { get; }

        public int World { get; }

        public int Channel { get; }

        public DateTimeOffset StartTime { get; }

        public IServerTransport Transport => throw new NotImplementedException();

        public string InstanceId { get; }

        public ServerCenter()
        {
            InstanceId = Guid.NewGuid().ToString();

            Port = 8484;
            World = -1;
            Channel = -1;
            StartTime = DateTimeOffset.Now;

            NettyServer = new LoginServer(this);
        }


        public void BroadcastGMMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public void BroadcastMessage(Packet p)
        {
            throw new NotImplementedException();
        }

        public IPEndPoint GetChannelEndPoint(int index)
        {
            throw new NotImplementedException();
        }

        public IWorldChannel GetChannelServer(int index)
        {
            throw new NotImplementedException();
        }

        public long GetServerCurrentTime()
        {
            throw new NotImplementedException();
        }

        public int GetServerCurrentTimestamp()
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetServerUpTime()
        {
            throw new NotImplementedException();
        }

        public async Task StartServer()
        {
            await NettyServer.Start();
        }

        public async Task Shutdown()
        {
            await NettyServer.Stop();
        }

        public Task Register()
        {
            return Task.CompletedTask;
        }

        public Task UnRegister()
        {
            return Task.CompletedTask;
        }
    }
}
