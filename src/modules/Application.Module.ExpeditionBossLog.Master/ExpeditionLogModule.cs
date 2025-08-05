using Application.Core.Login;
using Application.Core.Login.Events;
using Application.Core.Login.Services;
using Application.EF;
using Application.Utility;
using Application.Utility.Tasks;
using Dto;
using Microsoft.Extensions.Logging;

namespace Application.Module.ExpeditionBossLog.Master
{
    internal class ExpeditionLogModule : MasterModule, IExpeditionService
    {
        readonly ExpeditionBossLogManager _manager;

        ScheduledFuture? _task;
        public ExpeditionLogModule(MasterServer server, ILogger<MasterModule> logger, ExpeditionBossLogManager manager) : base(server, logger)
        {
            _manager = manager;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _manager.ResetBossLogTable();

        }

        public override void RegisterTask(ITimerManager timerManager)
        {
            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            _task = timerManager.register(new BossLogTask(_manager), TimeSpan.FromDays(1), timeLeft);
        }

        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();

            if (_task != null)
                await _task.CancelAsync(false);
        }

        public ExpeditionCheckResponse CanStartExpedition(ExpeditionCheckRequest request)
        {
            return new ExpeditionCheckResponse { IsSuccess = _manager.AttemptBoss(request.Cid, request.Channel, request.BossName, false) };
        }

        public void RegisterExpedition(ExpeditionRegistry request)
        {
            foreach (var cid in request.CidList)
            {
                _manager.AttemptBoss(cid, request.Channel, request.BossName, true);
            }
        }
    }
}
