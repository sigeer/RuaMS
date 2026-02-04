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


using Application.Core.Channel.Commands;
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
    private IMap target;
    private KeyValuePair<string, int>? posStatus = null;
    private long deployTime;
    private bool active;

    private DoorObject? townDoor;
    private DoorObject? areaDoor;

    Door(Player owner, Point targetPosition, MysticDoorPortal townDoorPortal)
    {
        this.ownerId = owner.getId();
        this.target = owner.getMap();
        this.town = this.target.getReturnMap();
        this.deployTime = target.ChannelServer.Node.getCurrentTime();
        this.active = true;
        this.areaDoor = new DoorObject(owner, targetPosition, town, target, townDoorPortal);
        this.townDoor = areaDoor.LinkDoor;
    }

    public static int TryCreateDoor(Player chr, Point targetPosition, out Door? door)
    {
        door = null;

        var toMap = chr.MapModel.getReturnMap();
        if (!chr.MapModel.canDeployDoor(targetPosition))
        {
            return -2;
        }

        if (!toMap.TryGetEffectiveDoorPortal(out var townPortal) || townPortal == null)
        {
            return -1;
        }

        door = new Door(chr, targetPosition, townPortal);
        return 0;
    }

    public void Destroy()
    {
        DoorObject areaDoor = this.getAreaDoor();
        DoorObject townDoor = this.getTownDoor();

        IMap target = this.getTarget();
        IMap town = this.getTown();

        var targetChars = target.getAllPlayers();
        var townChars = town.getAllPlayers();

        target.removeMapObject(areaDoor);
        town.removeMapObject(townDoor);

        foreach (Player chr in targetChars)
        {
            areaDoor.sendDestroyData(chr.getClient());
            chr.removeVisibleMapObject(areaDoor);
        }

        foreach (Player chr in townChars)
        {
            townDoor.sendDestroyData(chr.getClient());
            chr.removeVisibleMapObject(townDoor);
        }

        var owner = target.ChannelServer.getPlayerStorage().getCharacterById(ownerId);
        if (owner != null)
        {
            owner.silentPartyUpdate();
        }


        //// 坐标传送都是0x80，时空门的第1个portal也是0x80，前面移除了这个portal，这里重新生成？--portalFactory让时空门的portalid从0x80+1开始
        //if (this.getTownPortal().getId() == 0x80)
        //{
        //    foreach (Player chr in townChars)
        //    {
        //        var door = chr.getMainTownDoor();
        //        if (door != null)
        //        {
        //            townDoor.sendSpawnData(chr.getClient());
        //            chr.addVisibleMapObject(townDoor);
        //        }
        //    }
        //}
    }

    public static void attemptRemoveDoor(Player owner)
    {
        var destroyDoor = owner.getPlayerDoor();
        if (destroyDoor != null)
        {
            long effectTimeLeft = 3000 - destroyDoor.getElapsedDeployTime();   // portal deployment effect duration
            if (effectTimeLeft > 0)
            {
                IMap town = destroyDoor.getTown();
                OverallService service = owner.Client.getChannelServer().OverallService;
                service.registerOverallAction(owner.getMapId(), new RequestRemoveDoorCommand(owner.Id), effectTimeLeft);
            }
            else
            {
                owner.Client.CurrentServer.Post(new InvokeRemoveDoorCommand(owner.Id));
            }
        }
        else
        {
            owner.Client.CurrentServer.Post(new RequestRemoveDoorCommand(owner.Id));
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
        return target.ChannelServer.Node.getCurrentTime() - deployTime;
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
