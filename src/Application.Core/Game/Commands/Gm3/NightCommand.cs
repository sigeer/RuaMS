namespace Application.Core.Game.Commands.Gm3;

public class NightCommand : CommandBase
{
    public NightCommand() : base(3, "night")
    {
        Description = "Set sky background to black.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        await player.getMap().broadcastNightEffect();
        await player.Yellow("Done.");
    }
}
