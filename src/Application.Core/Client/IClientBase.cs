using Application.Core.Servers;
using Application.Core.ServerTransports;
using DotNetty.Transport.Channels;
using net.packet;
using net.server.coordinator.session;
using System.Net;

namespace Application.Core.Client
{
    public interface IClientBase: ISocketClient, IDisposable
    {
        IServerBase<IServerTransport> CurrentServer { get; }
        /// <summary>
        /// 对于LoginClient而言，登录后为true
        /// 对于ChannelClient而言，进入游戏后为true
        /// </summary>
        bool IsOnlined { get; }
        bool IsActive { get; }
        AccountEntity? AccountEntity { get; set; }

        bool CheckBirthday(DateTime date);
        bool CheckBirthday(int date);
    }
}
