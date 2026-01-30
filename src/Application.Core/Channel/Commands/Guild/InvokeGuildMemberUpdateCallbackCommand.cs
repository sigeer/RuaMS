using GuildProto;
using net.server.guild;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildMemberUpdateCallbackCommand : IWorldChannelCommand
    {
        GuildMemberUpdateResponse res;

        public InvokeGuildMemberUpdateCallbackCommand(GuildMemberUpdateResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var guild in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(guild);
                if (chr != null)
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        if (res.Type == 0)
                        {
                            chr.sendPacket(PacketCreator.levelUpMessage(2, res.MemberLevel, res.MemberName));
                        }
                        else
                        {
                            chr.sendPacket(PacketCreator.jobMessage(0, res.MemberJob, res.MemberName));
                        }
                        chr.sendPacket(GuildPackets.guildMemberLevelJobUpdate(res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
                    }
                    chr.sendPacket(GuildPackets.updateAllianceJobLevel(res.AllianceId, res.GuildId, res.MemberId, res.MemberLevel, res.MemberJob));
                }
            }
        }
    }
}
