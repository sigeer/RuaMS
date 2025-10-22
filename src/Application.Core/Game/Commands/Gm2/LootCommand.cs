using Application.Core.Game.Maps;

namespace Application.Core.Game.Commands.Gm2;
public class LootCommand : CommandBase
{
    public LootCommand() : base(2, "loot")
    {
        Description = "Loots all items that belong to you.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        c.OnlinedCharacter.getMap().ProcessMapObject(x => x.getType() == MapObjectType.ITEM, o =>
        {
            if (o is MapItem mapItem)
            {
                if (mapItem.getOwnerId() == c.OnlinedCharacter.getId() || mapItem.getOwnerId() == c.OnlinedCharacter.getPartyId())
                {
                    c.OnlinedCharacter.pickupItem(mapItem);
                }
            }
        });
    }
}
