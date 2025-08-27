using Application.Core.Game.Maps;
using Application.Core.Gameplay;

namespace Application.Core.Game.Commands.Gm4;

public class ForceVacCommand : CommandBase
{
    public ForceVacCommand() : base(4, "forcevac")
    {
        Description = "Loot all drops on the map.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var items = player.getMap().GetMapObjects(x => x.getType() == MapObjectType.ITEM);
        var p = new PlayerPickupProcessor(player);
        p.Flags = PickupCheckFlags.None;
        foreach (var item in items)
        {
            p.Handle(item as MapItem);
        }
    }
}
