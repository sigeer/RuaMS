using GuildProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildMemberServerChangeCallbackCommand : IWorldChannelCommand
    {
        GuildMemberServerChangedResponse res;

        public InvokeGuildMemberServerChangeCallbackCommand(GuildMemberServerChangedResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var guild in res.AllMembers)
            {
                if (guild != res.MemberId)
                {
                    var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(guild);
                    if (chr != null)
                    {
                        if (chr.GuildId == res.GuildId)
                        {
                            chr.sendPacket(GuildPackets.guildMemberOnline(res.GuildId, res.MemberId, res.MemberChanel > 0));
                        }
                        chr.sendPacket(GuildPackets.allianceMemberOnline(res.AllianceId, res.GuildId, res.MemberId, res.MemberChanel > 0));
                    }
                }
            }
        }
    }
}
