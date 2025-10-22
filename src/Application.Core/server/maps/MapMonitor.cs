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


using Application.Core.Game.Maps;

namespace server.maps;



public class MapMonitor
{
    private ScheduledFuture? monitorSchedule;
    private IMap map;
    private Portal? portal;

    public MapMonitor(IMap map, string portal)
    {
        this.map = map;
        this.portal = map.getPortal(portal);
        this.monitorSchedule = map.ChannelServer.Container.TimerManager.register(() =>
        {
            if (map.getAllPlayers().Count < 1)
            {
                cancelAction();
            }
        }, 5000);
    }

    private void cancelAction()
    {
        if (monitorSchedule != null)
        {  // thanks Thora for pointing a NPE occurring here
            monitorSchedule.cancel(false);
            monitorSchedule = null;
        }

        map.killAllMonsters();
        map.clearDrops();
        if (portal != null)
        {
            portal.setPortalStatus(PortalConstants.OPEN);
        }
        map.resetReactors();

        portal = null;
    }
}
