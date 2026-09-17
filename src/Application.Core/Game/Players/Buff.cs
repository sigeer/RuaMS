using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Gameplay;
using Application.Core.Game.Players.Tickables;
using Application.Core.Game.Skills;
using Application.Core.model;
using Application.Core.Server;
using Application.Core.Server.life;
using Application.Core.Server.maps;
using Application.Shared.Battle.Skills;
using Application.Shared.MapObjects.Summons;
using Application.Utility.Tickables;
using Humanizer;
using net.server;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {

        /// <summary>
        /// buffEffects 同一类型buff取效果最好的那种，当该buff过期时，使用次一级效果且未过期的
        /// </summary>

        public Dictionary<BuffStat, EffectBuff> ActiveEffects { get; } = new();
        /// <summary>
        /// sourceid - effects
        /// <para>
        /// 玩家所有buff
        /// 同类型的buff可能有多个来源：加成同一种属性的不同药品
        /// </para>
        /// <para>
        /// 键不能用StatEffect: 玩家的技能等级可能发生变化。可以通过EffectBuff获取到StatEffect
        /// </para>
        /// </summary>
        private Dictionary<int, Dictionary<BuffStat, EffectBuff>> buffEffects = new();


        public bool hasBuffFromSourceid(int sourceid)
        {
            return buffEffects.ContainsKey(sourceid);
        }

        public EffectBuff? GetBuffStatValue(BuffStat effect)
        {
            return ActiveEffects.GetValueOrDefault(effect);
        }

        public int? getBuffedValue(BuffStat effect)
        {
            return GetBuffStatValue(effect)?.Value;
        }

        public int getBuffSource(BuffStat stat)
        {
            return GetBuffStatValue(stat)?.Effect?.getSourceId() ?? -1;
        }

        public bool HasBuff(BuffStat stat)
        {
            return ActiveEffects.ContainsKey(stat);
        }

        public List<EffectBuff> getAllStatups()
        {
            return buffEffects.Values.SelectMany(x => x.Values).OrderBy(x => x).ToList();
        }

        public List<PlayerBuffValueHolder> getAllBuffs()
        {
            Dictionary<int, PlayerBuffValueHolder> ret = new();
            foreach (var bel in buffEffects)
            {
                var effectBuffStats = new List<BuffStatValue>();
                foreach (var mbsvh in bel.Value)
                {
                    if (!ret.ContainsKey(bel.Key))
                    {
                        ret.Add(bel.Key, new PlayerBuffValueHolder(mbsvh.Value.StartTime, mbsvh.Value.ExpiredAt, mbsvh.Value.Effect, effectBuffStats));
                    }
                    effectBuffStats.Add(new BuffStatValue(mbsvh.Key, mbsvh.Value.Value));
                }
            }
            return new(ret.Values);
        }

        public string debugListAllBuffs()
        {
            StringBuilder sb = new();
            sb.AppendLine("======== 所有BUFF =========");
            foreach (var item in buffEffects.ToList())
            {
                sb.AppendLine($"来源：#b{(item.Key < 0 ? Client.CurrentCulture.GetItemName(item.Key) : Client.CurrentCulture.GetSkillName(item.Key))}#k");
                foreach (var effect in item.Value)
                {
                    var isActive = ActiveEffects.GetValueOrDefault(effect.Key) == effect.Value;
                    if (isActive)
                        sb.AppendLine("#r");
                    else
                        sb.AppendLine("#k");
                    sb.Append($"    效果: {effect.Key}, 值: {effect.Value.Value}, 过期时间: {DateTimeOffset.FromUnixTimeMilliseconds(effect.Value.ExpiredAt).ToLocalTime().Humanize(culture: Client.CurrentCulture.CultureInfo)}");

                    sb.AppendLine("#k");
                }
            }

            sb.AppendLine("======== 正在生效BUFF =========");
            foreach (var item in ActiveEffects.ToList())
            {
                sb.AppendLine($"效果: {item.Key}, 值: {item.Value.Value}, 过期时间: {DateTimeOffset.FromUnixTimeMilliseconds(item.Value.ExpiredAt).ToLocalTime().Humanize(culture: Client.CurrentCulture.CultureInfo)}");
            }

            return sb.ToString();
        }

        public async Task ClearExpiredBuffs()
        {
            HashSet<BuffStat> toCancel = new();

            foreach (var bel in getAllStatups())
            {
                if (bel.Status == TickableStatus.Remove)
                {
                    toCancel.Add(bel.BuffStat);    //rofl
                }
            }

            await CancelBuffs(toCancel, false);
            //foreach (var item in toCancel)
            //{
            //    await cancelEffect(item.Effect, false);
            //}
        }




        public bool hasActiveBuff(int sourceid)
        {
            var allBuffs = ActiveEffects.Values.ToList();

            foreach (EffectBuff mbsvh in allBuffs)
            {
                if (mbsvh.Effect.getBuffSourceId() == sourceid)
                {
                    return true;
                }
            }
            return false;
        }

        public float getCardRate(int itemid)
        {
            float rate = 100.0f;

            if (itemid == 0)
            {
                var mseMeso = GetBuffStatValue(BuffStat.MESO_UP_BY_ITEM);
                if (mseMeso != null)
                {
                    rate += mseMeso.Effect.getCardRate(this, itemid);
                }
            }
            else
            {
                var mseItem = GetBuffStatValue(BuffStat.ITEM_UP_BY_ITEM);
                if (mseItem != null)
                {
                    rate += mseItem.Effect.getCardRate(this, itemid);
                }
            }

            return rate / 100;
        }

        public bool IsMorphWithoutAttack()
        {
            var morphBuff = getBuffedValue(BuffStat.MORPH);
            if (morphBuff != null)
            {
                return morphBuff > 0 && morphBuff < 100;
            }
            return false;
        }

        public static EffectBuff GetPlayerBuffStatValueHolder(Player chr, BuffStat buffStat, StatEffect effect, long startTime, long expiredAt, int value)
        {
            // 这几个技能都是对自己释放，不会触发“相同取最优”的逻辑
            if (effect.isDragonBlood())
            {
                return new EffectDragonBlood(chr, buffStat, effect, startTime, expiredAt, value);
            }
            else if (effect.isBerserk())
            {
                return new EffectBerserk(chr, buffStat, effect, startTime, expiredAt, value);
            }
            else if (effect.isRecovery())
            {
                return new EffectRecovery(chr, buffStat, effect, startTime, expiredAt, value);
            }
            else if (buffStat == BuffStat.HOMING_BEACON)
            {
                return new EffectGuideBullet(chr, buffStat, effect, startTime, expiredAt, value);
            }
            return new EffectBuff(chr, buffStat, effect, startTime, expiredAt, value);
        }

        /// <summary>
        /// buff存在时更新
        /// </summary>
        /// <param name="buffStat"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task UpdateBuff(BuffStat buffStat, Action<EffectBuff> action, Func<Task>? triggerIfNotExsited = null)
        {
            var oHolder = GetBuffStatValue(buffStat);
            if (oHolder != null)
            {
                action(oHolder);

                await SendPacket(BuffPackets.GiveBuff(this, oHolder));

                var p = BuffPackets.GiveRemoteBuff(Id, oHolder);
                if (p != null)
                {
                    await BroadcastMap(p, Id);
                }
            }
            else
            {
                if (triggerIfNotExsited != null)
                {
                    await triggerIfNotExsited.Invoke();
                }
            }
        }

        public async Task dispel()
        {
            if (!(YamlConfig.config.server.USE_UNDISPEL_HOLY_SHIELD && this.hasActiveBuff(Bishop.HOLY_SHIELD)))
            {
                var mbsvhList = getAllStatups().Where(x => x.Effect.getBuffSourceId() != Aran.COMBO_ABILITY)
                    .Select(x => x.Effect).ToList();
                await CancelBuffFromSource(mbsvhList);
            }
        }
    }
}
