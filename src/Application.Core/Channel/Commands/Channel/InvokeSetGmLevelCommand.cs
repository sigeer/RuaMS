using Application.Resources.Messages;
using SystemProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeSetGmLevelCommand : IWorldChannelCommand
    {
        SetGmLevelResponse res;

        public InvokeSetGmLevelCommand(SetGmLevelResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            Player? masterChr;
            if (res.Code != 0)
            {
                masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.OperatorId);
                if (masterChr != null)
                {
                    masterChr.Yellow(nameof(ClientMessage.PlayerNotFound), res.Request.TargetName);
                }
                return;
            }

            masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.OperatorId);
            if (masterChr != null)
            {
                masterChr.Yellow(nameof(ClientMessage.SetGmLevelCommand_Result), res.Request.TargetName, res.Request.Level.ToString());
            }

            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.TargetId);
            if (chr != null)
            {
                chr.Client.AccountEntity!.GMLevel = (sbyte)res.Request.Level;
                chr.Notice(nameof(ClientMessage.Notice_GmLevelChanged), res.Request.Level.ToString());
            }
        }
    }
}
