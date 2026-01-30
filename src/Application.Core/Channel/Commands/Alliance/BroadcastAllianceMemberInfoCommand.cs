using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class BroadcastAllianceMemberInfoCommand : IWorldChannelCommand
    {
        AllianceBroadcastPlayerInfoResponse res;

        public BroadcastAllianceMemberInfoCommand(AllianceBroadcastPlayerInfoResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                return;
            }

            foreach (var item in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(item);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.sendShowInfo(res.AllianceId, res.Request.MasterId));
                }
            }
            return;
        }
    }
}
