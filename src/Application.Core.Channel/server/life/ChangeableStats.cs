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


using constants.game;

namespace server.life;

public class ChangeableStats : OverrideMonsterStats
{

    public int watk, matk, wdef, mdef, level;

    public ChangeableStats(MonsterStats stats, OverrideMonsterStats ostats)
    {
        hp = ostats.getHp();
        exp = ostats.getExp();
        mp = ostats.getMp();
        watk = stats.getPADamage();
        matk = stats.getMADamage();
        wdef = stats.getPDDamage();
        mdef = stats.getMDDamage();
        level = stats.getLevel();
    }

    public ChangeableStats(MonsterStats stats, int newLevel, bool pqMob)
    { // here we go i think
        double mod = newLevel / (double)stats.getLevel();
        double hpRatio = stats.getHp() / (double)stats.getExp();
        double pqMod = (pqMob ? 1.5 : 1.0); // god damn
        hp = Math.Min((int)Math.Round((!stats.isBoss() ? GameConstants.getMonsterHP(newLevel) : (stats.getHp() * mod)) * pqMod), int.MaxValue); // right here lol
        exp = Math.Min((int)Math.Round((!stats.isBoss() ? (GameConstants.getMonsterHP(newLevel) / hpRatio) : (stats.getExp())) * pqMod), int.MaxValue);
        mp = Math.Min((int)Math.Round(stats.getMp() * mod * pqMod), int.MaxValue);
        watk = Math.Min((int)Math.Round(stats.getPADamage() * mod), int.MaxValue);
        matk = Math.Min((int)Math.Round(stats.getMADamage() * mod), int.MaxValue);
        wdef = Math.Min(Math.Min(stats.isBoss() ? 30 : 20, (int)Math.Round(stats.getPDDamage() * mod)), int.MaxValue);
        mdef = Math.Min(Math.Min(stats.isBoss() ? 30 : 20, (int)Math.Round(stats.getMDDamage() * mod)), int.MaxValue);
        level = newLevel;
    }

    public ChangeableStats(MonsterStats stats, float statModifier, bool pqMob) : this(stats, (int)(statModifier * stats.getLevel()), pqMob)
    {

    }
}
