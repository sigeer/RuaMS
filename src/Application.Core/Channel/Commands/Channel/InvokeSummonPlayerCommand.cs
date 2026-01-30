using Application.Resources.Messages;
using SystemProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeSummonPlayerCommand : IWorldChannelCommand
    {
        SummonPlayerByNameResponse res;

        public InvokeSummonPlayerCommand(SummonPlayerByNameResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.MasterId);
                if (chr != null)
                {
                    chr.Yellow(nameof(ClientMessage.PlayerNotOnlined), res.Request.Victim);
                }
                return;
            }

            var summoned = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.VictimId);
            if (summoned != null)
            {
                if (summoned.getEventInstance() == null)
                {
                    ctx.WorldChannel.NodeService.AdminService.WarpPlayerByName(summoned, res.WarpToName);
                }
            }
        }
    }
}
