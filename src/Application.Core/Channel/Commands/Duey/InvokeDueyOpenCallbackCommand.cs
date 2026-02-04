using Application.Core.Channel.DueyService;
using Application.Core.Channel.Net.Packets;
using DueyDto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Duey
{
    internal class InvokeDueyOpenCallbackCommand : IWorldChannelCommand
    {
        GetPlayerDueyPackageResponse data;

        public InvokeDueyOpenCallbackCommand(GetPlayerDueyPackageResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.ReceiverId);
            if (chr != null)
            {
                var dataList = ctx.WorldChannel.Mapper.Map<DueyPackageObject[]>(data.List);
                chr.sendPacket(DueyPacketCreator.sendDuey(DueyProcessorActions.TOCLIENT_OPEN_DUEY.getCode(), dataList));
            }
        }
    }
}
