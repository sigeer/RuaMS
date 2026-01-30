using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class UpdateAllianceRankTitleCallbackCommand : IWorldChannelCommand
    {
        UpdateAllianceRankTitleResponse res;

        public UpdateAllianceRankTitleCallbackCommand(UpdateAllianceRankTitleResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                return;
            }

            foreach (var memberId in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.changeAllianceRankTitle(res.AllianceId, res.Request.RankTitles.ToArray()));
                }
            }
            return;
        }
    }
}
