using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Server;
using Application.Core.Server.maps;
using Application.Shared.MapObjects.Summons;
using client.inventory;
using System.Numerics;

namespace Application.Core.Game.Players.Tickables
{
    public class EffectBuff : EffectBase
    {
        StatEffect _effect;
        public StatEffect Effect
        {
            get
            {
                if (_effect is StatEffect statEffect && statEffect.isSkill() && SkillIds.IsPassiveSkill(statEffect.getSourceId()))
                {
                    // 被动技能时，获取最新技能效果
                    return _chr.GetPlayerSkillEffect(statEffect.getSourceId())!;
                }
                return _effect;
            }
        }
        public EffectBuff(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value) : base(chr, buffStat, effect, startTime, expiredAt, value)
        {
            _effect = effect;
        }

        public override async Task OnMounted()
        {
            if (BuffStat == BuffStat.SUMMON || BuffStat == BuffStat.PUPPET)
            {
                Summon tosummon = new Summon(_chr, Effect, _chr.getPosition());
                if (tosummon.MovementType != SummonMovementType.STATIONARY)
                {
                    _chr.addSummon(Effect.getBuffSourceId(), tosummon);
                }

            }
            else if (BuffStat == BuffStat.MONSTER_RIDING && Effect.getSourceId() == Corsair.BATTLE_SHIP)
            {
                await _chr.announceBattleshipHp();
            }

            await base.OnMounted();
        }

        public override async Task OnUnmounted()
        {
            if (BuffStat == BuffStat.SUMMON || BuffStat == BuffStat.PUPPET)
            {
                int summonId = Effect.getSourceId();
                if (_chr.summons.Remove(summonId, out var summon) && summon != null)
                {
                    await _chr.MapModel.RemoveMapObject(summon, chr => chr.SendPacket(SummonPackets.RemoveSummon(summon, SummonRemoveType.Normal)));
                }
            }
            else if (BuffStat == BuffStat.MysticDoor && Effect.getSourceId() == Priest.MYSTIC_DOOR)
            {
                // 时空门借用了 SOULARROW
                await Door.attemptRemoveDoor(_chr);
            }
            await base.OnUnmounted();
        }
    }

    /// <summary>
    /// 越小优先级越高
    /// </summary>
    public class BuffOrder : IComparer<EffectBuff>
    {
        public static BuffOrder Default = new BuffOrder();
        public int Compare(EffectBuff? x, EffectBuff? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            // 取最佳效果
            if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                // 副作用优先级更高
                if (x.Value < 0)
                {
                    return -1;
                }
                if (y.Value < 0)
                {
                    return 1;
                }

                // 加成效果
                var cmp = x.Value.CompareTo(y.Value);
                if (cmp != 0)
                    return -cmp;

                // source的加成数量
                cmp = x.Effect.Statups.Count.CompareTo(y.Effect.Statups.Count);
                if (cmp != 0)
                    return -cmp;

                // 时间
                return -x.ExpiredAt.CompareTo(y.ExpiredAt);
            }
            else
            {
                if (x.Effect.Statups.Count == 1 && y.Effect.Statups.Count == 1)
                {
                    // 单buff 此时互相覆盖
                    return -x.StartTime.CompareTo(y.StartTime);
                }
                else
                {
                    // 多重buff时，可能某个buff优先高，某个低
                    // 此时取副作用
                    if (x.Value < 0)
                    {
                        return -1;
                    }
                    if (y.Value < 0)
                    {
                        return 1;
                    }

                    return -x.StartTime.CompareTo(y.StartTime);
                }
            }
        }
    }
}
