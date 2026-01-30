using MessageProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeWhisperCommand : IWorldChannelCommand
    {
        SendWhisperMessageResponse res;

        public InvokeWhisperCommand(SendWhisperMessageResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (res.Code != 0)
            {
                var masterChr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.Request.FromId);
                if (masterChr != null)
                {
                    masterChr.sendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, false));
                    return;
                }
            }

            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(res.ReceiverId);
            if (chr != null)
            {
                if (res.Code == 0)
                {
                    chr.sendPacket(PacketCreator.getWhisperResult(res.Request.TargetName, true));
                    chr.sendPacket(PacketCreator.getWhisperReceive(res.FromName, res.FromChannel - 1, res.IsFromGM, res.Request.Text));
                }
            }

        }
    }
}
