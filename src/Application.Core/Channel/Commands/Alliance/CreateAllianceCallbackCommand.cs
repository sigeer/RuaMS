using AllianceProto;
using net.server.guild;

namespace Application.Core.Channel.Commands.Alliance
{
    internal class CreateAllianceCallbackCommand : IWorldChannelCommand
    {
        CreateAllianceResponse res;

        public CreateAllianceCallbackCommand(CreateAllianceResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.Members[0]);
                if (masterChr != null)
                {
                    masterChr.GainMeso(res.Request.Cost);

                    masterChr.Client.NPCConversationManager?.sendOk("请检查一下你和另一个公会领袖是否都在这个房间里，确保两个公会目前都没有在联盟中注册。在这个过程中，除了你们两个，不应该有其他公会领袖在场。");
                    masterChr.Client.NPCConversationManager?.dispose();
                }
            }

            foreach (var member in res.Model.Guilds.SelectMany(x => x.Members))
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(member.Id);
                if (chr != null)
                {
                    chr.AllianceRank = member.AllianceRank;

                    chr.sendPacket(GuildPackets.UpdateAllianceInfo(res.Model));
                    // UpdateAllianceInfo有完整数据，这个包是否有必要？
                    chr.sendPacket(GuildPackets.allianceNotice(res.Model.AllianceId, res.Model.Notice));

                    if (chr.Id == res.Request.Members[0])
                    {
                        chr.Client.NPCConversationManager?.sendOk("已成功组建了家族联盟。");
                        chr.Client.NPCConversationManager?.dispose();
                    }
                }
            }
        }
    }
}
