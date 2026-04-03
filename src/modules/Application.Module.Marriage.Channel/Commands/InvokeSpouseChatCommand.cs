using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Module.Marriage.Channel.Net;
using MarriageProto;

namespace Application.Module.Marriage.Channel.Commands
{
    internal class InvokeSpouseChatCommand : IWorldChannelCommand
    {
        public string Name => nameof(InvokeSpouseChatCommand);

        SendSpouseChatResponse data;

        public InvokeSpouseChatCommand(SendSpouseChatResponse data)
        {
            this.data = data;
        }

        public void Execute(WorldChannel ctx)
        {
            var sender = ctx.getPlayerStorage().GetCharacterClientById(data.Request.SenderId);
            if (sender != null)
            {
                if (data.Code == 1)
                {
                    sender.TypedMessage(5, "You don't have a spouse.");
                    return;
                }

                if (data.Code == 2)
                {
                    sender.TypedMessage(5, "Your spouse is currently offline.");
                    return;
                }

                sender.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Request.Text, true));
            }

            if (data.Code == 0)
            {
                var receiver = ctx.getPlayerStorage().GetCharacterClientById(data.SenderPartnerId);
                if (receiver != null)
                {
                    receiver.sendPacket(WeddingPackets.OnCoupleMessage(data.SenderName, data.Request.Text, true));
                }
            }
        }
    }
}
