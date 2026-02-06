using Application.Core.Models;
using Dto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeNewYearCardSendCommand : IWorldChannelCommand
    {
        SendNewYearCardResponse data;

        public InvokeNewYearCardSendCommand(SendNewYearCardResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (data.Code == 0)
            {
                var sender = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.FromId);
                if (sender != null)
                {
                    sender.GainItem(ItemId.NEW_YEARS_CARD, -1, show: GainItemShow.ShowInChat);
                    sender.GainItem(ItemId.NEW_YEARS_CARD_SEND, 1, show: GainItemShow.ShowInChat);

                    var model = ctx.WorldChannel.Mapper.Map<NewYearCardObject>(data.Model);
                    sender.addNewYearRecord(model);
                    sender.sendPacket(PacketCreator.onNewYearCardRes(sender, model, 4, 0));    // successfully sent
                }
            }
            else
            {
                var sender = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.FromId);
                if (sender != null)
                {
                    sender.sendPacket(PacketCreator.onNewYearCardRes(sender, null, 5, data.Code));
                }
            }
        }
    }
}
