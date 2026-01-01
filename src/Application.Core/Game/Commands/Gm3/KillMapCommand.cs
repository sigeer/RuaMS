namespace Application.Core.Game.Commands.Gm3;

public class KillMapCommand : CommandBase
{
    public KillMapCommand() : base(3, "killmap")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        foreach (var mch in player.getMap().getAllPlayers())
        {
            mch.KilledBy(player);
        }
        return Task.CompletedTask;
    }
}
