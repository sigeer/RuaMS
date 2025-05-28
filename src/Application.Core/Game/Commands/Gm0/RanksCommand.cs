using net.server.guild;

namespace Application.Core.Game.Commands.Gm0;

public class RanksCommand : CommandBase
{
    public RanksCommand() : base(0, "ranks")
    {
        Description = "Show player rankings.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        player.sendPacket(GuildPackets.showPlayerRanks(NpcId.MAPLE_ADMINISTRATOR, c.CurrentServer.RankService.LoadPlayerRanking()));
    }
}
