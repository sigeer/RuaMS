using Application.Core.Channel;
using Application.Core.Channel.Invitation;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;

namespace Application.Module.Family.Channel
{
    internal class FamilySummonInviteChannelHandler : InviteChannelHandler
    {
        readonly FamilyManager _familyManager;
        public FamilySummonInviteChannelHandler(WorldChannelServer server, ILogger<InviteChannelHandler> logger, FamilyManager familyManager) 
            : base(server, Constants.InviteType_FamilySummon, logger)
        {
            _familyManager = familyManager;
        }

        public override void OnInvitationAnswered(AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;

            if (result != InviteResultType.ACCEPTED)
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    var inviterEntry = _familyManager.GetFamily(data.SenderPlayerId)?.getEntryByID(data.SenderPlayerId);
                    if (inviterEntry == null)
                    {
                        return;
                    }

                    if (result == InviteResultType.DENIED)
                    {
                        sender.dropMessage(5, data.ReceivePlayerName + " has denied the summon request.");
                    }
                }
            }
            else
            {
                var receiver = _server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.changeMap(data.Key);
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
                    receiver.sendPacket(FamilyPacketCreator.sendFamilySummonRequest(data.KeyString, data.SenderPlayerName));
                }
            }
            else
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    switch (code)
                    {
                        case InviteResponseCode.MANAGING_INVITE:
                            sender.sendPacket(FamilyPacketCreator.sendFamilyMessage(74, 0));
                            break;
                        case InviteResponseCode.InviteesNotFound:
                            // 
                            break;
                        default:
                            _logger.LogCritical("预料之外的邀请回调: Type:{Type}, Code: {Code}", "FamilySummon", code);
                            break;
                    }
                }

            }
        }
    }
}
