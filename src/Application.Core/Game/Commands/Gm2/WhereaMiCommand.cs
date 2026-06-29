using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class WhereaMiCommand : CommandBase
{
    public WhereaMiCommand() : base(2, "whereami")
    {
        Description = "Show info about objects on current map.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;


        var sb = new StringBuilder();
        sb.Append("我在...\r\n");
        sb.Append("地图ID：").Append(player.getMap().getId()).Append("\r\n");
        sb.Append("IsLargeMap：").Append(player.getMap().IsLargeMap).Append("\r\n");
        sb.Append("我的事件：").Append(player.getEventInstance()?.getName()).Append("\r\n");
        sb.Append("地图事件：").Append(player.getMap().getEventInstance()?.getName()).Append("\r\n");
        sb.Append("当前坐标：").Append(player.getPosition()).Append("\r\n");
        sb.Append("Foothold Id：").Append(player.getMap().FindFh(player.getPosition())).Append("\r\n");
        sb.Append("Stance：").Append(player.getStance()).Append("\r\n");

        sb.Append("=========Portals=========\r\n");
        sb.Append("由近到远：\r\n");
        var allPortals = player.getMap().SourceTemplate.Portals
            .OrderBy(x => new Point(x.nX, x.nY).distanceSq(player.getPosition())).ToArray();
        foreach (var portal in allPortals)
        {
            sb
                .Append("Id: ").Append(portal.nIndex).Append("\r\n")
                .Append("PortalType(pt): ").Append(portal.nPortalType).Append("\r\n")
                .Append("PortalName(pn): ").Append(portal.sPortalName).Append("\r\n")
                .Append("TargetName(tn): ").Append(portal.sTargetName).Append("\r\n")
                .Append("TargetMap(tm): ").Append(portal.nTargetMap).Append("\r\n")
                .Append("Script: ").Append(portal.Script).Append("\r\n");
        }

        var allMapObjects = player.getMap().getMapObjects()
            .OrderBy(x => x.getPosition().distanceSq(player.getPosition()))
            .GroupBy(x => x.getType());
        sb.Append("=========MapObject=========\r\n");
        sb.Append("地图上有：\r\n");
        foreach (var group in allMapObjects)
        {
            sb.Append(group.Key).Append("===>\r\n");

            foreach (var obj in group)
            {
                sb.Append(">> ")
                    .Append(obj.GetReadableName(c))
                    .Append(" - Id: ").Append(obj.GetSourceId())
                    .Append(" - Oid: ").Append(obj.getObjectId())
                    .Append(" - Position: ").Append(obj.getPosition());

                if (!player.MapModel.IsMapObjectVisibleForPlayerCached(player, obj))
                {
                    sb.Append("（超出视野）");
                }
                sb.Append("\r\n");
            }
        }

        await player.Dialog(sb.ToString());
    }
}
