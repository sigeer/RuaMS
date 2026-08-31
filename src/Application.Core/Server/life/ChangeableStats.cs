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

namespace Application.Core.Server.life;

/// <summary>
/// 仅靠服务端只能修改hp, mp, exp
/// </summary>
public class ChangeableStats : OverrideMonsterStats
{

    public ChangeableStats(MonsterStats stats, int newLevel, bool pqMob)
    { 
        // here we go i think
        double mod = newLevel / (double)stats.getLevel();
        double hpRatio = stats.MaxHP / (double)stats.getExp();
        double pqMod = (pqMob ? 1.5 : 1.0); // god damn
        Hp = Math.Min((int)Math.Round((!stats.isBoss() ? GameConstants.getMonsterHP(newLevel) : (stats.MaxHP * mod)) * pqMod), int.MaxValue); // right here lol
        Exp = Math.Min((int)Math.Round((!stats.isBoss() ? (GameConstants.getMonsterHP(newLevel) / hpRatio) : (stats.getExp())) * pqMod), int.MaxValue);
        Mp = Math.Min((int)Math.Round(stats.MaxMP * mod * pqMod), int.MaxValue);
    }

    public ChangeableStats(MonsterStats stats, float statModifier, bool pqMob) : this(stats, (int)(statModifier * stats.getLevel()), pqMob)
    {

    }
}
