using Application.Resources.Messages;
using client.inventory;

namespace Application.Core.Game.Commands.Gm4;

public class ItemExpCommand : ParamsCommandBase
{
    public ItemExpCommand() : base(["<slot>", "<exp>"], 4, "itemexp")
    {
        Description = "给装备栏中指定栏位的装备经验";
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.ProItemCommand_Syntax));
            return;
        }

        var itemSlot = GetIntParam("slot");
        var itemExp = GetIntParam("exp");

        var item = player.Bag[InventoryType.EQUIP].getItem((short)itemSlot);
        if (item == null || item is not Equip equip)
        {
            await player.Yellow("没有找到装备");
            return;
        }

        await equip.gainItemExp(c, itemExp);
    }
}
