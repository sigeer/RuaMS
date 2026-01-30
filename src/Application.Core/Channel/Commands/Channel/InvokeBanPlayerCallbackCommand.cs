using Application.Resources.Messages;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using SystemProto;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeBanPlayerCallbackCommand : IWorldChannelCommand
    {
        BanResponse data;

        public InvokeBanPlayerCallbackCommand(BanResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.OperatorId);
            if (masterChr != null)
            {
                if (data.Code != 0)
                {
                    masterChr.sendPacket(PacketCreator.getGMEffect(6, 1));
                    return;
                }
                else
                {
                    masterChr.sendPacket(PacketCreator.getGMEffect(4, 0));
                }
            }


            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.VictimId);
            if (chr != null)
            {
                chr.Yellow(nameof(ClientMessage.Ban_NoticePlayer), data.OperatorName);
                chr.yellowMessage(chr.GetMessageByKey(ClientMessage.BanReason) + data.Request.ReasonDesc);

                Timer? timer = null;
                timer = new System.Threading.Timer(_ =>
                {
                    ctx.WorldChannel.Post(new InvokePlayerDisconnectCommand(data.VictimId));

                    timer?.Dispose();
                }, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
            }
        }
    }
}
