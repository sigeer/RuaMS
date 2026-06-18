using Application.Core.Game.Maps;

namespace Application.Core.Game.Commands.Gm2;

public class LootCommand : CommandBase
{
    public LootCommand() : base(2, "loot")
    {
        Description = "Loots all items that belong to you.";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        await c.OnlinedCharacter.getMap().ProcessMapObject(x => x.getType() == MapObjectType.ITEM, async o =>
        {
            if (o is MapItem mapItem)
            {
                if (mapItem.getOwnerId() == c.OnlinedCharacter.getId() || mapItem.getOwnerId() == c.OnlinedCharacter.getPartyId())
                {
                    await c.OnlinedCharacter.pickupItem(mapItem);
                }
            }
        });
    }
}
