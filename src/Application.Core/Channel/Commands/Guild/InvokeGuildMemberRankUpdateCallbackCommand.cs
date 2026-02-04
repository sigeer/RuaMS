using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildMemberRankUpdateCallbackCommand : IWorldChannelCommand
    {
        UpdateGuildMemberRankResponse res;

        public InvokeGuildMemberRankUpdateCallbackCommand(UpdateGuildMemberRankResponse res)
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
                    if (chr.Id == res.Request.TargetPlayerId)
                    {
                        chr.GuildRank = res.Request.NewRank;
                    }
                    chr.sendPacket(GuildPackets.changeRank(res.GuildId, res.Request.TargetPlayerId, res.Request.NewRank));
                }
            }
        }
    }
}
