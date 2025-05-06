using Application.Core.Managers;

namespace Application.Core.Game.Commands.Gm4;

public class PmobRemoveCommand : CommandBase
{
    public PmobRemoveCommand() : base(4, "pmobremove")
    {
        Description = "Remove all permanent mobs of the same type on the map.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        int mapId = player.getMapId();
        int mobId = paramsValue.Length > 0 ? int.Parse(paramsValue[0]) : -1;

        try
        {
            var removeResult = PLifeManager.RemovePMonster(mobId, player);
            if (removeResult > 0)
            {
                player.yellowMessage("Cleared " + removeResult + " pmob placements.");
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            player.dropMessage(5, "Failed to remove pmob from the database.");
        }
    }
}
