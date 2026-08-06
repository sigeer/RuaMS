using Application.Utility.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Services
{
    public interface INodeServer : IActorInstance<WorldChannelServer>
    {
        Task BroadcastPlayersAsync(Func<Player, Task> action);
        Task<int> SendToPlayersAsync(IEnumerable<int> playerIds, Action<Player> action);
        Task<int> SendToPlayersAsync(IEnumerable<int> playerIds, Func<Player, Task> action);
        Task<bool> SendToPlayerAsync(int playerId, Action<Player> action);
        Task<bool> SendToPlayerAsync(int playerId, Func<Player, Task> action);
    }
}
