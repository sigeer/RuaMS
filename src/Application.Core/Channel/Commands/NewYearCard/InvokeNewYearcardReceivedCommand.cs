using Application.Core.Models;
using Dto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class InvokeNewYearcardReceivedCommand : IWorldChannelCommand
    {
        ReceiveNewYearCardResponse data;

        public InvokeNewYearcardReceivedCommand(ReceiveNewYearCardResponse res)
        {
            this.data = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (data.Code == 0)
            {
                var newCard = ctx.WorldChannel.Mapper.Map<NewYearCardObject>(data.Model);

                var receiver = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);
                if (receiver != null)
                {
                    receiver.getAbstractPlayerInteraction().gainItem(ItemId.NEW_YEARS_CARD_RECEIVED, 1);
                    if (!string.IsNullOrEmpty(newCard.Message))
                    {
                        receiver.dropMessage(6, "[New Year] " + newCard.SenderName + ": " + newCard.Message);
                    }
                    receiver.addNewYearRecord(newCard);
                    receiver.sendPacket(PacketCreator.onNewYearCardRes(receiver, newCard, 6, 0));    // successfully rcvd

                    receiver.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(receiver, newCard, 0xD, 0));
                }


                var sender = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Model.SenderId);
                if (sender != null)
                {
                    sender.getMap().broadcastMessage(PacketCreator.onNewYearCardRes(sender, newCard, 0xD, 0));
                    sender.dropMessage(6, "[New Year] Your addressee successfully received the New Year card.");
                }
            }

            else
            {
                var receiver = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.MasterId);
                if (receiver != null)
                {
                    receiver.dropMessage(6, "[New Year] The sender of the New Year card already dropped it. Nothing to receive.");
                }
            }
            return;
        }
    }
}
