using Application.Core.Channel;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Services;
using Application.Core.Client;
using Application.Module.Duey.Common;
using Dto;
using DueyDto;

namespace Application.Module.Duey.Channel
{
    public class DueyChannelModule : ChannelModule, IDueyService
    {
        readonly DueyManager _manager;
        public DueyChannelModule(WorldChannelServer server, Microsoft.Extensions.Logging.ILogger<ChannelModule> logger, DueyManager manager) : base(server, logger)
        {
            _manager = manager;
        }

        public void DueyTalk(IChannelClient c, bool quickDelivery)
        {
            _manager.SendTalk(c, quickDelivery);
        }

        public override void Initialize()
        {
            base.Initialize();

            MessageDispatcher.Register<TakeDueyPackageResponse>(BroadcastType.OnDueyPackageTaking, _manager.OnTakePackage);
            MessageDispatcher.Register<RemovePackageResponse>(BroadcastType.OnDueyPackageRemove, _manager.OnDueyPackageRemoved);
            MessageDispatcher.Register<CreatePackageBroadcast>(BroadcastType.OnDueyPackageCreation, _manager.OnDueyPackageCreated);
            // MessageDispatcher.Register<DueyNotificationDto>(BroadcastType.OnDueyNotification, _manager.OnDueyNotificationReceived);
            MessageDispatcher.Register<DueyNotifyDto>(BroadcastType.OnDueyNotify, _manager.OnLoginDueyNotify);
        }
    }
}
