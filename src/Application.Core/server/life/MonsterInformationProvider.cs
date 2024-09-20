/*
 This file is part of the OdinMS Maple Story Server
 Copyright (C) 2008 ~ 2010 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License version 3
 as published by the Free Software Foundation. You may not use, modify
 or distribute this program under any other version of the
 GNU Affero General Public License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using constants.inventory;
using provider;
using provider.wz;
using System.Collections.Concurrent;
using tools;

namespace server.life;


public class MonsterInformationProvider
{
    private static ILogger log = LogFactory.GetLogger("MonsterInformationProvider");
    // Author : LightPepsi

    private static MonsterInformationProvider instance = new MonsterInformationProvider();

    public static MonsterInformationProvider getInstance()
    {
        return instance;
    }

    private Dictionary<int, List<MonsterDropEntry>> drops = new();
    private List<MonsterGlobalDropEntry> globaldrops = new();
    private Dictionary<int, List<MonsterGlobalDropEntry>> continentdrops = new();

    private Dictionary<int, List<int>> dropsChancePool = new();    // thanks to ronan
    private HashSet<int> hasNoMultiEquipDrops = new();
    private Dictionary<int, List<MonsterDropEntry>> extraMultiEquipDrops = new();

    private ConcurrentDictionary<KeyValuePair<int, int>, int> mobAttackAnimationTime = new();
    private ConcurrentDictionary<MobSkill, int> mobSkillAnimationTime = new();

    private ConcurrentDictionary<int, KeyValuePair<int, int>> mobAttackInfo = new();

    private Dictionary<int, bool> mobBossCache = new();
    private Dictionary<int, string> mobNameCache = new();

    protected MonsterInformationProvider()
    {
        retrieveGlobal();
    }

    public List<MonsterGlobalDropEntry> getRelevantGlobalDrops(int mapid)
    {
        int continentid = mapid / 100000000;

        var contiItems = continentdrops.GetValueOrDefault(continentid);
        if (contiItems == null)
        {   // continent separated global drops found thanks to marcuswoon
            contiItems = new();

            foreach (MonsterGlobalDropEntry e in globaldrops)
            {
                if (e.continentid < 0 || e.continentid == continentid)
                {
                    contiItems.Add(e);
                }
            }

            continentdrops.AddOrUpdate(continentid, contiItems);
        }

        return contiItems;
    }

    private void retrieveGlobal()
    {
        try
        {
            using var dbContext = new DBContext();
            var dataList = dbContext.DropDataGlobals.Where(x => x.Chance > 0).ToList()
                .Select(x => new MonsterGlobalDropEntry(x.Itemid, x.Chance, x.Continent, x.MinimumQuantity, x.MaximumQuantity, (short)x.Questid));

            globaldrops.AddRange(dataList);
        }
        catch (Exception e)
        {
            log.Error(e, "Error retrieving global drops");
        }
    }

    public List<MonsterDropEntry> retrieveEffectiveDrop(int monsterId)
    {
        // this reads the drop entries searching for multi-equip, properly processing them

        List<MonsterDropEntry> list = retrieveDrop(monsterId);
        if (hasNoMultiEquipDrops.Contains(monsterId) || !YamlConfig.config.server.USE_MULTIPLE_SAME_EQUIP_DROP)
        {
            return list;
        }

        var multiDrops = extraMultiEquipDrops.GetValueOrDefault(monsterId);
        List<MonsterDropEntry> extra = new();
        if (multiDrops == null)
        {
            multiDrops = new();

            foreach (MonsterDropEntry mde in list)
            {
                if (ItemConstants.isEquipment(mde.itemId) && mde.Maximum > 1)
                {
                    multiDrops.Add(mde);

                    int rnd = Randomizer.rand(mde.Minimum, mde.Maximum);
                    for (int i = 0; i < rnd - 1; i++)
                    {
                        extra.Add(mde);   // this passes copies of the equips' MDE with min/max quantity > 1, but idc on equips they are unused anyways
                    }
                }
            }

            if (multiDrops.Count > 0)
            {
                extraMultiEquipDrops.AddOrUpdate(monsterId, multiDrops);
            }
            else
            {
                hasNoMultiEquipDrops.Add(monsterId);
            }
        }
        else
        {
            foreach (MonsterDropEntry mde in multiDrops)
            {
                int rnd = Randomizer.rand(mde.Minimum, mde.Maximum);
                for (int i = 0; i < rnd - 1; i++)
                {
                    extra.Add(mde);
                }
            }
        }

        List<MonsterDropEntry> ret = new(list);
        ret.AddRange(extra);

        return ret;
    }

    public List<MonsterDropEntry> retrieveDrop(int monsterId)
    {
        if (drops.ContainsKey(monsterId))
        {
            return drops[monsterId];
        }
        List<MonsterDropEntry> ret = new();

        try
        {
            using var dbContext = new DBContext();
            var ds = dbContext.DropData.Where(x => x.Dropperid == monsterId).Select(x => new { x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, x.Questid }).ToList();
            ret = ds.Select(x => new MonsterDropEntry(x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, (short)x.Questid)).ToList();

        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return ret;
        }

        drops.AddOrUpdate(monsterId, ret);
        return ret;
    }

    public List<int> retrieveDropPool(int monsterId)
    {  // ignores Quest and Party Quest items
        if (dropsChancePool.ContainsKey(monsterId))
        {
            return dropsChancePool[monsterId];
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        List<MonsterDropEntry> dropList = retrieveDrop(monsterId);
        List<int> ret = new();

        int accProp = 0;
        foreach (MonsterDropEntry mde in dropList)
        {
            if (!ii.isQuestItem(mde.itemId) && !ii.isPartyQuestItem(mde.itemId))
            {
                accProp += mde.chance;
            }

            ret.Add(accProp);
        }

        if (accProp == 0)
        {
            ret.Clear();    // don't accept mobs dropping no relevant items
        }
        dropsChancePool.AddOrUpdate(monsterId, ret);
        return ret;
    }

    public void setMobAttackAnimationTime(int monsterId, int attackPos, int animationTime)
    {
        mobAttackAnimationTime.AddOrUpdate(new(monsterId, attackPos), animationTime);
    }

    public int getMobAttackAnimationTime(int monsterId, int attackPos)
    {
        return mobAttackAnimationTime.GetValueOrDefault(new(monsterId, attackPos));
    }

    public void setMobSkillAnimationTime(MobSkill skill, int animationTime)
    {
        mobSkillAnimationTime.AddOrUpdate(skill, animationTime);
    }

    public int getMobSkillAnimationTime(MobSkill skill)
    {
        return mobSkillAnimationTime.GetValueOrDefault(skill);
    }

    public void setMobAttackInfo(int monsterId, int attackPos, int mpCon, int coolTime)
    {
        mobAttackInfo.AddOrUpdate((monsterId << 3) + attackPos, new(mpCon, coolTime));
    }

    public KeyValuePair<int, int>? getMobAttackInfo(int monsterId, int attackPos)
    {
        if (attackPos < 0 || attackPos > 7)
        {
            return null;
        }
        return mobAttackInfo.GetValueOrDefault((monsterId << 3) + attackPos);
    }

    public static List<KeyValuePair<int, string>> getMobsIDsFromName(string search)
    {
        DataProvider dataProvider = DataProviderFactory.getDataProvider(WZFiles.STRING);
        List<KeyValuePair<int, string>> retMobs = new();
        var data = dataProvider.getData("Mob.img");
        List<KeyValuePair<int, string>> mobPairList = new();
        foreach (var mobIdData in data.getChildren())
        {
            int mobIdFromData = int.Parse(mobIdData.getName());
            string mobNameFromData = DataTool.getString(mobIdData.getChildByPath("name")) ?? "NO-NAME";
            mobPairList.Add(new(mobIdFromData, mobNameFromData));
        }
        foreach (var mobPair in mobPairList)
        {
            if (mobPair.Value.ToLower().Contains(search.ToLower()))
            {
                retMobs.Add(mobPair);
            }
        }
        return retMobs;
    }

    public bool isBoss(int id)
    {
        var boss = mobBossCache.get(id);
        if (boss == null)
        {
            try
            {
                boss = LifeFactory.getMonster(id)?.isBoss() ?? false;
            }
            catch (Exception e)
            {   //nonexistant mob
                boss = false;

                log.Warning(e, "Non-existent mob id {MobId}", id);
            }

            mobBossCache.AddOrUpdate(id, boss ?? false);
        }

        return boss ?? false;
    }

    public string getMobNameFromId(int id)
    {
        var mobName = mobNameCache.GetValueOrDefault(id);
        if (mobName == null)
        {
            DataProvider dataProvider = DataProviderFactory.getDataProvider(WZFiles.STRING);
            var mobData = dataProvider.getData("Mob.img");

            mobName = DataTool.getString(mobData.getChildByPath(id + "/name")) ?? "";
            mobNameCache.AddOrUpdate(id, mobName);
        }

        return mobName;
    }

    public void clearDrops()
    {
        drops.Clear();
        hasNoMultiEquipDrops.Clear();
        extraMultiEquipDrops.Clear();
        dropsChancePool.Clear();
        globaldrops.Clear();
        continentdrops.Clear();
        retrieveGlobal();
    }
}
