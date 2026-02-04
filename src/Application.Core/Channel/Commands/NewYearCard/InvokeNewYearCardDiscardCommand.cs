using Application.Core.Models;
using Dto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeNewYearCardDiscardCommand : IWorldChannelCommand
    {
        DiscardNewYearCardResponse data;

        public InvokeNewYearCardDiscardCommand(DiscardNewYearCardResponse data)
        {
            this.data = data;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            var cardList = ctx.WorldChannel.Mapper.Map<NewYearCardObject[]>(data.UpdateList);
            var chr = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);

            foreach (var item in cardList)
            {
                if (chr != null)
                {
                    chr.RemoveNewYearRecord(item.Id);
                    chr.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(chr, item, 0xE, 0));
                }

                var other = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.IsSender ? item.ReceiverId : item.SenderId);
                if (other != null)
                {
                    other.RemoveNewYearRecord(item.Id);
                    other.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(other, item, 0xE, 0));

                    other.dropMessage(6, "[New Year] " + (data.Request.IsSender ? item.SenderName : item.ReceiverName) + " threw away the New Year card.");
                }
            }
        }
    }
}
