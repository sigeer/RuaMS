namespace Application.Core.Game.Commands.Gm4;

public class ZakumCommand : CommandBase
{
    public ZakumCommand() : base(4, "zakum")
    {
        Description = "Spawn Zakum on your location.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        await player.getMap().SpawnZakumOnGroundBelow(player.getPosition());
    }
}
