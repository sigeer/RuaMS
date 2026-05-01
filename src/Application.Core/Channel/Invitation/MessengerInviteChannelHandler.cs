using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Invitation
{
    internal class MessengerInviteChannelHandler : InviteChannelHandler
    {
        public MessengerInviteChannelHandler(WorldChannel server, ILogger<InviteChannelHandler> logger) : base(server, InviteTypes.Messenger, logger)
        {
        }

        public override void OnInvitationAnswered(InvitationProto.AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;
            if (result != InviteResultType.ACCEPTED)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                receiverActor?.Send(m =>
                {
                    m.getCharacterById(data.ReceivePlayerId)?.Pink("Could not verify your Maple Messenger accept since the invitation rescinded.");
                });

                if (result == InviteResultType.DENIED)
                {
                    var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                    senderActor?.Send(m =>
                    {
                        m.getCharacterById(data.SenderPlayerId)?.sendPacket(PacketCreator.messengerNote(data.ReceivePlayerName, 5, 0));
                    });
                }
            }
        }

        public override void OnInvitationCreated(InvitationProto.CreateInviteResponse data)
        {
            var code = (InviteResponseCode)data.Code;
            if (code == InviteResponseCode.Success)
            {
                var receiverActor = _server.getPlayerStorage().GetCharacterActor(data.ReceivePlayerId);
                receiverActor?.Send(m =>
                {
                    m.getCharacterById(data.ReceivePlayerId)?.sendPacket(PacketCreator.messengerInvite(data.SenderPlayerName, data.Key));
                });

                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                senderActor?.Send(m =>
                {
                    m.getCharacterById(data.SenderPlayerId)?.sendPacket(PacketCreator.messengerNote(data.ReceivePlayerName, 4, 1));
                });
            }
            else
            {
                var senderActor = _server.getPlayerStorage().GetCharacterActor(data.SenderPlayerId);
                senderActor?.Send(m =>
                {
                    var sender = m.getCharacterById(data.SenderPlayerId);
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
                });
            }
        }
    }
}
