using Application.Core.Server;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    public class MonsterPosion : MonsterDebuff
    {
        int _actPeriod;
        long _nextAct;
        public MonsterPosion(Monster mob, int chrObjectId, MonsterStatus buffStat, StatEffect buffSource, int value, long startTime, long expiredAt)
            : base(mob, chrObjectId, buffStat, buffSource, value, startTime, expiredAt)
        {
            Value = value;
            _actPeriod = 1000;
        }

        protected override async Task Process(long now)
        {
            if (now >= _nextAct)
            {
                int curHp = _mob.getHp();
                if (curHp <= 1)
                {
                    Status = Utility.Tickables.TickableStatus.Remove;
                }
                else
                {
                    int damage = Value;
                    if (damage >= curHp)
                    {
                        damage = curHp - 1;
                    }
                    if (damage > 0)
                    {
                        var chr = _mob.MapModel.getCharacterById(FromPlayerOId);
                        if (chr != null)
                        {
                            await _mob.DamageBy(chr, damage, 0, true);
                        }
                        else
                        {
                            // 释放者离开地图，取消
                            Status = Utility.Tickables.TickableStatus.Remove;
                        }
                    }
                }
                _nextAct = now + _actPeriod;
            }
        }
    }
}
