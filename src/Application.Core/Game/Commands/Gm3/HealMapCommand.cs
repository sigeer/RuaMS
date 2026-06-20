namespace Application.Core.Game.Commands.Gm3;

public class HealMapCommand : CommandBase
{
    public HealMapCommand() : base(3, "healmap")
    {
        Description = "Heal all HP/MP of all players in the map.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var mch in player.getMap().getAllPlayers())
        {
            if (mch != null)
            {
                await mch.healHpMp();
            }
        }
    }
}
