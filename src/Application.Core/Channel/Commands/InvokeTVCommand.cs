using ItemProto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeTVCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeTVCommand);
        CreateTVMessageBroadcast res;

        public InvokeTVCommand(CreateTVMessageBroadcast res)
        {
            this.res = res;
        }

        public async Task Execute(WorldChannel ctx)
        {
            var noticeMsg = string.Join(" ", res.Request.MessageList);
            await ctx.broadcastPacket(PacketCreator.enableTV());
            await ctx.broadcastPacket(PacketCreator.sendTV(res.Master, res.Request.MessageList.ToArray(), res.Request.Type <= 2 ? res.Request.Type : res.Request.Type - 3, res.MasterPartner));

            if (res.Request.Type >= 3)
                await ctx.broadcastPacket(PacketCreator.serverNotice(3, res.Master.Channel, CharacterViewDtoUtils.GetPlayerNameWithMedal(res.Master) + " : " + noticeMsg, res.Request.ShowEar));

            return;
        }
    }

    internal class InvokeTVFinishCommand : IWorldChannelAsyncCommand
    {
        public string Name => nameof(InvokeTVFinishCommand);
        public async Task Execute(WorldChannel ctx)
        {
            await ctx.broadcastPacket(PacketCreator.removeTV());

            return;
        }
    }
}
