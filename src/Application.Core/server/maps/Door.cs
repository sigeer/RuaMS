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


using Application.Core.Channel;
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
    WorldChannel _channelServer;
    public int ownerId;


    private KeyValuePair<string, int>? posStatus = null;
    private long deployTime;
    private bool active;

    private DoorObject townDoor = null!;
    private DoorObject areaDoor = null!;

    Door(Player owner)
    {
        _channelServer = owner.Client.CurrentServer;
        this.ownerId = owner.getId();

        this.deployTime = _channelServer.Node.getCurrentTime();
        this.active = true;
    }

    public static async Task<(int State, Door? Door)> TryCreateDoor(Player chr, Point areaPos)
    {
        var townMap = await chr.MapModel.getReturnMap();
        if (!CanDeployDoor(chr.MapModel, areaPos))
        {
            return (-2, null);
        }

        if (!townMap.TryGetEffectiveDoorPortal(out var townPortal))
        {
            return (-1, null);
        }

        var door = new Door(chr);
        await door.CreateDoorObject(chr.MapModel, areaPos, townMap, townPortal);
        return (0, door);
    }

    static bool CanDeployDoor(IMap map, Point pos)
    {
        Point? toStep = map.getPointBelow(pos);
        return toStep != null && toStep.Value.distance(pos) <= 42;
    }

    public async Task CreateDoorObject(IMap areaMap, Point areaPos, IMap townMap, MysticDoorPortal townDoorPortal)
    {
        this.areaDoor = new DoorObject(this, areaMap, areaPos, townMap);
        this.townDoor = new DoorObject(this, townMap, townDoorPortal, areaMap);
    }

    public async Task Destroy()
    {
        DoorObject areaDoor = this.getAreaDoor();
        DoorObject townDoor = this.getTownDoor();

        IMap target = this.getTarget();
        IMap town = this.getTown();

        await target.RemoveMapObject(areaDoor, chr => areaDoor.sendDestroyData(chr.Client));
        await town.RemoveMapObject(townDoor, chr => townDoor.sendDestroyData(chr.Client));

        var ownerActor = target.ChannelServer.getPlayerStorage().GetCharacterActor(ownerId);
        if (ownerActor != null)
        {
            await ownerActor.Send(m =>
            {
                m.getCharacterById(ownerId)?.silentPartyUpdate();
            });
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
                owner.Client.CurrentServer.Send(new InvokeRemoveDoorCommand(owner.Id));
            }
        }
        else
        {
            owner.Client.CurrentServer.Send(new RequestRemoveDoorCommand(owner.Id));
        }
    }

    private Portal? getTownDoorPortal(int doorid)
    {
        return townDoor.getTown().getDoorPortal(doorid);
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
        return townDoor.getTown();
    }

    public IMap getTarget()
    {
        return townDoor.getTo();
    }

    public KeyValuePair<string, int>? getDoorStatus()
    {
        return posStatus;
    }

    public long getElapsedDeployTime()
    {
        return _channelServer.Node.getCurrentTime() - deployTime;
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
