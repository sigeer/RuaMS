using Application.Core.Session;
using DotNetty.Transport.Channels;

namespace Application.Core.Client
{
    public interface ISocketClient
    {
        long SessionId { get; }
        IChannel NettyChannel { get; }

        Hwid? Hwid { get; set; }
        string RemoteAddress { get; }

        DateTimeOffset LastPacket { get; }
        void PongReceived();

        void sendPacket(Packet packet);

        void CloseSocket();
        void ForceDisconnect();
        string GetSessionRemoteHost();

        bool tryacquireClient();
        void releaseClient();
    }
}
