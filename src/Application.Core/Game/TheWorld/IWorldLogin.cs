using Application.Shared.Servers;
using System.Net;

namespace Application.Core.Game.TheWorld
{
    public interface IWorldLogin : IServerBase<IServerTransport>
    {
        /// <summary>
        /// 获取频道服务器的IPEndPoint
        /// </summary>
        /// <param name="index">从1开始</param>
        /// <returns></returns>
        IPEndPoint GetChannelEndPoint(int index);
        IWorldChannel GetChannelServer(int index);
    }
}
