using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands
{
    internal class ChangeAllianceLeaderCallbackCommand : IWorldChannelCommand
    {
        AllianceChangeLeaderResponse res;

        public ChangeAllianceLeaderCallbackCommand(AllianceChangeLeaderResponse res)
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
                    if (chr.Id == res.Request.MasterId)
                    {
                        chr.AllianceRank = 2;
                    }
                    else if (chr.Id == res.Request.PlayerId)
                    {
                        chr.AllianceRank = 1;
                    }

                    chr.sendPacket(GuildPackets.GetGuildAlliances(res.AllianceDto));
                    chr.dropMessage("'" + res.NewLeaderName + "' has been appointed as the new head of this Alliance.");
                }
            }
            return;
        }
    }
}
