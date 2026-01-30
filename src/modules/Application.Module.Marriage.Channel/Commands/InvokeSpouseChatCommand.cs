using Application.Core.Channel.Commands;
using Application.Module.Marriage.Channel.Net;
using MarriageProto;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using XmlWzReader;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeSpouseChatCommand : IWorldChannelCommand
    {
        SendSpouseChatResponse data;

        public InvokeSpouseChatCommand(SendSpouseChatResponse data)
        {
            this.data = data;
        }

        public Task Execute(ChannelCommandContext ctx)
        {
            var sender = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.Request.SenderId);
            if (sender != null)
            {
                if (data.Code == 1)
                {
                    sender.dropMessage(5, "You don't have a spouse.");
                    return Task.CompletedTask;
                }

                if (data.Code == 2)
                {
                    sender.dropMessage(5, "Your spouse is currently offline.");
                    return Task.CompletedTask;
                }

                sender.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Request.Text, true));
            }

            if (data.Code == 0)
            {
                var receiver = ctx.WorldChannel.getPlayerStorage().getCharacterById(data.SenderPartnerId);
                if (receiver != null)
                {
                    receiver.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Request.Text, true));
                }
            }
            return Task.CompletedTask;
        }
    }
}
