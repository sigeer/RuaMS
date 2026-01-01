namespace Application.Core.Game.Commands.Gm2;

public class ClearDropsCommand : CommandBase
{
    public ClearDropsCommand() : base(2, "cleardrops")
    {
        Description = "Clear drops by player.";
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        player.getMap().clearDrops();
        player.dropMessage(5, "Cleared dropped items");
        return Task.CompletedTask;
    }
}
