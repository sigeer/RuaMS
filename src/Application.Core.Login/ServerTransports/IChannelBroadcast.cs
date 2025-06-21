using Application.Core.Login.Servers;

namespace Application.Core.Login.ServerTransports
{
    public interface IChannelBroadcast
    {
        void Broadcast(ChannelServerWrapper server, object message);
    }
}
