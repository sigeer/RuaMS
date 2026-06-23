using Application.Core.Channel.DataProviders;
using client;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author Xari
 * @author Ronan - added concurrency protection and quest progress limit
 */
public class RaiseIncExpHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        sbyte inventorytype = p.ReadSByte();//nItemIT
        short slot = p.readShort();//nSlotPosition
        int itemid = p.readInt();//nItemID

        {
            await c.tryacquireClient();
            try
            {
                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                var consItem = ii.getQuestConsumablesInfo(itemid);
                if (consItem == null)
                {
                    return;
                }

                int infoNumber = consItem.questid;
                Dictionary<int, int> consumables = consItem.items;

                var chr = c.OnlinedCharacter;
                var quest = QuestFactory.Instance.GetInstanceFromInfoNumber(infoNumber);
                if (!chr.getQuest(quest).getStatus().Equals(QuestStatus.Status.STARTED))
                {
                    await c.SendPacket(PacketCreator.enableActions());
                    return;
                }

                int consId;
                var inv = chr.getInventory(InventoryTypeUtils.getByType(inventorytype));

                consId = inv.getItem(slot)!.getItemId();
                if (!consumables.ContainsKey(consId) || !chr.haveItem(consId))
                {
                    return;
                }

                await InventoryManipulator.removeFromSlot(c, InventoryTypeUtils.getByType(inventorytype), slot, 1, false, true);

                int questid = quest.getId();
                int nextValue = Math.Min(consumables.GetValueOrDefault(consId) + c.OnlinedCharacter.getAbstractPlayerInteraction().getQuestProgressInt(questid, infoNumber), consItem.exp * consItem.grade);
                await c.OnlinedCharacter.getAbstractPlayerInteraction().setQuestProgress(questid, infoNumber, nextValue);

                await c.SendPacket(PacketCreator.enableActions());
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
