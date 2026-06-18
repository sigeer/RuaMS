namespace Application.Core.Game.Commands.Gm3;

public class MuteMapCommand : CommandBase
{
    public MuteMapCommand() : base(3, "mutemap")
    {
        Description = "Toggle mute players in the map.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (player.getMap().isMuted())
        {
            player.getMap().setMuted(false);
            await player.Pink("The map you are in has been un-muted.");
        }
        else
        {
            player.getMap().setMuted(true);
            await player.Pink("The map you are in has been muted.");
        }
    }
}
