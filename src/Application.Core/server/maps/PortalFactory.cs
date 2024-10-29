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

using XmlWzReader.WzEntity;

namespace server.maps;



public class PortalFactory
{
    private int nextDoorPortal;

    public PortalFactory()
    {
        nextDoorPortal = 0x80;
    }

    public Portal makePortal(MapPortalWz portal)
    {
        GenericPortal? ret = null;
        if (portal.Pt == PortalConstants.MAP_PORTAL)
        {
            ret = new MapPortal();
        }
        else
        {
            ret = new GenericPortal(portal.Pt);
        }
        loadPortal(ret, portal);
        return ret;
    }

    private void loadPortal(GenericPortal myPortal, MapPortalWz portal)
    {
        myPortal.setName(portal.Pn);
        myPortal.setTarget(portal.Tn);
        myPortal.setTargetMapId(portal.Tm);
        myPortal.setPosition(new Point(portal.X, portal.Y));
        myPortal.setScriptName(portal.Script);

        if (myPortal.getType() == PortalConstants.DOOR_PORTAL)
        {
            myPortal.setId(nextDoorPortal);
            nextDoorPortal++;
        }
        else
        {
            myPortal.setId(portal.PortalId);
        }
    }
}
