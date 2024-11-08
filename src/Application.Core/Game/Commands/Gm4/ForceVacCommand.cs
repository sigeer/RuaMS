using Application.Core.Game.Maps;
using Application.Core.Gameplay;
using Application.Core.Managers;
using client.inventory.manipulator;
using constants.id;
using server.maps;
using tools;

namespace Application.Core.Game.Commands.Gm4;

public class ForceVacCommand : CommandBase
{
    public ForceVacCommand() : base(4, "forcevac")
    {
        Description = "Loot all drops on the map.";
    }

    public override void Execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        var items = player.getMap().getMapObjectsInRange(player.getPosition(), double.PositiveInfinity, Arrays.asList(MapObjectType.ITEM));
        var p = new PlayerPickupProcessor(player);
        p.Flags = PickupCheckFlags.None;
        foreach (var item in items)
        {
            p.Handle(item as MapItem);
        }
    }
}
