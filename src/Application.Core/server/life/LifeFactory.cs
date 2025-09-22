/*
This file is part of the OdinMS Maple Story Server
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
using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using Application.Resources;
using Application.Shared.Constants.Npc;
using Application.Shared.WzEntity;
using Application.Templates.Providers;
using Application.Templates.String;
using Application.Templates.XmlWzReader.Provider;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using tools;

namespace server.life;


public class LifeFactory : IStaticService
{
    private static LifeFactory? _instance;

    public static LifeFactory Instance => _instance ?? throw new BusinessFatalException("LifeFactory 未注册");

    public void Register(IServiceProvider sp)
    {
        if (_instance != null)
            return;

        _instance = sp.GetService<LifeFactory>() ?? throw new BusinessFatalException("LifeFactory 未注册");
    }


    private static ILogger log = LogFactory.GetLogger(LogType.LifeData);
    private static DataProvider data = DataProviderFactory.getDataProvider(WZFiles.MOB);

    private static ConcurrentDictionary<int, MonsterStats> monsterStats = new();


    private HashSet<int> hpbarBosses;

    readonly StringProvider _stringProvider;
    public LifeFactory()
    {
        hpbarBosses = getHpBarBosses();
        _stringProvider = ProviderFactory.GetProvider<StringProvider>();
    }

    private HashSet<int> getHpBarBosses()
    {
        HashSet<int> ret = new();
        Data uiDataWZ = DataProviderFactory.getDataProvider(WZFiles.UI).getData("UIWindow.img");
        foreach (var bossData in uiDataWZ.getChildByPath("MobGage/Mob").getChildren())
        {
            if (int.TryParse(bossData.getName(), out var d))
                ret.Add(d);
        }

        return ret;
    }

    public AbstractLifeObject? getLife(int id, string type)
    {
        if (type.Equals(LifeType.NPC, StringComparison.CurrentCultureIgnoreCase))
        {
            return getNPC(id);
        }
        else if (type.Equals(LifeType.Monster, StringComparison.CurrentCultureIgnoreCase))
        {
            return getMonster(id);
        }
        else
        {
            log.Warning("Unknown Life type: {LifeType}", type);
            return null;
        }
    }

    private void setMonsterAttackInfo(int mid, List<MobAttackInfoHolder> attackInfos)
    {
        if (attackInfos.Count > 0)
        {
            MonsterInformationProvider mi = MonsterInformationProvider.getInstance();

            foreach (MobAttackInfoHolder attackInfo in attackInfos)
            {
                mi.setMobAttackInfo(mid, attackInfo.attackPos, attackInfo.mpCon, attackInfo.coolTime);
                mi.setMobAttackAnimationTime(mid, attackInfo.attackPos, attackInfo.animationTime);
            }
        }
    }

    private MonsterCore? getMonsterStats(int mid)
    {
        var monsterData = data.getData(StringUtil.getLeftPaddedStr(mid + ".img", '0', 11));
        if (monsterData == null)
        {
            return null;
        }
        var monsterInfoData = monsterData.getChildByPath("info");

        List<MobAttackInfoHolder> attackInfos = new();
        MonsterStats stats = new MonsterStats();

        int linkMid = DataTool.getIntConvert("link", monsterInfoData, 0);
        if (linkMid != 0)
        {
            var linkStats = getMonsterStats(linkMid);
            if (linkStats == null)
            {
                return null;
            }

            // thanks resinate for noticing non-propagable infos such as revives getting retrieved
            attackInfos.AddRange(linkStats.AttackInfo);
        }

        stats.setHp(DataTool.getIntConvert("maxHP", monsterInfoData));
        stats.setFriendly(DataTool.getIntConvert("damagedByMob", monsterInfoData, stats.isFriendly() ? 1 : 0) == 1);
        stats.setPADamage(DataTool.getIntConvert("PADamage", monsterInfoData));
        stats.setPDDamage(DataTool.getIntConvert("PDDamage", monsterInfoData));
        stats.setMADamage(DataTool.getIntConvert("MADamage", monsterInfoData));
        stats.setMDDamage(DataTool.getIntConvert("MDDamage", monsterInfoData));
        stats.setMp(DataTool.getIntConvert("maxMP", monsterInfoData, stats.getMp()));
        stats.setExp(DataTool.getIntConvert("exp", monsterInfoData, stats.getExp()));
        stats.setLevel(DataTool.getIntConvert("level", monsterInfoData));
        stats.setRemoveAfter(DataTool.getIntConvert("removeAfter", monsterInfoData, stats.removeAfter()));
        stats.setBoss(DataTool.getIntConvert("boss", monsterInfoData, stats.isBoss() ? 1 : 0) > 0);
        stats.setExplosiveReward(DataTool.getIntConvert("explosiveReward", monsterInfoData, stats.isExplosiveReward() ? 1 : 0) > 0);
        stats.setFfaLoot(DataTool.getIntConvert("publicReward", monsterInfoData, stats.isFfaLoot() ? 1 : 0) > 0);
        stats.setUndead(DataTool.getIntConvert("undead", monsterInfoData, stats.isUndead() ? 1 : 0) > 0);
        stats.setName(_stringProvider.GetSubProvider(StringCategory.Mob).GetRequiredItem<StringTemplate>(mid)?.Name ?? StringConstants.WZ_MissingNo);
        stats.setBuffToGive(DataTool.getIntConvert("buff", monsterInfoData, stats.getBuffToGive()));
        stats.setCP(DataTool.getIntConvert("getCP", monsterInfoData, stats.getCP()));
        stats.setRemoveOnMiss(DataTool.getIntConvert("removeOnMiss", monsterInfoData, stats.removeOnMiss() ? 1 : 0) > 0);

        var special = monsterInfoData?.getChildByPath("coolDamage");
        if (special != null)
        {
            int coolDmg = DataTool.getIntConvert("coolDamage", monsterInfoData);
            int coolProb = DataTool.getIntConvert("coolDamageProb", monsterInfoData, 0);
            stats.setCool(new(coolDmg, coolProb));
        }

        special = monsterInfoData?.getChildByPath("loseItem");
        if (special != null)
        {
            foreach (Data liData in special.getChildren())
            {
                stats.addLoseItem(new LoseItem(DataTool.getInt(liData.getChildByPath("id")), (byte)DataTool.getInt(liData.getChildByPath("prop")), (byte)DataTool.getInt(liData.getChildByPath("x"))));
            }
        }

        special = monsterInfoData?.getChildByPath("selfDestruction");
        if (special != null)
        {
            stats.setSelfDestruction(new SelfDestruction((byte)DataTool.getInt(special.getChildByPath("action")), DataTool.getIntConvert("removeAfter", special, -1), DataTool.getIntConvert("hp", special, -1)));
        }

        var firstAttackData = monsterInfoData?.getChildByPath("firstAttack");
        int firstAttack = 0;
        if (firstAttackData != null)
        {
            if (firstAttackData.DataType == DataType.FLOAT)
            {
                firstAttack = (int)Math.Round(DataTool.getFloat(firstAttackData));
            }
            else
            {
                firstAttack = DataTool.getInt(firstAttackData);
            }
        }
        stats.setFirstAttack(firstAttack > 0);
        stats.setDropPeriod(DataTool.getIntConvert("dropItemPeriod", monsterInfoData, stats.getDropPeriod() / 10000) * 10000);

        // thanks yuxaij, Riizade, Z1peR, Anesthetic for noticing some bosses crashing players due to missing requirements
        bool hpbarBoss = stats.isBoss() && hpbarBosses.Contains(mid);
        stats.setTagColor(hpbarBoss ? DataTool.getIntConvert("hpTagColor", monsterInfoData, 0) : 0);
        stats.setTagBgColor(hpbarBoss ? DataTool.getIntConvert("hpTagBgcolor", monsterInfoData, 0) : 0);

        foreach (Data idata in monsterData)
        {
            var idataName = idata.getName();
            if (idataName != "info")
            {
                int delay = 0;
                foreach (Data pic in idata.getChildren())
                {
                    delay += DataTool.getIntConvert("delay", pic, 0);
                }
                stats.setAnimationTime(idataName, delay);
            }
        }
        var reviveInfo = monsterInfoData?.getChildByPath("revive");
        if (reviveInfo != null)
        {
            List<int> revives = new();
            foreach (Data data_ in reviveInfo)
            {
                revives.Add(DataTool.getInt(data_));
            }
            stats.setRevives(revives);
        }
        decodeElementalString(stats, DataTool.getString("elemAttr", monsterInfoData) ?? "");

        MonsterInformationProvider mi = MonsterInformationProvider.getInstance();
        var monsterSkillInfoData = monsterInfoData?.getChildByPath("skill");
        if (monsterSkillInfoData != null)
        {
            int localI = 0;
            HashSet<MobSkillId> skills = new();
            while (monsterSkillInfoData.getChildByPath(localI.ToString()) != null)
            {
                int skillId = DataTool.getInt(localI + "/skill", monsterSkillInfoData, 0);
                int skillLv = DataTool.getInt(localI + "/level", monsterSkillInfoData, 0);
                MobSkillType type = MobSkillTypeUtils.from(skillId);
                skills.Add(new MobSkillId(type, skillLv));

                var monsterSkillData = monsterData.getChildByPath("skill" + (localI + 1));
                if (monsterSkillData != null)
                {
                    int animationTime = 0;
                    foreach (Data effectEntry in monsterSkillData.getChildren())
                    {
                        animationTime += DataTool.getIntConvert("delay", effectEntry, 0);
                    }

                    MobSkill skill = MobSkillFactory.getMobSkillOrThrow(type, skillLv);
                    mi.setMobSkillAnimationTime(skill, animationTime);
                }

                localI++;
            }
            stats.setSkills(skills);
        }

        int i = 0;
        Data? monsterAttackData;
        while ((monsterAttackData = monsterData.getChildByPath("attack" + (i + 1))) != null)
        {
            int animationTime = 0;
            foreach (Data effectEntry in monsterAttackData.getChildren())
            {
                animationTime += DataTool.getIntConvert("delay", effectEntry, 0);
            }

            int mpCon = DataTool.getIntConvert("info/conMP", monsterAttackData, 0);
            int coolTime = DataTool.getIntConvert("info/attackAfter", monsterAttackData, 0);
            attackInfos.Add(new MobAttackInfoHolder(i, mpCon, coolTime, animationTime));
            i++;
        }

        var banishData = monsterInfoData?.getChildByPath("ban");
        if (banishData != null)
        {
            int map = DataTool.getInt("banMap/0/field", banishData, -1);
            string portal = DataTool.getString("banMap/0/portal", banishData) ?? "sp";
            string? msg = DataTool.getString("banMsg", banishData);
            stats.setBanishInfo(new BanishInfo(map, portal, msg));
        }

        int noFlip = DataTool.getInt("noFlip", monsterInfoData, 0);
        if (noFlip > 0)
        {
            var origin = DataTool.getPoint("stand/0/origin", monsterData);
            if (origin != null)
            {
                stats.setFixedStance(origin.Value.X < 1 ? 5 : 4);    // fixed left/right
            }
        }

        return new(stats, attackInfos);
    }

    public Monster? getMonster(int mid)
    {
        try
        {
            var stats = monsterStats.GetValueOrDefault(mid);
            if (stats == null)
            {
                var mobStats = getMonsterStats(mid);
                if (mobStats == null)
                    return null;

                stats = mobStats.Stats;
                setMonsterAttackInfo(mid, mobStats.AttackInfo);

                monsterStats.AddOrUpdate(mid, stats);
            }
            return new Monster(mid, stats);
        }
        catch (NullReferenceException npe)
        {
            log.Error(npe, "[SEVERE] MOB {MobId} failed to load.", mid);
            return null;
        }
    }

    public Monster GetMonsterTrust(int mid) => getMonster(mid) ?? throw new BusinessResException($"getMonster({mid})");

    public int getMonsterLevel(int mid)
    {
        try
        {
            var stats = monsterStats.GetValueOrDefault(mid);
            if (stats == null)
            {
                Data monsterData = data.getData(StringUtil.getLeftPaddedStr(mid + ".img", '0', 11));
                if (monsterData == null)
                {
                    return -1;
                }
                var monsterInfoData = monsterData.getChildByPath("info");
                return DataTool.getIntConvert("level", monsterInfoData);
            }
            else
            {
                return stats.getLevel();
            }
        }
        catch (NullReferenceException npe)
        {
            log.Error(npe, "[SEVERE] MOB {MobId} failed to load.", mid);
        }

        return -1;
    }

    private static void decodeElementalString(MonsterStats stats, string elemAttr)
    {
        for (int i = 0; i < elemAttr.Length; i += 2)
        {
            stats.setEffectiveness(Element.getFromChar(elemAttr.ElementAt(i)), ElementalEffectivenessUtils.getByNumber(int.Parse(elemAttr.ElementAt(i + 1).ToString())));
        }
    }

    public NPC getNPC(int nid)
    {
        return new NPC(nid, GetNPCStats(nid));
    }

    public string getNPCDefaultTalk(int nid)
    {
        return _stringProvider.GetSubProvider(StringCategory.Npc).GetRequiredItem<StringNpcTemplate>(nid)?.DefaultTalk ?? "(...)";
    }

    public NPCStats GetNPCStats(int npcId)
    {
        return new NPCStats(_stringProvider.GetSubProvider(StringCategory.Npc).GetRequiredItem<StringNpcTemplate>(npcId)?.Name ?? StringConstants.WZ_MissingNo);
    }
}
