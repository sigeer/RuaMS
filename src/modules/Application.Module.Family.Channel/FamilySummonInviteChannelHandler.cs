using Application.Core.Channel;
using Application.Core.Channel.Invitation;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Invitations;
using Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tools;

namespace Application.Module.Family.Channel
{
    internal class FamilySummonInviteChannelHandler : InviteChannelHandler
    {
        public FamilySummonInviteChannelHandler(WorldChannelServer server, string type, ILogger<InviteChannelHandler> logger) : base(server, type, logger)
        {
        }

        public override void OnInvitationAnswered(AnswerInviteResponse data)
        {
            var result = (InviteResultType)data.Result;

            if (result != InviteResultType.ACCEPTED)
            {
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    var inviterEntry = sender.getFamilyEntry();
                    if (inviterEntry == null)
                    {
                        return;
                    }

                    inviterEntry.refundEntitlement(FamilyEntitlement.SUMMON_FAMILY);
                    inviterEntry.gainReputation(FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false); //refund rep cost if declined

                    if (result == InviteResultType.DENIED)
                    {
                        sender.sendPacket(FamilyPacketCreator.getFamilyInfo(inviterEntry));
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
                var receiver =_server.FindPlayerById(data.ReceivePlayerId);
                if (receiver != null)
                {
                    receiver.sendPacket(FamilyPacketCreator.sendFamilySummonRequest(receiver.getFamily()!.getName(), data.SenderPlayerName));
                }
                var sender = _server.FindPlayerById(data.SenderPlayerId);
                if (sender != null)
                {
                    var entry = sender.getFamilyEntry();
                    if (entry.useEntitlement(FamilyEntitlement.SUMMON_FAMILY))
                    {
                        entry.gainReputation(-FamilyEntitlement.SUMMON_FAMILY.getRepCost(), false);
                        sender.sendPacket(FamilyPacketCreator.getFamilyInfo(entry));
                    }
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
