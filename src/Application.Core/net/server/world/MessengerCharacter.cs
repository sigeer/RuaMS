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

public class MessengerCharacter
{
    private string name;
    private int id;
    private int position;
    private int channel;
    private bool online;

    public MessengerCharacter(IPlayer maplechar, int position)
    {
        this.name = maplechar.getName();
        this.channel = maplechar.getClient().getChannel();
        this.id = maplechar.getId();
        this.online = true;
        this.position = position;
    }

    public int getId()
    {
        return id;
    }

    public int getChannel()
    {
        return channel;
    }

    public string getName()
    {
        return name;
    }

    public bool isOnline()
    {
        return online;
    }

    public int getPosition()
    {
        return position;
    }

    public void setPosition(int position)
    {
        this.position = position;
    }

    public override int GetHashCode()
    {
        int prime = 31;
        int result = 1;
        result = prime * result + ((name == null) ? 0 : name.GetHashCode());
        return result;
    }

    public override bool Equals(object? obj)
    {
        return obj is MessengerCharacter other && other.name == name;
    }
}
