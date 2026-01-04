using Application.Core.Channel;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Game.Players;
using Application.Module.Marriage.Common;
using Application.Module.Marriage.Common.Models;
using Application.Shared.Constants.Item;
using Application.Shared.Net;
using Dto;
using Microsoft.Extensions.Logging;

namespace Application.Module.Marriage.Channel
{
    public class MarriageChannelModule : AbstractChannelModule, IMarriageService
    {
        readonly WeddingManager _weddingManager;
        readonly MarriageManager _marriageManager;
        public MarriageChannelModule(WorldChannelServer server, ILogger<AbstractChannelModule> logger, WeddingManager weddingManager, MarriageManager marriageManager) : base(server, logger)
        {
            _weddingManager = weddingManager;
            _marriageManager = marriageManager;
        }

        public override void Initialize()
        {
            base.Initialize();

            MessageDispatcher.Register<MarriageProto.BroadcastWeddingDto>(MasterSend.WeddingBroadcast, _weddingManager.BroadcastWedding);
            MessageDispatcher.Register<MarriageProto.InviteGuestCallback>(MasterSend.WeddingInviteGuest, _weddingManager.OnGuestInvited);
            MessageDispatcher.Register<MarriageProto.BreakMarriageCallback>(MasterSend.MarriageBroken, _weddingManager.OnMarriageBroken);

            MessageDispatcher.Register<MarriageProto.PlayerTransferDto>(MasterSend.NotifyPartnerWhenTransfer, _marriageManager.NotifyPartnerWhenTransfer);
            MessageDispatcher.Register<MarriageProto.OnSpouseChatCallback>(MasterSend.SpouseChat, _marriageManager.OnReceivedSpouseChat);
        }

        public override void OnPlayerLogin(Player chr)
        {
            base.OnPlayerLogin(chr);
            _marriageManager.CheckMarriageData(chr);
        }

        public void WriteMarriageRing(OutPacket p, IPlayer chr)
        {
            var info = _marriageManager.GetPlayerMarriageInfo(chr.Id);
            if (info == null)
            {
                p.writeShort(0);
            }

            else
            {
                p.writeShort(1);
                p.writeInt(info.Id);
                p.writeInt(info.HusbandId);
                p.writeInt(info.WifeId);
                p.writeShort(info.Status == MarriageStatusEnum.Engaged ? (int)MarriageClientStatus.ENGAGED : (int)MarriageClientStatus.MARRIED);
                if (info.Status == Common.Models.MarriageStatusEnum.Engaged)
                {
                    p.writeInt(ItemId.WEDDING_RING_MOONSTONE); // Engagement Ring's Outcome (doesn't matter for engagement)
                    p.writeInt(ItemId.WEDDING_RING_MOONSTONE); // Engagement Ring's Outcome (doesn't matter for engagement)
                }
                else
                {
                    var ringInfo = chr.GetRingBySourceId(info.RingSourceId)!;
                    p.writeInt(ringInfo.getItemId());
                    p.writeInt(ringInfo.getItemId());
                }
                p.writeFixedString(info.HusbandName);
                p.writeFixedString(info.WifeName);
            }
        }
    }
}
