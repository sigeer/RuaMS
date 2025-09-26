using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Templates.Item.Pet;
using client.inventory;

namespace Application.Core.Game.Commands.Gm2;

public class ItemDropCommand : CommandBase
{
    public ItemDropCommand() : base(2, "drop")
    {
        Description = "Spawn an item onto the ground.";
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !drop <itemid> <quantity>");
            return;
        }

        int itemId = int.Parse(paramsValue[0]);
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        var abTemplate = ii.GetTemplate(itemId);
        if (abTemplate == null)
        {
            player.yellowMessage("Item id '" + paramsValue[0] + "' does not exist.");
            return;
        }

        short quantity = 1;
        if (paramsValue.Length >= 2)
        {
            quantity = short.Parse(paramsValue[1]);
        }

        if (YamlConfig.config.server.BLOCK_GENERATE_CASH_ITEM && ii.isCash(itemId))
        {
            player.yellowMessage("You cannot create a cash item with this command.");
            return;
        }

        if (abTemplate is PetItemTemplate petTemplate)
        {
            if (paramsValue.Length >= 2)
            {
                // thanks to istreety & TacoBell
                quantity = 1;
                long days = Math.Max(1, int.Parse(paramsValue[1]));
                long expiration = c.CurrentServerContainer.GetCurrentTimeDateTimeOffSet().AddDays(days).ToUnixTimeMilliseconds();

                var toDropTemp = new Pet(itemId, 0, Yitter.IdGenerator.YitIdHelper.NextId());
                toDropTemp.setExpiration(expiration);

                toDropTemp.setOwner("");
                if (player.gmLevel() < 3)
                {
                    short f = toDropTemp.getFlag();
                    f |= ItemConstants.ACCOUNT_SHARING;
                    f |= ItemConstants.UNTRADEABLE;
                    f |= ItemConstants.SANDBOX;

                    toDropTemp.setFlag(f);
                    toDropTemp.setOwner("TRIAL-MODE");
                }

                c.OnlinedCharacter.getMap().spawnItemDrop(c.OnlinedCharacter, c.OnlinedCharacter, toDropTemp, c.OnlinedCharacter.getPosition(), true, true);

                return;
            }
            else
            {
                player.yellowMessage("Pet Syntax: !drop <itemid> <expiration>");
                return;
            }
        }

        Item toDrop;
        if (ItemConstants.getInventoryType(itemId) == InventoryType.EQUIP)
        {
            toDrop = ii.getEquipById(itemId);
        }
        else
        {
            toDrop = Item.CreateVirtualItem(itemId, quantity);
        }

        toDrop.setOwner(player.getName());
        if (player.gmLevel() < 3)
        {
            short f = toDrop.getFlag();
            f |= ItemConstants.ACCOUNT_SHARING;
            f |= ItemConstants.UNTRADEABLE;
            f |= ItemConstants.SANDBOX;

            toDrop.setFlag(f);
            toDrop.setOwner("TRIAL-MODE");
        }

        c.OnlinedCharacter.getMap().spawnItemDrop(c.OnlinedCharacter, c.OnlinedCharacter, toDrop, c.OnlinedCharacter.getPosition(), true, true);
    }
}
