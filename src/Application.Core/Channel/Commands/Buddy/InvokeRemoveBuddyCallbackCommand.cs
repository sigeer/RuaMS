using BuddyProto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeRemoveBuddyCallbackCommand : IWorldChannelCommand
    {
        DeleteBuddyResponse res;

        public InvokeRemoveBuddyCallbackCommand(DeleteBuddyResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.MasterId);
            if (masterChr != null)
            {
                masterChr.BuddyList.Remove(res.Buddyid);
                masterChr.sendPacket(PacketCreator.updateBuddylist(masterChr.BuddyList.getBuddies()));
            }

            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Buddyid);
            if (chr != null)
            {
                if (chr.BuddyList.Contains(res.MasterId))
                {
                    chr.sendPacket(PacketCreator.updateBuddyChannel(res.Buddyid, -1));
                }
            }
            return;
        }
    }
}
