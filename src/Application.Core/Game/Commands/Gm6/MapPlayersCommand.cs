using Application.Resources.Messages;
using System.Text;

namespace Application.Core.Game.Commands.Gm6;

public class MapPlayersCommand : CommandBase
{
    public MapPlayersCommand() : base(6, "mapplayers")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int map = player.getMapId();

        var sb = new StringBuilder();
        sb.Append(c.CurrentCulture.GetMessageByKey(nameof(ClientMessage.CurrentMap), player.MapModel.getMapName())).Append(":\r\n");
        foreach (var chr in player.getMap().getAllPlayers())
        {
            string hp = chr.HP.ToString();
            string maxhp = chr.ActualMaxHP.ToString();
            string name = chr.getName() + ": " + hp + "/" + maxhp;
            sb.Append(name).Append("\r\n");
        }
        await player.Dialog(sb.ToString());
    }
}
