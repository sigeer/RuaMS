using Application.Core.EF.Entities;
using Application.Core.Login;
using Application.Core.Login.Models;
using Application.Core.Login.Shared;
using Application.EF;
using Application.Shared.Battle;
using Application.Utility;
using Application.Utility.Configs;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.ServerData.ExpeditionBossLog
{
    public class ExpeditionManager : DataStorageBase<int, PlayerBossLogModel, BossLogEntity>
    {
        readonly MasterServer _server;

        List<ExpeditionEntryType> _allTypes = EnumClassUtils.GetValues<ExpeditionEntryType>();

        public ExpeditionManager(ILogger<ExpeditionManager> logger, MasterServer server, IDbContextFactory<DBContext> dbContextFactory, IMapper mapper)
            : base(StorageCategory.ExpeditionRecord, dbContextFactory, mapper, logger)
        {
            _server = server;
            _logger = logger;
        }

        protected override int GetKey(PlayerBossLogModel model) => model.Id;

        protected override PlayerBossLogModel MapModel(BossLogEntity entity)
        {
            return new PlayerBossLogModel
            {
                Id = entity.Id,
                BossName = entity.BossType,
                CharacterId = entity.CharacterId,
                Flag = entity.Flag,
                Time = entity.Time
            };
        }

        protected override BossLogEntity MapEntity(PlayerBossLogModel localModel)
        {
            return new BossLogEntity(localModel.Id, localModel.CharacterId, localModel.BossName, localModel.Flag, localModel.Time);
        }

        protected override BossLogEntity MapExsitedEntity(PlayerBossLogModel localModel, BossLogEntity dbModel)
        {
            dbModel.Flag = localModel.Flag;

            return dbModel;
        }

        List<PlayerBossLogModel> GetTodayData(int characterId, string bossName)
        {
            var today = _server.GetCurrentTimeDateTimeOffset().ToLocalTime();
            return Query(x => x.CharacterId == characterId && x.BossType == bossName && x.Time.Date == today
            , x => x.CharacterId == characterId && x.BossName == bossName && x.Time.Date == today);
        }

        List<PlayerBossLogModel> GetWeekData(int characterId, string bossName)
        {
            var now = _server.GetCurrentTimeDateTimeOffset().ToLocalTime();
            var diff = (int)now.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var monday = now.Date.AddDays(-diff);
            var mondayOffset = new DateTimeOffset(monday, now.Offset);
            var nextMondayOffset = mondayOffset.AddDays(7);

            return Query(x => x.CharacterId == characterId && x.BossType == bossName && x.Time >= mondayOffset && x.Time < nextMondayOffset && x.Flag == 0
            , x => x.CharacterId == characterId && x.BossName == bossName && x.Time >= mondayOffset && x.Time < nextMondayOffset && x.Flag == 0);
        }

        public bool AttemptBoss(int cid, int channel, string bossName, bool log)
        {
            if (!YamlConfig.config.server.USE_ENABLE_DAILY_EXPEDITIONS)
            {
                return true;
            }

            var boss = getBossEntryByName(bossName);
            if (boss == null)
            {
                return true;
            }

            if (channel < boss.MinChannel || channel > boss.MaxChannel)
            {
                return false;
            }

            var dataList = boss.Week ? GetWeekData(cid, bossName) : GetTodayData(cid, bossName);
            if (dataList.Count >= boss.Entries)
            {
                return false;
            }

            if (log)
            {
                SetDirty(new PlayerBossLogModel { Id = Interlocked.Increment(ref _localId), BossName = boss.name(), CharacterId = cid, Time = _server.GetCurrentTimeDateTimeOffset() });
            }
            return true;
        }

        public ExpeditionEntryType? getBossEntryByName(string name)
        {
            return _allTypes.FirstOrDefault(x => x.name() == name);
        }

        public ProtoService.ExpeditionCheckResponse CanStartExpedition(ProtoService.ExpeditionCheckRequest request)
        {
            return new ProtoService.ExpeditionCheckResponse { IsSuccess = AttemptBoss(request.Cid, request.Channel, request.BossName, false) };
        }

        public void RegisterExpedition(ProtoModel.ExpeditionRegistry request)
        {
            foreach (var cid in request.CidList)
            {
                AttemptBoss(cid, request.Channel, request.BossName, true);
            }
        }
    }
}
