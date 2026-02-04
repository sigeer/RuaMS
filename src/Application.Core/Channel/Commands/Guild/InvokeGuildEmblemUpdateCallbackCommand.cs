using Application.Shared.Team;
using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildEmblemUpdateCallbackCommand : IWorldChannelCommand
    {
        UpdateGuildEmblemResponse res;

        public InvokeGuildEmblemUpdateCallbackCommand(UpdateGuildEmblemResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var resCode = (GuildUpdateResult)res.Code;
            if (resCode != GuildUpdateResult.Success)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.MasterId);
                if (masterChr != null)
                {
                    // 失败回滚
                    masterChr.GainMeso(YamlConfig.config.server.CHANGE_EMBLEM_COST);
                }
                return;
            }

            var guildDto = res.AllianceDto.Guilds.FirstOrDefault(x => x.GuildId == res.GuildId);
            foreach (var memberId in res.AllMembers)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(memberId);
                if (chr != null)
                {
                    if (chr.GuildId == res.GuildId)
                    {
                        chr.sendPacket(GuildPackets.guildEmblemChange(res.GuildId, (short)res.Request.LogoBg, (byte)res.Request.LogoBgColor, (short)res.Request.Logo, (byte)res.Request.LogoColor));
                    }

                    if (res.AllianceDto != null)
                    {
                        chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    }

                }
            }
        }
    }
}
