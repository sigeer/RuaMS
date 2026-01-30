using Application.Core.Channel.Commands;
using Application.Core.Game.Players;
using Application.Module.Marriage.Channel.Net;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Item;
using client.inventory;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeMarriageBrokenCommand : IWorldChannelCommand
    {
        BreakMarriageResponse data;

        public InvokeMarriageBrokenCommand(BreakMarriageResponse data)
        {
            this.data = data;
        }

        public Task Execute(ChannelCommandContext ctx)
        {
            if (data.Code != 0)
            {
                return Task.CompletedTask;
            }



            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);
            if (chr != null)
            {
                RemoveMarriageItems(chr);

                chr.dropMessage(5, "You have successfully break the " + (data.Type == 0 ? "engagement." : "marriage") + " with " + data.MasterPartnerName + ".");
                //chr.sendPacket(Wedding.OnMarriageResult((byte) 0));
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
            }

            var partner = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.MasterPartnerId);
            if (partner != null)
            {
                RemoveMarriageItems(partner);

                partner.dropMessage(5, data.MasterName + " has decided to break up the " + (data.Type == 0 ? "engagement." : "marriage."));
                //partner.sendPacket(Wedding.OnMarriageResult((byte) 0)); ok, how to gracefully unengage someone without the need to cc?
                partner.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(0, 0));
            }
            return Task.CompletedTask;
        }

        void RemoveMarriageItems(Player chr)
        {
            chr.Bag.BatchRemoveFromInventory([InventoryType.ETC], x => ItemId.GetEngagementItems().Contains(x.getItemId()), false);
            var marriageRing = chr.getMarriageRing();
            if (marriageRing != null)
            {
                var it = chr.getInventory(InventoryType.EQUIP).findById(marriageRing.getItemId()) ?? chr.getInventory(InventoryType.EQUIPPED).findById(marriageRing.getItemId());
                if (it != null)
                {
                    Equip eqp = (Equip)it;
                    eqp.ResetRing();
                }
            }

        }
    }
}
