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
namespace client;

public class SkillMacro
{
    public int Skill1 { get; set; }
    public int Skill2 { get; set; }
    public int Skill3 { get; set; }
    public string Name { get; set; }
    public int Shout { get; set; }
    public int Position { get; set; }

    public SkillMacro(int skill1, int skill2, int skill3, string name, int shout, int position)
    {
        this.Skill1 = skill1;
        this.Skill2 = skill2;
        this.Skill3 = skill3;
        this.Name = name;
        this.Shout = shout;
        this.Position = position;
    }

    public int getSkill1()
    {
        return Skill1;
    }

    public int getSkill2()
    {
        return Skill2;
    }

    public int getSkill3()
    {
        return Skill3;
    }

    public void setSkill1(int skill)
    {
        Skill1 = skill;
    }

    public void setSkill2(int skill)
    {
        Skill2 = skill;
    }

    public void setSkill3(int skill)
    {
        Skill3 = skill;
    }

    public string getName()
    {
        return Name;
    }

    public int getShout()
    {
        return Shout;
    }

    public int getPosition()
    {
        return Position;
    }
}
