using Application.Core.Models;
using Dto;
using net.packet.outs;

namespace Application.Core.Channel.Commands
{
    internal class InvokeNoteReceiveCommand : IWorldChannelCommand
    {
        SendNoteResponse res;

        public InvokeNoteReceiveCommand(SendNoteResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.ReceiverId);
            if (chr != null)
            {
                chr.sendPacket(new ShowNotesPacket(chr.Client, ctx.WorldChannel.Mapper.Map<List<NoteObject>>(res.List)));
            }
        }
    }
}
