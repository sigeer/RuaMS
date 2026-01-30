using Application.Resources.Messages;
using JailProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeJailCallbackCommand : IWorldChannelCommand
    {
        CreateJailResponse res;

        public InvokeJailCallbackCommand(CreateJailResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.MasterId);
            if (res.Code != 0)
            {
                if (masterChr != null)
                {
                    masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                }
                return;
            }

            var targetChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.TargetId);
            if (targetChr != null)
            {
                targetChr.addJailExpirationTime(res.Request.Minutes * 60000);
            }

            if (masterChr != null)
            {
                if (res.IsExtend)
                    masterChr.Pink(nameof(ClientMessage.Jail_ExtendResult), res.Request.TargetName, res.Request.Minutes.ToString());
                else
                    masterChr.Pink(nameof(ClientMessage.Jail_Result), res.Request.TargetName, res.Request.Minutes.ToString());
            }
        }
    }
}
