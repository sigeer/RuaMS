using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Items;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using client.inventory.manipulator;
using System.Text;

namespace Application.Core.Game.Commands.Gm2;

public class ItemCommand : CommandBase
{
    readonly WzStringQueryService _wzManager;
    public ItemCommand(WzStringQueryService wzManager) : base(2, "item")
    {
        _wzManager = wzManager;
    }

    public override async Task Execute(IChannelClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;

        if (paramsValue.Length < 1)
        {
            await player.Yellow(nameof(ClientMessage.ItemCommand_Syntax));
            return;
        }

        if (!int.TryParse(paramsValue[0], out var itemId))
        {
            var findResult = _wzManager.FindItemIdByName(c, paramsValue[0]);
            if (findResult.BestMatch != null)
                itemId = findResult.BestMatch.Id;
            else if (findResult.MatchedItems.Count > 0)
            {
                var messages = new StringBuilder("找到了这些相似项：\r\n");
                for (int i = 0; i < findResult.MatchedItems.Count; i++)
                {
                    var item = findResult.MatchedItems[i];
                    messages.Append($"\r\n#L{i}# {item.Id} #t{item.Id}# - {item.Name} #l");
                }

                await TempConversation.CreateScope(c, async ctx =>
                {
                    var idx = await ctx.AskMenu(messages.ToString());
                    var item = findResult.MatchedItems[idx];
                    if (await ctx.AskYesNo($"选择 {item.Id} #t{item.Id}# - {item.Name}？"))
                    {
                        await SendItem(c, item.Id, paramsValue);
                    }
                });
                return;
            }
            else
            {
                await player.Yellow(nameof(ClientMessage.ItemNotFound), paramsValue[0]);
                return;
            }
        }
        await SendItem(c, itemId, paramsValue);
    }

    private async Task SendItem(IChannelClient c, int itemId, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        if (YamlConfig.config.server.BLOCK_GENERATE_CASH_ITEM && ii.isCash(itemId))
        {
            await player.Yellow(nameof(ClientMessage.ItemCommand_CannotCreateCashItem));
            return;
        }

        short quantity = 1;
        if (paramsValue.Length >= 2)
        {
            quantity = short.Parse(paramsValue[1]);
        }


        var item = ii.GenerateVirtualItemById(itemId, quantity);
        if (item == null)
        {
            await player.Yellow(nameof(ClientMessage.ItemNotFound), itemId.ToString());
            return;
        }

        item.setExpiration(-1);
        if (item is Pet pet)
        {
            pet.setExpiration(c.CurrentServer.Node.GetCurrentTimeDateTimeOffset().AddDays(quantity).ToUnixTimeMilliseconds());
        }

        short flag = 0;
        if (player.gmLevel() < 3)
        {
            flag |= ItemConstants.ACCOUNT_SHARING;
            flag |= ItemConstants.UNTRADEABLE;
        }

        item.setFlag(flag);
        item.setOwner(player.getName());

        await InventoryManipulator.addFromDrop(c, item, false);
    }
}
