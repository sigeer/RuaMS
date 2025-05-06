using Application.Core.Game.TheWorld;

namespace net.server.task;


public class CharacterAutoSaveController : TimelyControllerBase
{
    readonly IWorldChannel worldChannel;
    public CharacterAutoSaveController(IWorldChannel world) : base("CharacterAutoSaveController", TimeSpan.FromHours(1), TimeSpan.FromHours(1))
    {
        worldChannel = world;
    }
    protected override void HandleRun()
    {
        if (!YamlConfig.config.server.USE_AUTOSAVE)
        {
            return;
        }

        var ps = worldChannel.getPlayerStorage();
        foreach (var chr in ps.getAllCharacters())
        {
            if (chr != null && chr.isLoggedin())
            {
                chr.saveCharToDB(false);
            }
        }
    }

}
