using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Players.Tickables;
using Application.Core.Server;
using ZLinq;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public async Task RegisterEffect(StatEffect source, Dictionary<BuffStat, EffectBuff> effectBuffs, bool hideRemote = false)
        {
            await RegisterEffects(new Dictionary<StatEffect, Dictionary<BuffStat, EffectBuff>>() { { source, effectBuffs } }, hideRemote);
        }

        public Task RefreshEffects() => RegisterEffects([]);

        public async Task RegisterEffects(Dictionary<StatEffect, Dictionary<BuffStat, EffectBuff>> newEffects, bool hideRemote = false)
        {
            foreach (var oItem in newEffects)
            {
                var source = oItem.Key;
                var effectBuffs = oItem.Value;
                buffEffects[source.getBuffSourceId()] = effectBuffs;
            }

            // 本次新增/更新（覆盖）buff
            Dictionary<BuffStat, EffectBuff> toUpdateEffects = [];
            // 不满足生效条件临时隐藏
            Dictionary<BuffStat, EffectBuff> toRemoveEffects = [];
            // 被覆盖的旧buff
            HashSet<EffectBuff> overwritedBuffs = [];

            List<IGrouping<BuffStat, EffectBuff>> allStatups = [];
            if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                allStatups = buffEffects
                    .AsValueEnumerable()
                    .Select(x => x.Value).SelectMany(x => x.Values)
                    .OrderBy(x => x)
                    .GroupBy(x => x.BuffStat)
                    .ToList();
            }
            else
            {
                allStatups = buffEffects
                    .AsValueEnumerable()
                    .Select(x => x.Value).SelectMany(x => x.Values)
                    .OrderByDescending(x => x.StartTime)
                    .GroupBy(x => x.BuffStat)
                    .ToList();
            }

            foreach (var currentActive in ActiveEffects.ToList())
            {
                if (!allStatups.Any(x => x.Key == currentActive.Key))
                {
                    toRemoveEffects[currentActive.Key] = currentActive.Value;
                    ActiveEffects.Remove(currentActive.Key);
                }
            }

            foreach (var item in allStatups)
            {
                // 同类型buff中，优先级最高/释放最晚的 且有效的
                var activeItem = item.FirstOrDefault(x => x.Effect.isActive(this));
                if (ActiveEffects.TryGetValue(item.Key, out var exsitedActive))
                {
                    // 应生效buff不存在，移除
                    if (activeItem == null)
                    {
                        toRemoveEffects[item.Key] = exsitedActive;
                        ActiveEffects.Remove(item.Key);
                    }
                    else
                    {
                        if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
                        {
                            // buff被移除 或 应生效buff优于现有buff，替换
                            if (!item.Contains(exsitedActive) || activeItem.CompareTo(exsitedActive) < 0)
                            {
                                toUpdateEffects[item.Key] = activeItem;
                                ActiveEffects[item.Key] = activeItem;

                                overwritedBuffs.Add(exsitedActive);
                            }
                        }
                        else
                        {
                            if (!item.Contains(exsitedActive))
                            {
                                // 被移除
                                toRemoveEffects[item.Key] = exsitedActive;
                                ActiveEffects.Remove(item.Key);
                            }
                            else if (exsitedActive != activeItem)
                            {
                                // 覆盖 必定触发
                                toUpdateEffects[item.Key] = activeItem;
                                ActiveEffects[item.Key] = activeItem;

                                overwritedBuffs.Add(exsitedActive);
                            }
                        }
                    }

                }
                else
                {
                    if (activeItem != null)
                    {
                        toUpdateEffects[item.Key] = activeItem;
                        ActiveEffects[item.Key] = activeItem;
                    }
                }
            }

            if (toUpdateEffects.Count > 0)
            {
                foreach (var item in overwritedBuffs)
                {
                    await item.OnUnmounted();
                }

                foreach (var item in toUpdateEffects)
                {
                    await item.Value.OnMounted();
                }

                await SendPacket(BuffPackets.GiveBuff(this, toUpdateEffects));

                if (!hideRemote)
                {
                    var p = BuffPackets.GiveRemoteBuff(Id, toUpdateEffects);
                    if (p != null)
                    {
                        await BroadcastMap(p, Id);
                    }
                }
            }

            if (toRemoveEffects.Count > 0)
            {
                foreach (var item in toRemoveEffects)
                {
                    await item.Value.OnUnmounted();
                }
                // 是否有必要
                await SendPacket(BuffPackets.CancelBuff(this, toRemoveEffects.Keys));
                await BroadcastMap(BuffPackets.CancelRemoteBuff(Id, toRemoveEffects.Keys), Id);
            }

            if (toUpdateEffects.Count > 0 || toRemoveEffects.Count > 0)
            {
                await UpdateLocalStats();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffStat"></param>
        /// <param name="deep">true: 完全移除, false: 仅移除当前生效项（取最优时）</param>
        /// <returns></returns>
        public Task CancelBuff(BuffStat buffStat, bool deep = true) => CancelBuffs([buffStat], deep);

        public async Task CancelBuffs(IEnumerable<BuffStat> buffStats, bool deep = true)
        {
            if (buffStats.Count() == 0)
            {
                return;
            }

            HashSet<StatEffect> effects = [];
            if (deep || !YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                foreach (var buff in buffStats)
                {
                    foreach (var item in buffEffects.Values)
                    {
                        if (item.TryGetValue(buff, out var effect))
                        {
                            effects.Add(effect.Effect);
                        }
                    }
                }
            }
            else
            {
                foreach (var buff in buffStats)
                {
                    if (ActiveEffects.TryGetValue(buff, out var effect))
                    {
                        effects.Add(effect.Effect);
                    }
                }
            }
            // 比如轻功 是speed + jump， 但取消不可能仅取消speed/jump --> 取消应该以source为单位
            await CancelBuffFromSource(effects);
        }

        public async Task CancelBuffFromSource(IEnumerable<StatEffect> statEffects)
        {
            if (statEffects.Count() == 0)
            {
                return;
            }

            foreach (var statEffect in statEffects)
            {
                buffEffects.Remove(statEffect.getBuffSourceId());
            }

            await RefreshEffects();
        }

        public Task CancelBuffFromSource(StatEffect statEffect) => CancelBuffFromSource([statEffect]);

        public Task CancelAllBuffs() => CancelBuffFromSource(buffEffects.Values.AsValueEnumerable().SelectMany(x => x.Values.Select(y => y.Effect)).ToHashSet());
        public async Task CancelBuffFromSourceId(int skillId)
        {
            if (buffEffects.TryGetValue(skillId, out var data) && data.Values.Count > 0)
            {
                await CancelBuffFromSource(data.Values.FirstOrDefault()!.Effect);
            }
        }
    }
}
