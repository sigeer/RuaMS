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


using constants.game;
using constants.id;
using scripting.portal;
using tools;

namespace server.maps;


public class GenericPortal : Portal
{
    private string name;
    private string target;
    private Point position;
    private int targetmap;
    private int type;
    private bool status = true;
    private int id;
    private string? scriptName;
    private bool portalState;
    private object? scriptLock = null;

    public GenericPortal(int type)
    {
        this.type = type;
    }

    public int getId()
    {
        return id;
    }

    public void setId(int id)
    {
        this.id = id;
    }

    public string getName()
    {
        return name;
    }

    public Point getPosition()
    {
        return position;
    }

    public string getTarget()
    {
        return target;
    }

    public void setPortalStatus(bool newStatus)
    {
        this.status = newStatus;
    }

    public bool getPortalStatus()
    {
        return status;
    }

    public int getTargetMapId()
    {
        return targetmap;
    }

    public int getType()
    {
        return type;
    }

    public string? getScriptName()
    {
        return scriptName;
    }

    public void setName(string name)
    {
        this.name = name;
    }

    public void setPosition(Point position)
    {
        this.position = position;
    }

    public void setTarget(string target)
    {
        this.target = target;
    }

    public void setTargetMapId(int targetmapid)
    {
        this.targetmap = targetmapid;
    }

    public void setScriptName(string? scriptName)
    {
        this.scriptName = scriptName;

        if (scriptName != null)
        {
            if (scriptLock == null)
            {
                scriptLock = new object();
            }
        }
        else
        {
            scriptLock = null;
        }
    }

    public void enterPortal(IChannelClient c)
    {
        bool changed = false;
        if (getScriptName() != null)
        {
            try
            {
                Monitor.Enter(scriptLock!);
                try
                {
                    changed = c.CurrentServer.PortalScriptManager.executePortalScript(this, c);
                }
                finally
                {
                    Monitor.Exit(scriptLock!);
                }
            }
            catch (NullReferenceException npe)
            {
                Log.Logger.Error(npe.ToString());
            }
        }
        else if (getTargetMapId() != MapId.NONE)
        {
            var chr = c.OnlinedCharacter;
            if (!(chr.getChalkboard() != null && GameConstants.isFreeMarketRoom(getTargetMapId())))
            {
                var to = chr.getEventInstance() == null ? c.CurrentServer.getMapFactory().getMap(getTargetMapId()) : chr.getEventInstance()!.getMapInstance(getTargetMapId());
                var pto = to.getPortal(getTarget());
                if (pto == null)
                {// fallback for missing portals - no real life case anymore - interesting for not implemented areas
                    pto = to.getPortal(0);
                }
                chr.changeMap(to, pto); //late resolving makes this harder but prevents us from loading the whole world at once
                changed = true;
            }
            else
            {
                chr.dropMessage(5, "You cannot enter this map with the chalkboard opened.");
            }
        }
        if (!changed)
        {
            c.sendPacket(PacketCreator.enableActions());
        }
    }

    public void setPortalState(bool state)
    {
        this.portalState = state;
    }

    public bool getPortalState()
    {
        return portalState;
    }
}
