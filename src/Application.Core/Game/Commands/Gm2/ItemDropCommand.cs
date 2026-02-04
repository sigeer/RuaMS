using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Resources.Messages;
using Application.Templates.Item.Pet;
using client.inventory;

namespace Application.Core.Game.Commands.Gm2;

public class ItemDropCommand : CommandBase
{
    public ItemDropCommand() : base(2, "drop")
    {
    }

    public override void Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ItemDropCommand_Syntax));
            return;
        }

        int itemId = int.Parse(paramsValue[0]);
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        short quantity = 1;
        if (paramsValue.Length >= 2)
        {
            quantity = short.Parse(paramsValue[1]);
        }

        if (YamlConfig.config.server.BLOCK_GENERATE_CASH_ITEM && ii.isCash(itemId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.ItemDropCommand_CannotCreateCashItem));
            return;
        }

        var item = ii.GenerateVirtualItemById(itemId, quantity);
        if (item == null)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ItemNotFound), itemId.ToString());
            return;
        }

        if (item is Pet pet)
        {
            pet.setExpiration(c.CurrentServer.Node.GetCurrentTimeDateTimeOffset().AddDays(quantity).ToUnixTimeMilliseconds());
        }

        if (player.gmLevel() < 3)
        {
            short f = item.getFlag();
            f |= ItemConstants.ACCOUNT_SHARING;
            f |= ItemConstants.UNTRADEABLE;
            f |= ItemConstants.SANDBOX;

            item.setFlag(f);
            item.setOwner("TRIAL-MODE");
        }

        c.OnlinedCharacter.getMap().spawnItemDrop(c.OnlinedCharacter, c.OnlinedCharacter, item, c.OnlinedCharacter.getPosition(), true, true);
    }
}
