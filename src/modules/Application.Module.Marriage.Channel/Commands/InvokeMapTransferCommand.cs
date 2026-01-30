using Application.Core.Channel.Commands;
using Application.Module.Marriage.Channel.Net;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeMapTransferCommand : IWorldChannelCommand
    {
        PlayerTransferDto res;

        public InvokeMapTransferCommand(PlayerTransferDto res)
        {
            this.res = res;
        }

        public Task Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.ToPlayerId);
            if (chr != null)
            {
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(res.PlayerId, res.MapId));
            }
            return Task.CompletedTask;
        }
    }
}
