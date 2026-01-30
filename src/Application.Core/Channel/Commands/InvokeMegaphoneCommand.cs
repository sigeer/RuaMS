using client.inventory;
using ItemProto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeMegaphoneCommand : IWorldChannelCommand
    {
        UseItemMegaphoneBroadcast res;

        public InvokeMegaphoneCommand(UseItemMegaphoneBroadcast res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var p = PacketCreator.itemMegaphone(res.Request.Message, res.Request.IsWishper, res.MasterChannel, ctx.WorldChannel.Mapper.Map<Item>(res.Request.Item));
            ctx.WorldChannel.broadcastPacket(p);
            return;
        }
    }
}
