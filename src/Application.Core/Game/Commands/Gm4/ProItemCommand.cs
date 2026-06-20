using Application.Core.Channel.DataProviders;
using Application.Core.Managers;
using Application.Resources.Messages;
using Application.Templates.Character;
using client.inventory;
using client.inventory.manipulator;

namespace Application.Core.Game.Commands.Gm4;

public class ProItemCommand : CommandBase
{
    public ProItemCommand() : base(4, "proitem")
    {
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            await player.Yellow(nameof(ClientMessage.ProItemCommand_Syntax));
            return;
        }


        if (!int.TryParse(paramsValue[0], out var itemId))
        {
            await player.Yellow(nameof(ClientMessage.ProItemCommand_Syntax));
            return;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var abTemplate = ii.GetTemplate(itemId);
        if (abTemplate is not EquipTemplate equipTemplate)
        {
            await player.Yellow(nameof(ClientMessage.ItemNotFound), paramsValue[0]);
            return;
        }

        if (!short.TryParse(paramsValue[1], out short stat))
        {
            await player.Yellow(nameof(ClientMessage.ProItemCommand_Syntax));
            return;
        }

        stat = Math.Max((short)0, stat);
        short spdjmp = 0;

        if (paramsValue.Length >= 3 && short.TryParse(paramsValue[2], out spdjmp))
            spdjmp = Math.Max((short)0, spdjmp);

        var it = ii.GetEquipByTemplate(equipTemplate);
        it.setOwner(player.getName());

        ItemManager.SetEquipStat((Equip)it, stat, spdjmp);
        await InventoryManipulator.addFromDrop(c, it);
    }
}
