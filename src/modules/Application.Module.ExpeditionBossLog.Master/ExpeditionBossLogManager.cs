using Application.Core.Login.Events;
using Application.EF;
using Application.Utility;
using Application.Utility.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Application.Module.ExpeditionBossLog.Master
{
    public class ExpeditionBossLogManager
    {
        readonly ILogger<ExpeditionBossLogManager> _logger;
        List<PlayerBossLogModel> _dataSource = new();

        List<BossLogEntry> _allTypes = EnumClassUtils.GetValues<BossLogEntry>();

        public ExpeditionBossLogManager(ILogger<ExpeditionBossLogManager> logger)
        {
            _logger = logger;
        }

        public async Task LoadDataAsync(DBContext dbContext)
        {
            var weeklyData = await dbContext.BosslogWeeklies.AsNoTracking()
                .Select(x => new PlayerBossLogModel { BossName = x.Bosstype, CharacterId = x.CharacterId, Time = x.Attempttime }).ToArrayAsync();
            var dailyData = await dbContext.BosslogDailies.AsNoTracking()
                .Select(x => new PlayerBossLogModel { BossName = x.Bosstype, CharacterId = x.CharacterId, Time = x.Attempttime }).ToArrayAsync();
            _dataSource.AddRange(weeklyData);
            _dataSource.AddRange(dailyData);
        }

        public async Task CommitDataAsync(DBContext dbContext)
        {
            await dbContext.BosslogWeeklies.ExecuteDeleteAsync();
            await dbContext.BosslogDailies.ExecuteDeleteAsync();
            foreach (var type in _allTypes)
            {
                var typedData = _dataSource.Where(x => x.BossName == type.name());
                if (type.Week)
                {
                    dbContext.BosslogWeeklies.AddRange(typedData.Select(x => new EF.Entities.BosslogWeekly(x.CharacterId, x.BossName, x.Time)));
                }
                else
                {
                    dbContext.BosslogDailies.AddRange(typedData.Select(x => new EF.Entities.BosslogDaily(x.CharacterId, x.BossName, x.Time)));
                }
            }
            await dbContext.SaveChangesAsync();
        }



        public void ResetBossLogTable()
        {
            /*
            Boss logs resets 12am, weekly thursday 12AM - thanks Smitty Werbenjagermanjensen (superadlez) - https://www.reddit.com/r/Maplestory/comments/61tiup/about_reset_time/
            */
            var thursday = DateTimeOffset.UtcNow.Date;
            int delta = ((int)DayOfWeek.Thursday - (int)thursday.DayOfWeek + 7) % 7;
            //if (delta <= 0)
            //    delta += 7; // 如加7天到下一个周四
            thursday = thursday.AddDays(delta);

            DateTimeOffset now = DateTimeOffset.UtcNow;
            var deltaTime = now - thursday;
            if (deltaTime.TotalHours < 12)
            {
                ResetWeeklyData(thursday);
            }

            ResetDailyData(now);
        }

        private void ResetWeeklyData(DateTimeOffset c)
        {
            var weeklyType = _allTypes.Where(x => x.Week).ToList();
            foreach (var data in weeklyType)
            {
                var bossName = data.name();
                _dataSource.RemoveAll(x => x.BossName == bossName && x.Time <= c);
            }
        }

        private void ResetDailyData(DateTimeOffset c)
        {
            var dailyType = _allTypes.Where(x => !x.Week).ToList();
            foreach (var data in dailyType)
            {
                var bossName = data.name();
                _dataSource.RemoveAll(x => x.BossName == bossName && x.Time <= c);
            }
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

            if (_dataSource.Count(x => x.CharacterId == cid && x.BossName == boss.name()) >= boss.Entries)
            {
                return false;
            }

            if (log)
            {
                _dataSource.Add(new PlayerBossLogModel { BossName = boss.name(), CharacterId = cid, Time = DateTimeOffset.UtcNow });
            }
            return true;
        }

        public BossLogEntry? getBossEntryByName(string name)
        {
            return _allTypes.FirstOrDefault(x => x.name() == name);
        }
    }
}
