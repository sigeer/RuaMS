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


        player.yellowMessage("地图ID：" + player.getMap().getId());
        player.yellowMessage("当前坐标：" + player.getPosition());

        foreach (var group in allMapObjects)
        {
            player.yellowMessage($"{group.Key} on this map:");

            foreach (var obj in group)
            {
                player.dropMessage(5, ">> " + obj.GetName() + " - Id: " + obj.GetSourceId() + " - Oid: " + obj.getObjectId());
            }
        }
    }
}
