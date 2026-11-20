using Application.Core.Client.inventory;
using client.inventory;
using client.inventory.manipulator;
using System.Diagnostics.CodeAnalysis;

namespace Application.Core.Channel.Net.Handlers
{
    public abstract class UseItemHandlerBase : ChannelHandlerBase
    {
        protected bool TryGetItem(Player chr, short slot, int itemId, int useCount, [MaybeNullWhen(false)] out Item invItem)
        {
            var invType = ItemConstants.getInventoryType(itemId);
            invItem = chr.getInventory(invType).getItem(slot);
            if (invItem == null
                || invItem.getItemId() != itemId
                || chr.getInventory(invType).countById(itemId) < useCount)
            {
                invItem = null;
                return false;
            }

            return true;
        }
        /// <summary>
        /// 只要消耗了这个id的物品即可
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="invItem"></param>
        /// <returns></returns>
        protected async Task RemoveItem(Player chr, Item invItem)
        {
            if (invItem.Store is AbstractInventory inv)
            {
                await InventoryManipulator.removeById(chr.Client, inv.getType(), invItem.getItemId(), 1, false, false);
            }

        }
        /// <summary>
        /// 必须要消耗指定的物品
        /// </summary>
        /// <param name="chr"></param>
        /// <param name="invItem"></param>
        /// <returns></returns>
        protected async Task RemoveItemStrictly(Player chr, Item invItem)
        {
            if (invItem.Store is AbstractInventory inv)
            {
                await InventoryManipulator.removeFromSlot(chr.Client, inv.getType(), invItem.getPosition(), 1, false, false);
            }
        }
    }
}
