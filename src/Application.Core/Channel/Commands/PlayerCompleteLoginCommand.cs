using Dto;
using Google.Protobuf.Collections;
using net.server.guild;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class PlayerCompleteLoginCommand : IWorldChannelCommand
    {
        int _chrId;
        bool isNewCommer;
        RepeatedField<Dto.RemoteCallDto> remoteCallDtos;

        public PlayerCompleteLoginCommand(int chrId, bool isNewCommer, RepeatedField<RemoteCallDto> remoteCallDtos)
        {
            _chrId = chrId;
            this.isNewCommer = isNewCommer;
            this.remoteCallDtos = remoteCallDtos;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(_chrId);
            if (chr != null)
            {
                if (isNewCommer)
                {
                    chr.setLoginTime(ctx.WorldChannel.Node.GetCurrentTimeDateTimeOffset());
                    chr.sendPacket(PacketCreator.SyncHpMpAlert(chr.HpAlert, chr.MpAlert));
                }

                var guild = chr.GetGuild();
                if (guild != null)
                {
                    chr.sendPacket(GuildPackets.ShowGuildInfo(guild));
                }

                var alliance = chr.GetAlliance();
                if (alliance != null)
                {
                    chr.sendPacket(GuildPackets.UpdateAllianceInfo(alliance));
                    chr.sendPacket(GuildPackets.allianceNotice(alliance.AllianceId, alliance.Notice));
                }

                ctx.WorldChannel.NodeService.RemoteCallService.RunEventAfterLogin(chr, remoteCallDtos);

                chr.CheckJail();
            }
        }
    }
}
