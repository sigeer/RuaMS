using Application.Resources.Messages;
using client.inventory.manipulator;

namespace Application.Core.Game.Commands.Gm2;

public class ClearSlotCommand : CommandBase
{
    public ClearSlotCommand() : base(2, "clearslot")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.ClearSlotCommand_Syntax));
            return;
        }

        string type = paramsValue[0];
        switch (type)
        {
            case "all":
                await RemovePlayerSlot(c, InventoryType.EQUIP);
                await RemovePlayerSlot(c, InventoryType.USE);
                await RemovePlayerSlot(c, InventoryType.ETC);
                await RemovePlayerSlot(c, InventoryType.SETUP);
                await RemovePlayerSlot(c, InventoryType.CASH);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_AllCleared));
                break;
            case "equip":
                await RemovePlayerSlot(c, InventoryType.EQUIP);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_EquipCleared));
                break;
            case "use":
                await RemovePlayerSlot(c, InventoryType.USE);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_ConsumeCleared));
                break;
            case "setup":
                await RemovePlayerSlot(c, InventoryType.SETUP);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_InstallCleared));
                break;
            case "etc":
                await RemovePlayerSlot(c, InventoryType.ETC);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_EtcCleared));
                break;
            case "cash":
                await RemovePlayerSlot(c, InventoryType.CASH);
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_CashCleared));
                break;
            default:
                await player.Yellow(nameof(ClientMessage.ClearSlotCommand_Syntax));
                break;
        }
    }

    private async Task RemovePlayerSlot(IChannelClient c, InventoryType type)
    {
        bool isFromDrop = false;
        for (short i = 0; i < 101; i++)
        {
            var tempItem = c.OnlinedCharacter.getInventory(type).getItem(i);
            if (tempItem == null)
            {
                continue;
            }
            await InventoryManipulator.removeFromSlot(c, type, i, tempItem.getQuantity(), isFromDrop, false);
        }
    }
}
