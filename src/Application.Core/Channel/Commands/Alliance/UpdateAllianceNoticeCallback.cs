using AllianceProto;
using Microsoft.AspNetCore.Hosting.Server;
using net.server.guild;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class UpdateAllianceNoticeCallback : IWorldChannelCommand
    {
        UpdateAllianceNoticeResponse res;

        public UpdateAllianceNoticeCallback(UpdateAllianceNoticeResponse res)
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
                    chr.sendPacket(GuildPackets.allianceNotice(res.AllianceId, res.Request.Notice));
                    chr.dropMessage(5, "* Alliance Notice : " + res.Request.Notice);
                }
            }

            return;
        }
    }
}
