/*
This file is part of the OdinMS Maple Story NewServer
Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
Matthias Butz <matze@odinms.de>
Jan Christian Meyer <vimes@odinms.de>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation version 3 as published by
the Free Software Foundation. You may not use, modify or distribute
this program under any other version of the GNU Affero General Public
License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Core.Game.Skills;
using Application.Core.Server.life;
using client.autoban;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class TakeDamageHandler : ChannelHandlerBase
{
    readonly ILogger<TakeDamageHandler> _logger;

    public TakeDamageHandler(ILogger<TakeDamageHandler> logger)
    {
        _logger = logger;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        List<Player> banishPlayers = new();

        var chr = c.OnlinedCharacter;
        var map = chr.getMap();

        // =====================================================================
        // CUserLocal::SetDamaged (0x9581a9)
        // =====================================================================

        // --- 公共字段 ---
        int update_time = p.readInt();                              // [4] COutPacket::Encode4(v134) - 客户端 get_update_time()
        /// <see cref="Shared.Battle.AttackIndex"/>
        sbyte damagefrom = p.ReadSByte();                          // [1] 攻击来源索引/类型 (下面分两种情况编码)
        var element = EnumClassCache<Element>.GetValues()[p.readByte()]; // [1] 元素属性
        int damage = p.readInt();                                   // [4] 伤害值
        double finalDamage = damage;
        // 向上取整
        int finalDamageValue = 0;
        int oid = 0, mobId = 0, stance = 0, direction = 0;
        int action = 0, pos_x = 0, pos_y = 0, fake = 0;
        byte pgmr = 0;
        bool is_pg = false;
        sbyte guard = 0;
        bool is_deadly = false;
        int mpattack = 0;
        Monster? attacker = null;

        // --- 分支1: 有怪物 (a7 != NULL) ---
        // if (a7) { ... } else { ... }
        // damagefrom >= -1 伤害来自mob
        // damagefrom = -2 或 -3 表示无怪物 (来自 a11 参数)
        if (damagefrom >= -1)
        {
            // Encode4(mobId) - 怪物模板ID (TSecType secure fused)
            mobId = p.readInt();

            // Encode4(oid) - 怪物对象ID
            oid = p.readInt();

            try
            {
                attacker = map.getMonsterByOid(oid);
                if (attacker == null)
                {
                    map.removeSelfDestructive(oid);
                }
                if (attacker?.getId() != mobId)
                {
                    return;
                }

                if (attacker.isBuffed(MonsterStatus.NEUTRALISE))
                {
                    return;
                }

                List<LoseItem>? loseItems;
                if (damage > 0)
                {
                    loseItems = attacker.getStats().loseItem();
                    if (loseItems != null)
                    {
                        if (chr.getBuffEffect(BuffStat.AURA) == null)
                        {
                            InventoryType type;
                            int playerpos = chr.getPosition().X;
                            byte d = 1;
                            Point pos = new Point(0, chr.getPosition().Y);
                            foreach (var loseItem in loseItems)
                            {
                                type = ItemConstants.getInventoryType(loseItem.Id);

                                int dropCount = 0;
                                for (byte b = 0; b < loseItem.X; b++)
                                {
                                    if (Randomizer.nextInt(100) < loseItem.Prob)
                                    {
                                        dropCount += 1;
                                    }
                                }

                                if (dropCount > 0)
                                {
                                    int qty;

                                    var inv = chr.getInventory(type);

                                    qty = Math.Min(chr.countItem(loseItem.Id), dropCount);
                                    await InventoryManipulator.removeById(c, type, loseItem.Id, qty, false, false);

                                    if (loseItem.Id == ItemId.ARPQ_SPIRIT_JEWEL)
                                    {
                                        chr.updateAriantScore();
                                    }

                                    for (byte b = 0; b < qty; b++)
                                    {
                                        pos.X = playerpos + ((d % 2 == 0) ? (25 * (d + 1) / 2) : -(25 * (d / 2)));
                                        await map.spawnItemDrop(chr, chr, ItemInformationProvider.getInstance().GenerateVirtualItemById(loseItem.Id, 1), map.calcDropPos(pos, chr.getPosition()), true, true);
                                        d++;
                                    }
                                }
                            }
                        }
                        // 应该没有必要 有loseItem也有selfDestruction
                        await map.RemoveMapObject(attacker, null);
                    }
                }
            }
            catch (InvalidCastException e)
            {
                //this happens due to mob on last map damaging player just before changing maps
                _logger.LogWarning(e, "Attack is not a mob-type, rather is a {MapObject} entity", map.getMapObject(oid)?.GetType()?.Name);
                return;
            }

            // Encode1(v137) - 击退方向 (0=左, 1=右, 根据怪物位置计算)
            direction = p.readByte();

            // Encode1(v189) - 伤害反击+魔法反击
            //   0=无反击
            pgmr = p.readByte();

            // Encode1(!v181 ? 0 : v175 + 1) - 寒冰掌/格挡反击
            //   v181 = !((job/10000)==190||==(193)) 即未使用坐骑
            //   v175 = (attackInfo==0) 即是否近战攻击
            //   返回值: 0=不满足条件, 1=格挡但未触发击晕, 2=OK
            guard = p.ReadSByte();

            // if (v175 || v189) - 当 guard>0 或 pgmr>0 时发送反击数据
            if (pgmr > 0 || guard > 0)
            {
                // Encode1(v189 != 0 && a10 != 0) - 是否伤害反击（用于区分本次反击是伤害反击还是魔法反击）
                is_pg = p.readByte() > 0;

                // Encode4(reflectOId) - 反击目标怪物对象ID
                var reflectOId = p.readInt();

                // Encode1(v188) - 受击动作编号
                action = p.readByte();

                // Encode2(LOWORD(v194)) - 命中点X坐标 (怪物攻击命中的位置)
                pos_x = p.readShort();

                // Encode2(HIWORD(v194)) - 命中点Y坐标
                pos_y = p.readShort();

                // Encode2(*v140) - 角色X坐标 (用于受击动画效果)
                // Encode2(*(v141 + 4)) - 角色Y坐标 (用于受击动画效果)
                // 未使用
                p.skip(4);

                var targetMob = map.getMonsterByOid(reflectOId);
                if (targetMob != null && targetMob.isAlive())
                {
                    if (guard > 1)
                    {
                        // 寒冰掌
                        foreach (var skill in SkillIds.GuardSkills)
                        {
                            var skillEffect = chr.TryGetPlayerSkillEffect(skill);
                            if (skillEffect != null)
                            {
                                await targetMob.applyStatus(chr, new MonsterStatusEffect(skillEffect.getMonsterStati(), skillEffect.GetSkill()!), skillEffect.isPoison(), skillEffect.getDuration());

                                // await chr.BroadcastMap(EffectPacket.ForeignSkillSpecial(chr.Id, skillEffect.getSourceId()), chr.Id);
                                break;
                            }
                        }
                    }

                    if (!is_pg)
                    {
                        // 魔法反击 概率在客户端计算过了
                        var manaReflection = chr.GetBuffStatValue(BuffStat.MANA_REFLECTION);
                        if (damagefrom == 0 && manaReflection != null && chr.CheckBuff(manaReflection) && damage > 0 && !attacker.isBoss())
                        {
                            if (pgmr != manaReflection.Effect.getX())
                            {
                                await chr.getAutobanManager().addPoint(AutobanFactory.PACKET_EDIT, "传输的技能效果与实际拥有的技能效果不一致（篡改wz）");
                            }
                            int bouncedamage = (int)Math.Min((damage * manaReflection.Effect.getX() / 100.0), attacker.getMaxHp() / 5);
                            await attacker.DamageBy(chr, bouncedamage, 0);
                            await attacker.BroadcastMap(PacketCreator.damageMonster(oid, bouncedamage));

                            // await chr.SendPacket(EffectPacket.SkillSpecial(manaReflection.Effect.getSourceId()));
                            await chr.BroadcastMap(EffectPacket.ForeignSkillSpecial(chr.Id, manaReflection.Effect.getSourceId()), chr.Id);
                        }
                    }
                    else
                    {
                        // 伤害反击
                        var powerGuard = chr.GetBuffStatValue(BuffStat.POWERGUARD);
                        if (powerGuard != null && damage > 0 && chr.CheckBuff(powerGuard))
                        {
                            if (pgmr != powerGuard.Effect.getX())
                            {
                                await chr.getAutobanManager().addPoint(AutobanFactory.PACKET_EDIT, "传输的技能效果与实际拥有的技能效果不一致（篡改wz）");
                            }
                            // PG works on bosses, but only at half of the rate.
                            var bouncedamage = (int)Math.Min(damage * (powerGuard.Effect.getX() / (attacker.isBoss() ? 200.0 : 100.0)), attacker.getMaxHp() / 10);

                            finalDamage -= bouncedamage;
                            await attacker.DamageBy(chr, bouncedamage, 0);
                            await attacker.BroadcastMap(PacketCreator.damageMonster(oid, bouncedamage));

                            await attacker.aggroMonsterDamage(chr, bouncedamage);
                        }
                    }
                }
            }

            // Encode1(v182) - 稳如泰山
            stance = p.readByte();

            if (damagefrom == -1)
            {
                // 抗压 也能反击？
                var bPressure = chr.getBuffEffect(BuffStat.BODY_PRESSURE); // thanks Atoot for noticing an issue on Body Pressure neutralise
                if (bPressure != null && damage > 0 && !attacker.isBoss())
                {
                    if (!attacker.alreadyBuffedStats().Contains(MonsterStatus.NEUTRALISE))
                    {
                        if (bPressure.makeChanceResult())
                        {
                            await attacker.applyStatus(chr, new MonsterStatusEffect(Collections.singletonMap(MonsterStatus.NEUTRALISE, 1), bPressure.GetSkill()!), false, bPressure.getX(), false);
                        }
                    }
                }
            }
            else
            {
                var attackInfo = attacker.AttackInfoHolders.GetValueOrDefault(damagefrom);
                if (attackInfo != null)
                {
                    if (attackInfo.DeadlyAttack)
                    {
                        mpattack = chr.MP - 1;
                        is_deadly = true;
                    }
                    mpattack += attackInfo.MpBurn;

                    var possibleMobSkill = MobSkillFactory.GetMobSkill(attackInfo.Disease, attackInfo.Level);
                    if (possibleMobSkill != null && damage > 0)
                    {
                        await possibleMobSkill.applyEffect(chr, attacker, false, banishPlayers);
                    }

                    attacker.setMp(attacker.getMp() - attackInfo.ConMP);
                }
            }
        }
        else  // --- 分支2: 无怪物 (a7 == NULL) ---
        {
            // Encode1((a11==0)-3) - damagefrom: a11=0时为-3, a11!=0时为-2
            // damagefrom = -2: 无怪物但有攻击来源 反击
            // damagefrom = -3: 完全无攻击来源
            // (damagefrom 已在前面读取)

            // Encode1(0) - element 始终为0 (无怪物时无元素)
            // (element 已在前面读取)

            // Encode4(v131) - damage 伤害值
            // (damage 已在前面读取)

            // Encode2(a6) - 额外字段 (无怪物时的特殊标记, 如毒伤等)
            int extraField = p.readShort();

            // Encode1(v182) - 稳如泰山
            stance = p.readByte();
        }

        if (damage == -1)
        {
            // 假动作、寒冰掌 都能让damage = -1
            foreach (var guardSkill in SkillIds.AllGuardSkills)
            {
                if (chr.TryGetPlayerSkillEffect(guardSkill) != null)
                {
                    fake = guardSkill;
                    break;
                }
            }
            // 没有相应技能却触发damage=-1
            if (fake == 0)
            {
                chr.getAutobanManager().addMiss();
            }
        }
        else if (damage > 0)
        {
            await chr.getAutobanManager().resetMisses();
        }
        else if (damage == 0)
        {
            chr.getAutobanManager().addMiss();
        }

        //in dojo player cannot use pot, so deadly attacks should be turned off as well
        if (is_deadly && MapId.isDojo(chr.getMap().getId()) && !YamlConfig.config.server.USE_DEADLY_DOJO)
        {
            damage = 0;
            mpattack = 0;
        }

        if (damage > 0 && !chr.isHidden())
        {
            if (attacker != null)
            {
                var cBarrier = chr.getBuffEffect(BuffStat.COMBO_BARRIER);  // thanks BHB for noticing Combo Barrier buff not working
                if (cBarrier != null)
                {
                    finalDamage -= damage * (1 - ((cBarrier.getX() / 1000.0)));
                }
            }

            if (damagefrom != -3 && damagefrom != -4)
            {
                foreach (var skill in SkillIds.AchillesSkills)
                {
                    var achilles = chr.TryGetPlayerSkillEffect(skill);
                    if (achilles != null)
                    {
                        finalDamage -= damage * (1 - (achilles.getX() / 1000.0));
                        break;
                    }
                }

                var highDefEffect = chr.TryGetPlayerSkillEffect(Aran.HIGH_DEFENSE);
                if (highDefEffect != null)
                {
                    finalDamage -= damage * (1 - ((highDefEffect.getX() / 1000.0)));
                }
            }

            int? buffValue = null;
            finalDamageValue = (int)Math.Ceiling(finalDamage);
            // 魔法盾
            if ((buffValue = chr.getBuffedValue(BuffStat.MAGIC_GUARD)) != null && mpattack == 0)
            {
                int mploss = (int)(finalDamageValue * (buffValue.Value / 100.0));
                int hploss = finalDamageValue - mploss;

                int curmp = chr.MP;
                if (mploss > curmp)
                {
                    hploss += mploss - curmp;
                    mploss = curmp;
                }

                await chr.UpdateStatsChunk(async () =>
                {
                    await chr.DamageBy(attacker, hploss, 0);
                    chr.ChangeMP(-mploss);
                });
            }
            // 金钱盾
            else if ((buffValue = chr.getBuffedValue(BuffStat.MESOGUARD)) != null)
            {
                finalDamage = Math.Round(finalDamage / 2);
                int mesoloss = (int)(finalDamage * (buffValue.Value / 100.0));

                finalDamageValue = (int)Math.Ceiling(finalDamage);
                if (chr.getMeso() < mesoloss)
                {
                    await chr.GainMeso(-chr.getMeso());
                    await chr.cancelBuffStats(BuffStat.MESOGUARD);
                }
                else
                {
                    await chr.GainMeso(-mesoloss);
                }
                await chr.UpdateStatsChunk(async () =>
                {
                    await chr.DamageBy(attacker, finalDamageValue, 0);
                    chr.ChangeMP(-mpattack);
                });
            }
            else
            {
                if (chr.isRidingBattleship())
                {
                    await chr.decreaseBattleshipHp(finalDamageValue);
                }

                await chr.UpdateStatsChunk(async () =>
                {
                    await chr.DamageBy(attacker, finalDamageValue, 0);
                    chr.ChangeMP(-mpattack);
                });
            }
        }
        await chr.Notice($"AttackIndex: {damagefrom}, damage: {damage}, finalDamage: {finalDamageValue}, pgmr:{pgmr}, guard:{guard}, is_pg: {is_pg}");
        await chr.BroadcastMap(PacketCreator.DamagePlayer(chr.getId(), damagefrom, mobId, damage, finalDamageValue, fake, direction, pgmr, is_pg, oid, action, pos_x, pos_y, stance), chr.Id);
        if (MapId.isDojo(map.getId()))
        {
            chr.setDojoEnergy(chr.getDojoEnergy() + YamlConfig.config.server.DOJO_ENERGY_DMG);
            await c.SendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
        }

        foreach (var player in banishPlayers)
        {
            // chill, if this list ever gets non-empty an attacker does exist, trust me :)
            await player.ChangeMapBanish(attacker?.SourceTemplate?.Ban);
        }
    }
}
