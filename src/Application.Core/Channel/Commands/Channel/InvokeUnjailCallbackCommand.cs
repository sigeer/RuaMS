using Application.Resources.Messages;
using JailProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeUnjailCallbackCommand : IWorldChannelCommand
    {
        CreateUnjailResponse res;

        public InvokeUnjailCallbackCommand(CreateUnjailResponse res)
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
                    if (res.Code == 1)
                    {
                        masterChr.Pink(nameof(ClientMessage.PlayerNotFound));
                    }
                    else if (res.Code == 2)
                    {
                        masterChr.Pink(nameof(ClientMessage.UnjailCommand_AlreadyFree));
                    }
                }
                return;
            }

            var targetChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.TargetId);
            if (targetChr != null)
            {
                targetChr.removeJailExpirationTime();
                targetChr.Pink(nameof(ClientMessage.Unjail_Notify));
            }

            if (masterChr != null)
            {
                masterChr.Yellow(nameof(ClientMessage.Command_Done), masterChr.getLastCommandMessage());
            }
        }
    }
}
