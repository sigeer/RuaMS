using Application.Core.Channel.Internal.Handlers;
using Application.Shared.Team;
using GuildProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class InvokeGuildCreateCallbackCommand : IWorldChannelCommand
    {
        CreateGuildResponse res;

        public InvokeGuildCreateCallbackCommand(CreateGuildResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.LeaderId);
                if (masterChr != null)
                {
                    var msg = GuildHandlers.GetErrorMessage((GuildUpdateResult)res.Code);
                    if (msg != null)
                    {
                        masterChr.Popup(msg);
                    }
                    masterChr.GainMeso(YamlConfig.config.server.CREATE_GUILD_COST);
                }
                return;
            }

            foreach (var member in res.GuildDto.Members)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.Id);
                if (chr != null)
                {
                    if (res.GuildDto.Leader == chr.Id)
                    {
                        chr.GuildRank = 1;
                        chr.Popup("You have successfully created a Guild.");
                    }
                    else
                    {
                        chr.GuildRank = 2;
                        chr.Popup("You have successfully cofounded a Guild.");
                    }
                    chr.sendPacket(GuildPackets.ShowGuildInfo(res.GuildDto));

                    chr.getMap().broadcastPacket(chr, GuildPackets.guildNameChanged(chr.Id, res.GuildDto.Name));
                    chr.getMap().broadcastPacket(chr, GuildPackets.guildMarkChanged(chr.Id, res.GuildDto.LogoBg, res.GuildDto.LogoBgColor, res.GuildDto.Logo, res.GuildDto.LogoColor));
                }
            }
        }
    }
}
