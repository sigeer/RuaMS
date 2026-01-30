using Application.Resources.Messages;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class PlayerEnterCashShopPreCommand : IWorldChannelCommand
    {
        Player mc;

        public PlayerEnterCashShopPreCommand(Player chr)
        {
            mc = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (mc.cannotEnterCashShop())
            {
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getEventInstance() != null)
            {
                mc.Pink(nameof(ClientMessage.CashShop_CannotEnter_WithEventInstance));
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (MiniDungeonInfo.isDungeonMap(mc.getMapId()))
            {
                mc.Pink(nameof(ClientMessage.ChangeChannel_MiniDungeon));
                mc.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getCashShop().isOpened())
            {
                return;
            }

            _ = mc.SyncCharAsync(trigger: Shared.Events.SyncCharacterTrigger.EnterCashShop)
                    .ContinueWith(t =>
                    {
                        ctx.WorldChannel.Post(new PlayerEnterCashShopCommand(mc.Id));
                    });
        }
    }

    public class PlayerEnterCashShopCommand : IWorldChannelCommand
    {
        int _chrId;

        public PlayerEnterCashShopCommand(int chrId)
        {
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var mc = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (mc != null)
            {
                mc.closePlayerInteractions();
                mc.closePartySearchInteractions();

                mc.unregisterChairBuff();
                mc.Client.CurrentServer.NodeService.DataService.SaveBuff(mc);

                mc.Client.CurrentServer.EnterExtralWorld(mc);

                mc.cancelAllBuffs(true);
                mc.cancelAllDebuffs();
                mc.forfeitExpirableQuests();

                mc.StopPlayerTask();

                mc.sendPacket(PacketCreator.openCashShop(mc.Client, false));
                mc.sendPacket(PacketCreator.showCashInventory(mc.Client));
                mc.sendPacket(PacketCreator.showGifts(mc.Client.CurrentServer.NodeService.ItemService.LoadPlayerGifts(mc)));
                mc.sendPacket(PacketCreator.showWishList(mc, false));
                mc.sendPacket(PacketCreator.showCash(mc));

                mc.getCashShop().open(true);
            }
        }
    }
}
