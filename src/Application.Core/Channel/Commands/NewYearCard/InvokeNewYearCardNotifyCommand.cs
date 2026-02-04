using Application.Core.Models;
using Dto;
using Humanizer;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeNewYearCardNotifyCommand : IWorldChannelCommand
    {
        NewYearCardNotifyDto dto;

        public InvokeNewYearCardNotifyCommand(NewYearCardNotifyDto dto)
        {
            this.dto = dto;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            foreach (var item in dto.List)
            {
                var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(item.MasterId);
                if (chr != null)
                {
                    foreach (var obj in item.List)
                    {
                        chr.sendPacket(PacketCreator.onNewYearCardRes(chr, ctx.WorldChannel.Mapper.Map<NewYearCardObject>(obj), 0xC, 0));
                    }
                }
            }
        }
    }
}
