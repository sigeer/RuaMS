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


using Application.Core.constants.game;
using Application.Core.Game.Life;
using Application.Core.model;
using provider;
using provider.wz;
using tools;

namespace server.life;


public class LifeFactory
{
    private static ILogger log = LogFactory.GetLogger("LifeFactory");
    private static DataProvider data = DataProviderFactory.getDataProvider(WZFiles.MOB);
    private static DataProvider stringDataWZ = DataProviderFactory.getDataProvider(WZFiles.STRING);
    private static Data mobStringData = stringDataWZ.getData("Mob.img");
    private static Data npcStringData = stringDataWZ.getData("Npc.img");
    private static Dictionary<int, MonsterStats> monsterStats = new();
    private static HashSet<int> hpbarBosses = getHpBarBosses();

    private static HashSet<int> getHpBarBosses()
    {
        HashSet<int> ret = new();

        DataProvider uiDataWZ = DataProviderFactory.getDataProvider(WZFiles.UI);
        foreach (var bossData in uiDataWZ.getData("UIWindow.img").getChildByPath("MobGage/Mob").getChildren())
        {
            ret.Add(int.Parse(bossData.getName()));
        }

        return ret;
    }

    public static AbstractLifeObject? getLife(int id, string type)
    {
        if (type.ToLower() == LifeType.NPC)
        {
            return getNPC(id);
        }
        else if (type.ToLower() == LifeType.Monster)
        {
            return getMonster(id);
        }
        else
        {
            log.Warning("Unknown Life type: {LifeType}", type);
            return null;
        }
    }

    private static void setMonsterAttackInfo(int mid, List<MobAttackInfoHolder> attackInfos)
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

    private static KeyValuePair<MonsterStats, List<MobAttackInfoHolder>>? getMonsterStats(int mid)
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
            attackInfos.AddRange(linkStats.Value.Value);
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
        stats.setName(DataTool.getString(mid + "/name", mobStringData) ?? "MISSINGNO");
        stats.setBuffToGive(DataTool.getIntConvert("buff", monsterInfoData, stats.getBuffToGive()));
        stats.setCP(DataTool.getIntConvert("getCP", monsterInfoData, stats.getCP()));
        stats.setRemoveOnMiss(DataTool.getIntConvert("removeOnMiss", monsterInfoData, stats.removeOnMiss() ? 1 : 0) > 0);

        var special = monsterInfoData.getChildByPath("coolDamage");
        if (special != null)
        {
            int coolDmg = DataTool.getIntConvert("coolDamage", monsterInfoData);
            int coolProb = DataTool.getIntConvert("coolDamageProb", monsterInfoData, 0);
            stats.setCool(new(coolDmg, coolProb));
        }
        special = monsterInfoData.getChildByPath("loseItem");
        if (special != null)
        {
            foreach (Data liData in special.getChildren())
            {
                stats.addLoseItem(new loseItem(DataTool.getInt(liData.getChildByPath("id")), (byte)DataTool.getInt(liData.getChildByPath("prop")), (byte)DataTool.getInt(liData.getChildByPath("x"))));
            }
        }
        special = monsterInfoData.getChildByPath("selfDestruction");
        if (special != null)
        {
            stats.setSelfDestruction(new selfDestruction((byte)DataTool.getInt(special.getChildByPath("action")), DataTool.getIntConvert("removeAfter", special, -1), DataTool.getIntConvert("hp", special, -1)));
        }
        var firstAttackData = monsterInfoData.getChildByPath("firstAttack");
        int firstAttack = 0;
        if (firstAttackData != null)
        {
            if (firstAttackData.getType() == DataType.FLOAT)
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
            if (!idata.getName().Equals("info"))
            {
                int delay = 0;
                foreach (Data pic in idata.getChildren())
                {
                    delay += DataTool.getIntConvert("delay", pic, 0);
                }
                stats.setAnimationTime(idata.getName(), delay);
            }
        }
        var reviveInfo = monsterInfoData.getChildByPath("revive");
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
        var monsterSkillInfoData = monsterInfoData.getChildByPath("skill");
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

        var banishData = monsterInfoData.getChildByPath("ban");
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

    public static Monster? getMonster(int mid)
    {
        try
        {
            var stats = monsterStats.GetValueOrDefault(mid);
            if (stats == null)
            {
                var mobStats = getMonsterStats(mid).GetValueOrDefault();
                stats = mobStats.Key;
                setMonsterAttackInfo(mid, mobStats.Value);

                monsterStats.Add(mid, stats);
            }
            return new Monster(mid, stats);
        }
        catch (NullReferenceException npe)
        {
            log.Error(npe, "[SEVERE] MOB {MobId} failed to load.", mid);
            return null;
        }
    }

    public static int getMonsterLevel(int mid)
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

    public static NPC getNPC(int nid)
    {
        return new NPC(nid, new NPCStats(DataTool.getString(nid + "/name", npcStringData) ?? "MISSINGNO"));
    }

    public static string getNPCDefaultTalk(int nid)
    {
        return DataTool.getString(nid + "/d0", npcStringData) ?? "(...)";
    }

    public class loseItem
    {

        private int id;
        private byte chance;
        private byte x;

        public loseItem(int id, byte chance, byte x)
        {
            this.id = id;
            this.chance = chance;
            this.x = x;
        }

        public int getId()
        {
            return id;
        }

        public byte getChance()
        {
            return chance;
        }

        public byte getX()
        {
            return x;
        }
    }

    public class selfDestruction
    {

        private byte action;
        private int _removeAfter;
        private int hp;

        public selfDestruction(byte action, int removeAfter, int hp)
        {
            this.action = action;
            this._removeAfter = removeAfter;
            this.hp = hp;
        }

        public int getHp()
        {
            return hp;
        }

        public byte getAction()
        {
            return action;
        }

        public int removeAfter()
        {
            return _removeAfter;
        }
    }
}
