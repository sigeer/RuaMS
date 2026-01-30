using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;

namespace Application.Shared.Client
{
    public interface ISocketClient
    {
        ISocketServer CurrentServerBase { get; }
        long SessionId { get; }
        IChannel NettyChannel { get; }

        Hwid? Hwid { get; set; }
        string RemoteAddress { get; }

        DateTimeOffset LastPacket { get; }
        void PongReceived();
        void ProcessPacket(InPacket packet);
        void sendPacket(Packet packet);
        void CloseSession();
        void CloseSocket();
        void ForceDisconnect();
        string GetSessionRemoteHost();

        bool tryacquireClient();
        void releaseClient();
    }
}
