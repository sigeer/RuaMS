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
        Task<bool> SendToPlayerAsync(int playerId, Func<Player, Task> action);
        Task<bool> SendToPlayerAsync(int playerId, Action<Player> action);

        /// <summary>
        /// 批量版，在一次频道遍历中处理所有指定玩家。
        /// 每个玩家独立路由到其 Map Actor，互不阻塞。
        /// </summary>
        Task<int> SendToPlayersAsync(IEnumerable<int> playerIds, Func<Player, Task> action);
        Task<int> SendToPlayersAsync(IEnumerable<int> playerIds, Action<Player> action);

        Task BroadcastPlayersAsync(Func<Player, Task> action);
    }
}
