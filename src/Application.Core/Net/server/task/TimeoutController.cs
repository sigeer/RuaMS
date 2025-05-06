using Application.Core.Game.TheWorld;

namespace net.server.task;

public class TimeoutController : TimelyControllerBase
{
    protected override void HandleRun()
    {
        long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        var chars = worldChannel.getPlayerStorage().getAllCharacters();
        foreach (var chr in chars)
        {
            if (time - chr.getClient().getLastPacket() > YamlConfig.config.server.TIMEOUT_DURATION)
            {
                Log.Logger.Information("Chr {CharacterName} auto-disconnected due to inactivity", chr.getName());
                chr.getClient().disconnect(true, chr.getCashShop().isOpened());
            }
        }
    }
    readonly IWorldChannel worldChannel;
    public TimeoutController(IWorldChannel world) : base("TimeoutController", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
    {
        worldChannel = world;
    }
}
