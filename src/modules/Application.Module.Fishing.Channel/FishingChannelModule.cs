using Application.Core.Channel;
using Application.Core.Channel.Events;
using Application.Core.Channel.Services;
using Application.Core.Game.Players;
using Application.Shared.Constants.Item;
using Application.Shared.Constants.Map;
using Application.Utility.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Module.Fishing.Channel
{
    internal class FishingChannelModule : ChannelModule, IFishingService
    {
        readonly FishingManager _manager;
        public FishingChannelModule(WorldChannelServer server, ILogger<ChannelModule> logger, FishingManager manager) : base(server, logger)
        {
            _manager = manager;
        }

        public bool AttemptCatchFish(IPlayer chr, int baitLevel)
        {
            return MapId.isFishingArea(chr.getMapId()) && chr.getPosition().Y > 0 && ItemConstants.isFishingChair(chr.getChair()) && _manager.RegisterFisherPlayer(chr, baitLevel);
        }

        public void StopFishing(IPlayer chr)
        {
            _manager.UnregisterFisherPlayer(chr);
        }

        public override void RegisterTask(ITimerManager timerManager)
        {
            base.RegisterTask(timerManager);
            _manager.Register(timerManager);
        }
    }
}
