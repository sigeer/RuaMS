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
namespace net.server.world;






public class Messenger
{

    private int id;
    private List<MessengerCharacter> members = new(3);
    private bool[] pos = new bool[3];

    public Messenger(int id, MessengerCharacter chrfor)
    {
        this.id = id;
        for (int i = 0; i < 3; i++)
        {
            pos[i] = false;
        }
        addMember(chrfor, chrfor.getPosition());
    }

    public int getId()
    {
        return id;
    }

    public ICollection<MessengerCharacter> getMembers()
    {
        return members.ToList();
    }

    public void addMember(MessengerCharacter member, int position)
    {
        members.Add(member);
        member.setPosition(position);
        pos[position] = true;
    }

    public void removeMember(MessengerCharacter member)
    {
        int position = member.getPosition();
        pos[position] = false;
        members.Remove(member);
    }

    public int getLowestPosition()
    {
        for (byte i = 0; i < 3; i++)
        {
            if (!pos[i])
            {
                return i;
            }
        }
        return -1;
    }

    public int getPositionByName(string name)
    {
        foreach (MessengerCharacter messengerchar in members)
        {
            if (messengerchar.getName().Equals(name))
            {
                return messengerchar.getPosition();
            }
        }
        return -1;
    }
}

