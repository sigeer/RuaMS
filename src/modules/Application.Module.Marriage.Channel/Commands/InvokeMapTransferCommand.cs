using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Module.Marriage.Channel.Net;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeMapTransferCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokeMapTransferCommand);

        PlayerTransferDto res;

        public InvokeMapTransferCommand(PlayerTransferDto res)
        {
            this.res = res;
        }

        public void Execute(WorldChannel ctx)
        {
            var chr = ctx.getPlayerStorage().GetCharacterClientById(res.ToPlayerId);
            if (chr != null)
            {
                chr.sendPacket(WeddingPackets.OnNotifyWeddingPartnerTransfer(res.PlayerId, res.MapId));
            }
        }
    }
}
