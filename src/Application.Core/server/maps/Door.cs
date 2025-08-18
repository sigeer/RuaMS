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
using net.server.services.task.channel;

namespace server.maps;


/**
 * @author Matze
 * @author Ronan
 */
public class Door
{
    private int ownerId;
    private IMap? town;
    private Portal? townPortal;
    private IMap target;
    private KeyValuePair<string, int>? posStatus = null;
    private long deployTime;
    private bool active;

    private DoorObject? townDoor;
    private DoorObject? areaDoor;

    public Door(IPlayer owner, Point targetPosition)
    {
        this.ownerId = owner.getId();
        this.target = owner.getMap();

        if (target.canDeployDoor(targetPosition))
        {
            if (YamlConfig.config.server.USE_ENFORCE_MDOOR_POSITION)
            {
                posStatus = target.getDoorPositionStatus(targetPosition);
            }

            if (posStatus == null)
            {
                this.town = this.target.getReturnMap();
                this.townPortal = getTownDoorPortal(owner.getDoorSlot());
                this.deployTime = target.ChannelServer.Container.getCurrentTime();
                this.active = true;

                if (townPortal != null)
                {
                    this.areaDoor = new DoorObject(owner, town, target, townPortal.getId(), targetPosition, townPortal.getPosition());
                    this.townDoor = new DoorObject(owner, target, town, -1, townPortal.getPosition(), targetPosition);

                    this.areaDoor.setPairOid(this.townDoor.getObjectId());
                    this.townDoor.setPairOid(this.areaDoor.getObjectId());
                }
                else
                {
                    this.ownerId = -1;
                }
            }
            else
            {
                this.ownerId = -3;
            }
        }
        else
        {
            this.ownerId = -2;
        }
    }

    public void updateDoorPortal(IPlayer owner)
    {
        int slot = owner.fetchDoorSlot();

        var nextTownPortal = getTownDoorPortal(slot);
        if (nextTownPortal != null)
        {
            townPortal = nextTownPortal;
            areaDoor.update(nextTownPortal.getId(), nextTownPortal.getPosition());
        }
    }

    private void broadcastRemoveDoor(IPlayer owner)
    {
        DoorObject areaDoor = this.getAreaDoor();
        DoorObject townDoor = this.getTownDoor();

        IMap target = this.getTarget();
        IMap town = this.getTown();

        var targetChars = target.getCharacters();
        var townChars = town.getCharacters();

        target.removeMapObject(areaDoor);
        town.removeMapObject(townDoor);

        foreach (IPlayer chr in targetChars)
        {
            areaDoor.sendDestroyData(chr.getClient());
            chr.removeVisibleMapObject(areaDoor);
        }

        foreach (IPlayer chr in townChars)
        {
            townDoor.sendDestroyData(chr.getClient());
            chr.removeVisibleMapObject(townDoor);
        }

        owner.removePartyDoor(false);

        if (this.getTownPortal().getId() == 0x80)
        {
            foreach (IPlayer chr in townChars)
            {
                var door = chr.getMainTownDoor();
                if (door != null)
                {
                    townDoor.sendSpawnData(chr.getClient());
                    chr.addVisibleMapObject(townDoor);
                }
            }
        }
    }

    public static void attemptRemoveDoor(IPlayer owner)
    {
        var destroyDoor = owner.getPlayerDoor();
        if (destroyDoor != null && destroyDoor.dispose())
        {
            long effectTimeLeft = 3000 - destroyDoor.getElapsedDeployTime();   // portal deployment effect duration
            if (effectTimeLeft > 0)
            {
                IMap town = destroyDoor.getTown();

                OverallService service = town.getChannelServer().OverallService;
                service.registerOverallAction(town.getId(), () =>
                {
                    destroyDoor.broadcastRemoveDoor(owner);   // thanks BHB88 for noticing doors crashing players when instantly cancelling buff
                }, effectTimeLeft);
            }
            else
            {
                destroyDoor.broadcastRemoveDoor(owner);
            }
        }
    }

    private Portal? getTownDoorPortal(int doorid)
    {
        return town?.getDoorPortal(doorid);
    }

    public int getOwnerId()
    {
        return ownerId;
    }

    public DoorObject getTownDoor()
    {
        return townDoor;
    }

    public DoorObject getAreaDoor()
    {
        return areaDoor;
    }

    public IMap getTown()
    {
        return town;
    }

    public Portal getTownPortal()
    {
        return townPortal;
    }

    public IMap getTarget()
    {
        return target;
    }

    public KeyValuePair<string, int>? getDoorStatus()
    {
        return posStatus;
    }

    public long getElapsedDeployTime()
    {
        return target.ChannelServer.Container.getCurrentTime() - deployTime;
    }

    private bool dispose()
    {
        if (active)
        {
            active = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool isActive()
    {
        return active;
    }
}
