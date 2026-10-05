using Application.Core.Server;
using tools;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    internal class MonsterNinja : MonsterDebuff
    {
        long _nextAct;
        int _actPeriod;
        public MonsterNinja(Monster mob, int fromObject, MonsterStatus buffStat, StatEffect buffSource, int value, long startTime, long expiredAt)
            : base(mob, fromObject, buffStat, buffSource, value, startTime, expiredAt)
        {
        }

        protected override async Task Process(long now)
        {
            if (now > _nextAct)
            {
                int curHp = _mob.getHp();
                if (curHp <= 1)
                {
                    Status = Utility.Tickables.TickableStatus.Remove;
                }
                else
                {
                    int damage = (int)Math.Min(Value, curHp - 1);
                    if (damage > 0)
                    {
                        var chr = _mob.MapModel.getCharacterById(FromPlayerOId);
                        if (chr != null)
                        {
                            await _mob.DamageBy(chr, damage, 0, true);

                            if (damage < Value)
                            {
                                // ninja ambush (type 2) is already displaying DOT to the caster
                                await _mob.BroadcastMap(PacketCreator.damageMonster(_mob.getObjectId(), damage));
                            }
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
            await base.Process(now);
        }
    }
}
