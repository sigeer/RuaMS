using Application.Shared.Models;
using Application.Shared.Net;
using Application.Shared.Servers;
using DotNetty.Transport.Channels;

namespace Application.Shared.Client
{
    public interface ISocketClient: IAsyncDisposable
    {
        ISocketServer CurrentServerBase { get; }
        long SessionId { get; }
        IChannel NettyChannel { get; }

        Hwid? Hwid { get; set; }
        string RemoteAddress { get; }

        DateTimeOffset LastPacket { get; }
        void PongReceived();
        Task ProcessPacket(InPacket packet);
        Task SendPacket(Packet p);
        void CloseSession();
        Task CloseSocket();
        Task ForceDisconnect();
        string GetSessionRemoteHost();

        Task tryacquireClient();
        void releaseClient();
    }
}
