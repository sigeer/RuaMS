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

namespace server.maps;



public interface Portal
{
    int getType();
    int getId();
    Point getPosition();
    string getName();
    string getTarget();
    string getScriptName();
    void setScriptName(string newName);
    void setPortalStatus(bool newStatus);
    bool getPortalStatus();
    int getTargetMapId();
    void enterPortal(IClient c);
    void setPortalState(bool state);
    bool getPortalState();
}


public class PortalConstants
{
    public const int TELEPORT_PORTAL = 1;
    public const int MAP_PORTAL = 2;
    public const int DOOR_PORTAL = 6;
    public static bool OPEN = true;
    public static bool CLOSED = false;
}