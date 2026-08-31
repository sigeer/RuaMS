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

public abstract class OverrideMonsterStats
{

    public int Hp { get; set; }
    public int Exp { get; set; }
    public int Mp { get; set; }

    public OverrideMonsterStats()
    {
        Hp = 1;
        Exp = 0;
        Mp = 0;
    }

    public OverrideMonsterStats(int hp, int mp, int exp, bool change)
    {
        this.Hp = /*change ? (hp * 3L / 2L) : */ hp;
        this.Mp = mp;
        this.Exp = exp;
    }

    public OverrideMonsterStats(int hp, int mp, int exp) : this(hp, mp, exp, true)
    {

    }
}