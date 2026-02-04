using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class LeaveAllianceCallbackCommands : IWorldChannelCommand
    {
        GuildLeaveAllianceResponse res;

        public LeaveAllianceCallbackCommands(GuildLeaveAllianceResponse res)
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
                    chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                    chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.AllianceDto.Notice));

                    chr.dropMessage("[" + res.GuildDto.Name + "] guild has left the union.");
                }
            }

            foreach (var guildMember in res.GuildDto.Members)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(guildMember.Id);
                if (chr != null)
                {
                    chr.sendPacket(GuildPackets.RemoveGuildFromAlliance(res.AllianceDto, res.GuildDto));

                    chr.sendPacket(GuildPackets.disbandAlliance(res.AllianceId));
                }
            }

            return;
        }
    }
}
