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

using Application.Templates.Map;

namespace server.maps;



public class PortalFactory
{
    private int nextDoorPortal;

    public PortalFactory()
    {
        nextDoorPortal = 0x80 + 1;
    }

    public Portal makePortal(int type, MapPortalTemplate portal)
    {
        GenericPortal? myPortal = null;
        if (type == PortalConstants.MAP_PORTAL)
        {
            myPortal = new MapPortal();
        }
        else if (type == PortalConstants.DOOR_PORTAL)
            myPortal = new MysticDoorPortal();
        else
        {
            myPortal = new GenericPortal(type);
        }
        myPortal.setName(portal.sPortalName);
        myPortal.setTarget(portal.sTargetName);
        myPortal.setTargetMapId(portal.nTargetMap);
        myPortal.setPosition(new Point(portal.nX, portal.nY));
        myPortal.setScriptName(portal.Script);
        if (myPortal.getType() == PortalConstants.DOOR_PORTAL)
        {
            myPortal.setId(nextDoorPortal);
            nextDoorPortal++;
        }
        else
        {
            myPortal.setId(portal.nIndex);
        }
        return myPortal;
    }
}
