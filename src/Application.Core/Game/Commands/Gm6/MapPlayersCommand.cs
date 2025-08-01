using net.server;

namespace Application.Core.Game.Commands.Gm6;

public class MapPlayersCommand : CommandBase
{
    public MapPlayersCommand() : base(6, "mapplayers")
    {
        Description = "Show all players on the map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        string names = "";
        int map = player.getMapId();

        foreach (var chr in player.getMap().getPlayers().OfType<IPlayer>())
        {
            int curMap = chr.getMapId();
            string hp = chr.HP.ToString();
            string maxhp = chr.ActualMaxHP.ToString();
            string name = chr.getName() + ": " + hp + "/" + maxhp;
        }
        player.message("Players on mapid " + map + ": " + names);
    }
}
