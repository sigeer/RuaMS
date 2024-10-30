using Application.Core.Game.Maps;
using Application.Core.Managers;
using client.inventory.manipulator;
using constants.id;
using server.maps;
using tools;

namespace Application.Core.Game.Commands.Gm4;

/// <summary>
/// 和ItemVacCommand什么区别？
/// </summary>
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
        foreach (var item in items)
        {
            MapItem mapItem = (MapItem)item;
            mapItem.lockItem();
            try
            {
                if (mapItem.isPickedUp())
                {
                    continue;
                }

                if (mapItem.getMeso() > 0)
                {
                    player.gainMeso(mapItem.getMeso(), true);
                }
                else if (player.applyConsumeOnPickup(mapItem.getItemId()))
                {    // thanks Vcoc for pointing out consumables on pickup not being processed here
                }
                else if (ItemId.isNxCard(mapItem.getItemId()))
                {
                    // Add NX to account, show effect and make item disappear
                    player.getCashShop().gainCash(1, mapItem.getItemId() == ItemId.NX_CARD_100 ? 100 : 250);
                }
                else if (ItemId.isPet(mapItem.getItemId()))
                {
                    int petId = ItemManager.CreatePet(mapItem.getItemId());
                    if (petId == -1)
                    {
                        continue;
                    }
                    InventoryManipulator.addById(c, mapItem.getItem().getItemId(), mapItem.getItem().getQuantity(), null, petId);
                }
                else if (InventoryManipulator.addFromDrop(c, mapItem.getItem(), true))
                {
                    if (mapItem.getItemId() == ItemId.ARPQ_SPIRIT_JEWEL)
                    {
                        player.updateAriantScore();
                    }
                }

                player.getMap().pickItemDrop(PacketCreator.removeItemFromMap(mapItem.getObjectId(), 2, player.getId()), mapItem);
            }
            finally
            {
                mapItem.unlockItem();
            }
        }
    }
}