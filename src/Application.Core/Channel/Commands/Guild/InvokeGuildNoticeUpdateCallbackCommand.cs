using Application.Shared.Team;
using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildNoticeUpdateCallbackCommand : IWorldChannelCommand
    {
        UpdateGuildNoticeResponse res;

        public InvokeGuildNoticeUpdateCallbackCommand(UpdateGuildNoticeResponse res)
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
                    chr.sendPacket(GuildPackets.guildNotice(res.GuildId, res.Request.Notice));
                }
            }
        }
    }
}
