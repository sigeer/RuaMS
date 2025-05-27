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


using Application.Core.Game.Life;
using System.Collections.Concurrent;

namespace server.life;


public class MonsterInformationProvider
{
    private ILogger log = LogFactory.GetLogger(LogType.MonsterData);
    // Author : LightPepsi

    private static MonsterInformationProvider instance = new MonsterInformationProvider();

    public static MonsterInformationProvider getInstance()
    {
        return instance;
    }

    private List<DropEntry> globaldrops = new();

    private HashSet<int> hasNoMultiEquipDrops = new();

    private ConcurrentDictionary<int, List<DropEntry>> continentdrops = new();
    private ConcurrentDictionary<int, List<DropEntry>> extraMultiEquipDrops = new();

    private ConcurrentDictionary<KeyValuePair<int, int>, int> mobAttackAnimationTime = new();
    private ConcurrentDictionary<MobSkill, int> mobSkillAnimationTime = new();

    private ConcurrentDictionary<int, KeyValuePair<int, int>> mobAttackInfo = new();

    private Dictionary<int, List<DropEntry>> drops = new();
    private Dictionary<int, List<int>> dropsChancePool = new();    // thanks to ronan
    private Dictionary<int, bool> mobBossCache = new();

    protected MonsterInformationProvider()
    {
        retrieveGlobal();
    }

    public List<DropEntry> getRelevantGlobalDrops(int mapid)
    {
        int continentid = mapid / 100000000;

        if (continentdrops.TryGetValue(continentid, out var contiItems))
            return contiItems;

        contiItems = globaldrops.Where(e => e.ContinentId < 0 || e.ContinentId == continentid).ToList();

        continentdrops[continentid] = contiItems;

        return contiItems;
    }

    private void retrieveGlobal()
    {
        try
        {
            using var dbContext = new DBContext();
            globaldrops = dbContext.DropDataGlobals.Where(x => x.Chance > 0).ToList()
                .Select(x => DropEntry.Global(x.Continent, x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, (short)x.Questid)).ToList();

            drops = dbContext.DropData
                .Select(x => new { x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, x.Questid, x.Dropperid })
                .ToList()
                .GroupBy(x => x.Dropperid)
                .Select(x => new KeyValuePair<int, List<DropEntry>>(
                    x.Key,
                    x.Select(y => DropEntry.MobDrop(y.Dropperid, y.Itemid, y.Chance, y.MinimumQuantity, y.MaximumQuantity, (short)y.Questid)).ToList()))
                .ToDictionary();
        }
        catch (Exception e)
        {
            log.Error(e, "Error retrieving global drops");
        }
    }

    public List<DropEntry> retrieveEffectiveDrop(int monsterId)
    {
        // this reads the drop entries searching for multi-equip, properly processing them

        List<DropEntry> list = retrieveDrop(monsterId);
        if (!YamlConfig.config.server.USE_MULTIPLE_SAME_EQUIP_DROP || hasNoMultiEquipDrops.Contains(monsterId))
        {
            return list;
        }

        var multiDrops = extraMultiEquipDrops.GetValueOrDefault(monsterId);
        List<DropEntry> extra = new();
        if (multiDrops == null)
        {
            multiDrops = new();

            foreach (var mde in list)
            {
                if (ItemConstants.isEquipment(mde.ItemId) && mde.MaxCount > 1)
                {
                    multiDrops.Add(mde);

                    int rnd = mde.GetRandomCount();
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
            foreach (var mde in multiDrops)
            {
                int rnd = mde.GetRandomCount();
                for (int i = 0; i < rnd - 1; i++)
                {
                    extra.Add(mde);
                }
            }
        }

        List<DropEntry> ret = new(list);
        ret.AddRange(extra);

        return ret;
    }
    /// <summary>
    /// 怪物所有的掉落物
    /// </summary>
    /// <param name="monsterId"></param>
    /// <returns></returns>
    public List<DropEntry> retrieveDrop(int monsterId)
    {
        if (drops.TryGetValue(monsterId, out var value))
            return value;

        List<DropEntry> ret = new();

        try
        {
            using var dbContext = new DBContext();
            var ds = dbContext.DropData.Where(x => x.Dropperid == monsterId).Select(x => new { x.Dropperid, x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, x.Questid }).ToList();
            ret = ds.Select(x => DropEntry.MobDrop(x.Dropperid, x.Itemid, x.Chance, x.MinimumQuantity, x.MaximumQuantity, (short)x.Questid)).ToList();

        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
            return ret;
        }

        drops.AddOrUpdate(monsterId, ret);
        return ret;
    }

    /// <summary>
    /// 怪物所有非任务道具掉落物概率
    /// </summary>
    /// <param name="monsterId"></param>
    /// <returns></returns>
    public List<int> retrieveDropPool(int monsterId)
    {
        // ignores Quest and Party Quest items
        if (dropsChancePool.ContainsKey(monsterId))
        {
            return dropsChancePool[monsterId];
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        var dropList = retrieveDrop(monsterId);
        List<int> ret = new();

        int accProp = 0;
        foreach (var mde in dropList)
        {
            if (!ii.isQuestItem(mde.ItemId) && !ii.isPartyQuestItem(mde.ItemId))
            {
                accProp += mde.Chance;
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

    Dictionary<int, string> allMobNameCache = [];
    private void LoadAllMobNameCache()
    {
        if (allMobNameCache.Count == 0)
        {
            DataProvider dataProvider = DataProviderFactory.getDataProvider(WZFiles.STRING);
            var data = dataProvider.getData("Mob.img");
            List<KeyValuePair<int, string>> mobPairList = new();
            foreach (var mobIdData in data.getChildren())
            {
                if (int.TryParse(mobIdData.getName(), out var mobIdFromData))
                {
                    string mobNameFromData = DataTool.getString(mobIdData.getChildByPath("name")) ?? "NO-NAME";
                    mobPairList.Add(new(mobIdFromData, mobNameFromData));

                    allMobNameCache[mobIdFromData] = mobNameFromData;
                }
            }
        }
    }
    public List<KeyValuePair<int, string>> getMobsIDsFromName(string search)
    {
        LoadAllMobNameCache();
        return allMobNameCache.Where(x => x.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public bool isBoss(int id)
    {
        if (!mobBossCache.TryGetValue(id, out var boss))
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

            mobBossCache.AddOrUpdate(id, boss);
        }

        return boss;
    }

    public string getMobNameFromId(int id)
    {
        LoadAllMobNameCache();

        var mobName = allMobNameCache.GetValueOrDefault(id);
        if (mobName == null)
        {
            DataProvider dataProvider = DataProviderFactory.getDataProvider(WZFiles.STRING);
            var mobData = dataProvider.getData("Mob.img");

            mobName = DataTool.getString(mobData.getChildByPath(id + "/name")) ?? "";
            allMobNameCache.AddOrUpdate(id, mobName);
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
