using Application.Core.Channel.Net.Packets;
using DueyDto;

namespace Application.Core.Channel.Commands.Duey
{
    internal class InvokeDueyRemoveCallbackCommand : IWorldChannelCommand
    {
        RemovePackageResponse data;

        public InvokeDueyRemoveCallbackCommand(RemovePackageResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (data.Code == 0)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);
                if (chr != null)
                    chr.sendPacket(DueyPacketCreator.removeItemFromDuey(!data.Request.ByReceived, data.Request.PackageId));
            }
        }
    }
}
