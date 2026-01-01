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

    public override Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        if (paramsValue.Length < 2)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ProItemCommand_Syntax));
            return Task.CompletedTask;
        }


        if (!int.TryParse(paramsValue[0], out var itemId))
        {
            player.YellowMessageI18N(nameof(ClientMessage.ProItemCommand_Syntax));
            return Task.CompletedTask;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var abTemplate = ii.GetTemplate(itemId);
        if (abTemplate is not EquipTemplate equipTemplate)
        {
            player.YellowMessageI18N(nameof(ClientMessage.ItemNotFound), paramsValue[0]);
            return Task.CompletedTask;
        }

        if (!short.TryParse(paramsValue[1], out short stat))
        {
            player.YellowMessageI18N(nameof(ClientMessage.ProItemCommand_Syntax));
            return Task.CompletedTask;
        }

        stat = Math.Max((short)0, stat);
        short spdjmp = 0;

        if (paramsValue.Length >= 3 && short.TryParse(paramsValue[2], out spdjmp))
            spdjmp = Math.Max((short)0, spdjmp);

        var it = ii.GetEquipByTemplate(equipTemplate);
        if (it == null)
        {
            player.YellowMessageI18N(nameof(ClientMessage.EquipNotFound));
            return Task.CompletedTask;
        }
        it.setOwner(player.getName());

        ItemManager.SetEquipStat((Equip)it, stat, spdjmp);
        InventoryManipulator.addFromDrop(c, it);
        return Task.CompletedTask;
    }
}
