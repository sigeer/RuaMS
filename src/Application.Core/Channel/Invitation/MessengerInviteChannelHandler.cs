using Application.Core.Game.Players;
using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Invitation
{
    internal class MessengerInviteChannelHandler : InviteChannelHandler
    {
        public MessengerInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Messenger, logger)
        {
        }

        public override void OnInvitationAnswered(AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;
            if (result != InviteResultType.ACCEPTED)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                receiver?.message("Could not verify your Maple Messenger accept since the invitation rescinded.");

                if (result == InviteResultType.DENIED)
                {
                    var sender = _server.FindPlayerById(data.SenderPlayerId);
                    sender?.sendPacket(PacketCreator.messengerNote(data.ReceivePlayerName, 5, 0));
                }
            }
        }

        public override void OnInvitationCreated(CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.sendPacket(PacketCreator.messengerInvite(data.SenderPlayerName, data.Key));

                }

                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    sender.sendPacket(PacketCreator.messengerNote(data.ReceivePlayerName, 4, 1));
                }
            }
            else
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    switch (code)
                    {
                        case InviteResponseCode.ChatRoom_AlreadInRoom:
                            sender.sendPacket(PacketCreator.messengerChat(sender.Name + " : " + data.ReceivePlayerName + " is already using Maple Messenger"));
                            break;
                        case InviteResponseCode.MANAGING_INVITE:
                            sender.sendPacket(PacketCreator.messengerChat(sender.Name + " : " + data.ReceivePlayerName + " is already managing a Maple Messenger invitation"));
                            break;
                        case InviteResponseCode.InviteesNotFound:
                            sender.sendPacket(PacketCreator.messengerNote(data.ReceivePlayerName, 4, 0));
                            break;
                        case InviteResponseCode.ChatRoom_CapacityFull:
                            sender.sendPacket(PacketCreator.messengerChat(sender.Name + " : You cannot have more than 3 people in the Maple Messenger"));
                            break;
                        default:
                            _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", data.Type, code);
                            break;
                    }
                }
            }
        }
    }
}
