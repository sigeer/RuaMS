using Application.Core.Channel.Internal.Handlers;
using Application.Shared.Team;
using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildMemberLeaveCallbackCommand : IWorldChannelCommand
    {
        LeaveGuildResponse res;

        public InvokeGuildMemberLeaveCallbackCommand(LeaveGuildResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var resCode = (GuildUpdateResult)res.Code;
            var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.PlayerId);
            if (resCode != GuildUpdateResult.Success)
            {
                if (masterChr != null)
                {
                    var msg = GuildHandlers.GetErrorMessage(resCode);
                    if (msg != null)
                    {
                        masterChr.dropMessage(1, msg);
                    }
                }
                return;
            }


            if (masterChr != null)
            {
                masterChr.sendPacket(GuildPackets.updateGP(res.GuildId, 0));
                masterChr.sendPacket(GuildPackets.ShowGuildInfo(null));

                masterChr.getMap().broadcastPacket(masterChr, GuildPackets.guildNameChanged(masterChr.Id, ""));
            }

            foreach (var memberId in res.AllLeftMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        chr.sendPacket(GuildPackets.memberLeft(res.GuildId, res.Request.PlayerId, res.MasterName, false));
                    }
                    else
                    {
                        if (res.AllianceDto != null)
                        {
                            chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.AllianceDto));
                            chr.sendPacket(GuildPackets.allianceNotice(res.AllianceDto.AllianceId, res.AllianceDto.Notice));
                        }
                    }

                }
            }
        }
    }
}
