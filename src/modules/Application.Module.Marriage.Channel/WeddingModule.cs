using Application.Core.Channel;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Module.Marriage.Common;
using Microsoft.Extensions.Logging;

namespace Application.Module.Marriage.Channel
{
    public class WeddingModule : ChannelModule
    {
        readonly WeddingManager _weddingManager;
        public WeddingModule(WorldChannelServer server, ILogger<ChannelModule> logger, WeddingManager weddingManager) : base(server, logger)
        {
            _weddingManager = weddingManager;
        }

        public override void Initialize()
        {
            base.Initialize();

            MessageDispatcher.Register<MarriageProto.BroadcastWeddingDto>(MessageType.WeddingBroadcast, _weddingManager.BroadcastWedding);
            MessageDispatcher.Register<MarriageProto.InviteGuestCallback>(MessageType.WeddingInviteGuest, _weddingManager.OnGuestInvited);
            MessageDispatcher.Register<MarriageProto.BreakMarriageCallback>(MessageType.MarriageBroken, _weddingManager.OnMarriageBroken);
        }

    }
}
