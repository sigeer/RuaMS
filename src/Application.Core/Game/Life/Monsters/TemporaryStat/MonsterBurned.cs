using Application.Core.Server;
using Application.Shared.Battle;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    public class MonsterBurned : MonsterDebuff
    {
        int _actPeriod;
        long _nextAct;

        public MonsterBurned(
            Monster mob,
            int playerOId,
            StatEffect playerSkillEffect,
            long startTime,
            long expireAt)
            : base(mob, playerOId, MonsterStatus.Burned, playerSkillEffect, 0, startTime, expireAt)
        {
            BurnList = [];

            _actPeriod = 1000;
        }

        public Dictionary<int, BurnInfo> BurnList { get; }

        protected override async Task Process(long now)
        {
            if (now >= _nextAct)
            {
                var damage = BurnList.Sum(x => x.Value.Damage);

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

                _nextAct = now + _actPeriod;
            }
        }

        public override void EncodeForLocalOne(OutPacket p)
        {
            p.writeInt(_mob.BurnInfos.Count);
            foreach (var entry in _mob.BurnInfos)
            {
                p.writeInt(entry.FromSourceId);
                p.writeInt(entry.Damage);
                p.writeInt(entry.ExpiratedAt);
            }
        }
    }
}
