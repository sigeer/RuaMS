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


using provider;
using provider.wz;


namespace server.life;

/**
 * @author Danny (Leifde)
 */
public class MobSkillFactory
{
    private static Dictionary<string, MobSkill> mobSkills = new();
    private static DataProvider dataSource = DataProviderFactory.getDataProvider(WZFiles.SKILL);
    private static Data skillRoot = dataSource.getData("MobSkill.img");
    private static ReaderWriterLockSlim mainLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public static MobSkill getMobSkillOrThrow(MobSkillType type, int level)
    {
        return getMobSkill(type, level) ?? throw new BusinessResException($"No MobSkill exists for type {type}, level {level}"); ;
    }

    public static MobSkill? getMobSkill(MobSkillType type, int level)
    {
        mainLock.EnterReadLock();
        try
        {
            var m = mobSkills.GetValueOrDefault(createKey(type, level));
            if (m != null)
                return m;
        }
        finally
        {
            mainLock.ExitReadLock();
        }

        return loadMobSkill(type, level);
    }

    private static MobSkill? loadMobSkill(MobSkillType type, int level)
    {
        mainLock.EnterWriteLock();
        try
        {
            MobSkill? existingMs = mobSkills.GetValueOrDefault(createKey(type, level));
            if (existingMs != null)
            {
                return existingMs;
            }

            var skillData = skillRoot.getChildByPath($"{type.getId()}/level/{level}");
            if (skillData == null)
            {
                return null;
            }

            int mpCon = DataTool.getInt("mpCon", skillData, 0);
            List<int> toSummon = new();
            for (int i = 0; i > -1; i++)
            {
                if (skillData.getChildByPath(i.ToString()) == null)
                {
                    break;
                }
                toSummon.Add(DataTool.getInt(skillData.getChildByPath(i.ToString()), 0));
            }
            int effect = DataTool.getInt("summonEffect", skillData, 0);
            int hp = DataTool.getInt("hp", skillData, 100);
            int x = DataTool.getInt("x", skillData, 1);
            int y = DataTool.getInt("y", skillData, 1);
            int count = DataTool.getInt("count", skillData, 1);
            long duration = DataTool.getInt("time", skillData, 0) * 1000;
            long cooltime = DataTool.getInt("interval", skillData, 0) * 1000;
            int iprop = DataTool.getInt("prop", skillData, 100);
            float prop = iprop / 100;
            int limit = DataTool.getInt("limit", skillData, 0);

            var ltData = skillData.getChildByPath("lt");
            var rbData = skillData.getChildByPath("rb");
            Point? lt = null;
            Point? rb = null;
            if (ltData != null && rbData != null)
            {
                lt = (Point?)ltData.getData();
                rb = (Point?)rbData.getData();
            }

            MobSkill loadedMobSkill = new MobSkill.Builder(type, level)
                    .mpCon(mpCon)
                    .toSummon(toSummon)
                    .cooltime(cooltime)
                    .duration(duration)
                    .spawnEffect(effect)
                    .hp(hp)
                    .x(x)
                    .y(y)
                    .count(count)
                    .prop(prop)
                    .limit(limit)
                    .lt(lt)
                    .rb(rb)
                    .build();

            mobSkills.AddOrUpdate(createKey(type, level), loadedMobSkill);
            return loadedMobSkill;
        }
        finally
        {
            mainLock.ExitWriteLock();
        }
    }

    private static string createKey(MobSkillType type, int skillLevel)
    {
        return type.getId() + "" + skillLevel;
    }
}
