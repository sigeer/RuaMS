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

using Application.Templates.Providers;
using Application.Templates.XmlWzReader.Provider;

namespace server.life;

/**
 * @author Danny (Leifde)
 */
public class MobSkillFactory
{
    private static Dictionary<string, MobSkill> mobSkills = new();
    private static ReaderWriterLockSlim mainLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    static MobSkillProvider _provider = ProviderFactory.GetProvider<MobSkillProvider>();

    public static MobSkill? GetMobSkill(int type, int level)
    {
        if (Enum.IsDefined(typeof(MobSkillType), type))
            return getMobSkill((MobSkillType)type, level);
        return null;
    }

    public static MobSkill getMobSkillOrThrow(MobSkillType type, int level)
    {
        return getMobSkill(type, level) ?? throw new BusinessResException($"No MobSkill exists for type {type}, level {level}"); ;
    }

    public static MobSkill? getMobSkill(MobSkillType type, int level)
    {
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

            var template = _provider.GetItem((int)type);
            if (template == null)
                return null;

            var levelData = template.GetLevelData(level);
            if (levelData == null)
                return null;

            MobSkill loadedMobSkill = new MobSkill.Builder(type, level)
                    .mpCon(levelData.MpCon)
                    .toSummon(levelData.SummonIDs.ToList())
                    .cooltime(levelData.Interval * 1000)
                    .duration(levelData.Time * 1000)
                    .spawnEffect(levelData.SummonEffect)
                    .hp(levelData.HP)
                    .x(levelData.X)
                    .y(levelData.Y)
                    .count(levelData.Count)
                    .prop(levelData.Prop / 100.0f)
                    .limit(levelData.Limit)
                    .lt(levelData.Lt)
                    .rb(levelData.Rb)
                    .build();

            return mobSkills[createKey(type, level)] = loadedMobSkill;
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
