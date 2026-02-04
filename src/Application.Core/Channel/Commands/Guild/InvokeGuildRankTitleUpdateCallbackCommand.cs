using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildRankTitleUpdateCallbackCommand : IWorldChannelCommand
    {
        UpdateGuildRankTitleResponse res;

        public InvokeGuildRankTitleUpdateCallbackCommand(UpdateGuildRankTitleResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var memberId in res.GuildMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.rankTitleChange(res.GuildId, res.Request.RankTitles.ToArray()));
                }
            }
        }
    }
}
