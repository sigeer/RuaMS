namespace Application.Core.Game.Commands.Gm2;

public class ClearDropsCommand : CommandBase
{
    public ClearDropsCommand() : base(2, "cleardrops")
    {
        Description = "Clear drops by player.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await player.getMap().clearDrops();
        await player.Pink("Cleared dropped items");
    }
}
