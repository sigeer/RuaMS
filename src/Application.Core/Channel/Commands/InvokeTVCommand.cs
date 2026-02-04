using ItemProto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeTVCommand : IWorldChannelCommand
    {
        CreateTVMessageBroadcast res;

        public InvokeTVCommand(CreateTVMessageBroadcast res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var noticeMsg = string.Join(" ", res.Request.MessageList);
            ctx.WorldChannel.broadcastPacket(PacketCreator.enableTV());
            ctx.WorldChannel.broadcastPacket(PacketCreator.sendTV(res.Master, res.Request.MessageList.ToArray(), res.Request.Type <= 2 ? res.Request.Type : res.Request.Type - 3, res.MasterPartner));

            if (res.Request.Type >= 3)
                ctx.WorldChannel.broadcastPacket(PacketCreator.serverNotice(3, res.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(res.Master) + " : " + noticeMsg, res.Request.ShowEar));

            return;
        }
    }

    internal class InvokeTVFinishCommand : IWorldChannelCommand
    {

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.broadcastPacket(PacketCreator.removeTV());

            return;
        }
    }
}
