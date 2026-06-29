using Application.Core.Game.Players;
using Application.Shared.Servers;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;

namespace Application.Core.Channel
{
    public interface IChannelServer : ISocketServer, IActorInstance<WorldChannel>, ITickableTree
    {
        int Id { get; }

        /// <summary>
        /// 在目标玩家所在的 Map Actor 上执行操作。
        /// 封装了 PlayerStorage → MapActor → Player 的 Actor 路由。
        /// 玩家不在线则静默跳过。
        /// </summary>
        Task SendToPlayerAsync(int playerId, Func<Player, Task> action);
    }
}
