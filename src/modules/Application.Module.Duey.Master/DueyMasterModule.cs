using Application.Core.Login;
using Application.Core.Login.Events;
using Application.Core.Login.Models;
using Application.EF;
using Application.Utility;
using Application.Utility.Tasks;
using DotNetty.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Application.Module.Duey.Master
{
    public class DueyMasterModule: MasterModule
    {
        readonly DueyManager _manager;
        readonly DueyTask _dueyTask;
        public DueyMasterModule(MasterServer server, ILogger<MasterModule> logger, DueyManager dueyManager, DueyTask dueyTask) : base(server, logger)
        {
            _manager = dueyManager;
            _dueyTask = dueyTask;
        }

        public override void RegisterTask(ITimerManager timerManager)
        {
            base.RegisterTask(timerManager);

            var timeLeft = TimeUtils.GetTimeLeftForNextHour();
            timerManager.register(_dueyTask, TimeSpan.FromHours(1), timeLeft);
        }

        public override void OnPlayerLogin(CharacterLiveObject obj)
        {
            base.OnPlayerLogin(obj);

            _manager.SendDueyNotifyOnLogin(obj.Character.Id);
        }

        public override void OnPlayerLogoff(CharacterLiveObject obj)
        {
            base.OnPlayerLogoff(obj);

            _manager.PackageUnfreeze(obj.Character.Id);
        }
    }
}
