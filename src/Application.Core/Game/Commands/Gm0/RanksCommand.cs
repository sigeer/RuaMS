using Application.Core.Servers.Services;
using net.server.guild;

namespace Application.Core.Game.Commands.Gm0;

public class RanksCommand : CommandBase
{
    readonly RankService _rankService;
    public RanksCommand(RankService rankService) : base(0, "ranks")
    {
        _rankService = rankService;
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        player.sendPacket(GuildPackets.showPlayerRanks(NpcId.MAPLE_ADMINISTRATOR, _rankService.LoadPlayerRanking()));
        return Task.CompletedTask;
    }
}
