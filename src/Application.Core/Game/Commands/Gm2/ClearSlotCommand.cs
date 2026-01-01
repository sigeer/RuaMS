using Application.Resources.Messages;
using client.inventory.manipulator;

namespace Application.Core.Game.Commands.Gm2;

public class ClearSlotCommand : CommandBase
{
    public ClearSlotCommand() : base(2, "clearslot")
    {
    }

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 1)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_Syntax));
            return Task.CompletedTask;
        }

        string type = paramsValue[0];
        switch (type)
        {
            case "all":
                RemovePlayerSlot(c, InventoryType.EQUIP);
                RemovePlayerSlot(c, InventoryType.USE);
                RemovePlayerSlot(c, InventoryType.ETC);
                RemovePlayerSlot(c, InventoryType.SETUP);
                RemovePlayerSlot(c, InventoryType.CASH);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_AllCleared));
                break;
            case "equip":
                RemovePlayerSlot(c, InventoryType.EQUIP);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_EquipCleared));
                break;
            case "use":
                RemovePlayerSlot(c, InventoryType.USE);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_ConsumeCleared));
                break;
            case "setup":
                RemovePlayerSlot(c, InventoryType.SETUP);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_InstallCleared));
                break;
            case "etc":
                RemovePlayerSlot(c, InventoryType.ETC);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_EtcCleared));
                break;
            case "cash":
                RemovePlayerSlot(c, InventoryType.CASH);
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_CashCleared));
                break;
            default:
                player.YellowMessageI18N(nameof(ClientMessage.ClearSlotCommand_Syntax));
                break;
        }
        return Task.CompletedTask;
    }

    private void RemovePlayerSlot(IChannelClient c, InventoryType type)
    {
        bool isFromDrop = false;
        for (short i = 0; i < 101; i++)
        {
            var tempItem = c.OnlinedCharacter.getInventory(type).getItem(i);
            if (tempItem == null)
            {
                continue;
            }
            InventoryManipulator.removeFromSlot(c, type, i, tempItem.getQuantity(), isFromDrop, false);
        }
    }
}
