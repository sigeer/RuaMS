using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class JoinAllianceCallbackCommand : IWorldChannelCommand
    {
        GuildJoinAllianceResponse res;

        public JoinAllianceCallbackCommand(GuildJoinAllianceResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                return;
            }

            foreach (var member in res.AllianceDto.Guilds.SelectMany(x => x.Members))
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.Id);
                if (chr != null)
                {
                    // 似乎会被updateAllianceInfo覆盖
                    // chr.sendPacket(GuildPackets.addGuildToAlliance(res.AllianceDto, r));

                    chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                    // UpdateAllianceInfo有完整数据，这个包是否有必要？
                    chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));

                    if (chr.GuildId == res.GuildId)
                    {
                        chr.dropMessage("Your guild has joined the [" + res.AllianceDto.Name + "] union.");
                    }

                    chr.AllianceRank = 5;
                    if (chr.GuildRank == 1)
                        chr.AllianceRank = 2;
                }
            }
            return;
        }
    }
}
