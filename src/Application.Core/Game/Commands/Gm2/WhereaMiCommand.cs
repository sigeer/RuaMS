using Application.Core.scripting.npc;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class WhereaMiCommand : CommandBase
{
    public WhereaMiCommand() : base(2, "whereami")
    {
        Description = "Show info about objects on current map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        var allMapObjects = player.getMap().getMapObjects().GroupBy(x => x.getType());

        var sb = new StringBuilder();
        sb.Append("我在...\r\n");
        sb.Append("地图ID：").Append(player.getMap().getId()).Append("\r\n");
        sb.Append("当前坐标：").Append(player.getPosition()).Append("\r\n");
        sb.Append("地图上有：\r\n");
        foreach (var group in allMapObjects)
        {
            sb.Append(group.Key).Append("===>\r\n");

            foreach (var obj in group)
            {
                sb.Append(">> ").Append(obj.GetName()).Append(" - Id: ").Append(obj.GetSourceId()).Append(" - Oid: ").Append(obj.getObjectId()).Append("\r\n");
            }
        }

        TempConversation.Create(c)?.RegisterTalk(sb.ToString());
    }
}
