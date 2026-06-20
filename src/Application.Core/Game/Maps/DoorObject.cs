/*
    This file is part of the HeavenMS MapleStory Server
    Copyleft (L) 2016 - 2019 RonanLana

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


using Application.Core.Channel.Net.Packets;
using server.maps;
using tools;

namespace Application.Core.Game.Maps;


/**
 * @author Ronan
 */
public class DoorObject : AbstractMapObject
{
    public Door Door { get; }
    public int OwnerId => Door.ownerId;
    private IMap from;
    private IMap to;
    public bool InTown => TownPortal != null;
    public MysticDoorPortal? TownPortal { get; }
    /// <summary>
    /// 门的另一边
    /// </summary>
    public DoorObject LinkDoor => InTown ? Door.getAreaDoor() : Door.getTownDoor();

    /// <summary>
    /// Town
    /// </summary>
    /// <param name="door"></param>
    /// <param name="townMap"></param>
    /// <param name="townPortal"></param>
    /// <param name="areaMap"></param>
    public DoorObject(Door door, IMap townMap, MysticDoorPortal townPortal, IMap areaMap) : base(townMap, townPortal.getPosition())
    {
        Door = door;
        from = townMap;
        to = areaMap;

        TownPortal = townPortal;
    }

    /// <summary>
    /// Area
    /// </summary>
    /// <param name="door"></param>
    /// <param name="areaMap"></param>
    /// <param name="pos"></param>
    /// <param name="townMap"></param>
    public DoorObject(Door door, IMap areaMap, Point pos, IMap townMap) : base(areaMap, pos)
    {
        Door = door;
        from = areaMap;
        to = townMap;
    }

    public async Task warp(Player chr)
    {
        var party = chr.getParty();
        if (chr.getId() == OwnerId || party != null && party.containsMembers(OwnerId))
        {
            await chr.SendPacket(PacketCreator.playPortalSound());

            if (TownPortal != null)
            {
                await chr.changeMap(to, TownPortal.getId());
            }
            else
            {
                await chr.changeMap(to, LinkDoor.getPosition());
            }
        }
        else
        {
            await chr.SendPacket(PacketCreator.BlockMapMessage(6));
            await chr.SendPacket(PacketCreator.enableActions());
        }
    }

    public override async Task sendSpawnData(IChannelClient client)
    {
        await sendSpawnData(client, true);
    }

    public async Task sendSpawnData(IChannelClient client, bool launched)
    {
        var chr = client.OnlinedCharacter;
        if (getFrom().getId() == chr.getMapId())
        {
            var party = chr.getParty();
            if (party != null && (OwnerId == chr.getId() || party.containsMembers(OwnerId)))
            {
                await chr.SendPacket(TeamPacketCreator.partyPortal(getFrom().getId(), getTo().getId(), toPosition()));
            }

            await chr.SendPacket(PacketCreator.spawnPortal(getFrom().getId(), getTo().getId(), toPosition()));
            if (!InTown)
            {
                await chr.SendPacket(PacketCreator.spawnDoor(getObjectId(), getPosition(), launched));
            }
        }

        if (TownPortal != null)
        {
            TownPortal.Door = this;
        }
    }

    public override async Task sendDestroyData(IChannelClient client)
    {
        var chr = client.OnlinedCharacter;
        if (from.getId() == chr.getMapId())
        {
            var party = chr.getParty();
            if (party != null && (OwnerId == chr.getId() || party.containsMembers(OwnerId)))
            {
                await client.SendPacket(TeamPacketCreator.partyPortal(MapId.NONE, MapId.NONE, new Point(-1, -1)));
            }
            await client.SendPacket(PacketCreator.removeDoor(getObjectId(), InTown));
        }

        if (TownPortal != null)
        {
            TownPortal.Door = null;
        }
    }

    public IMap getFrom()
    {
        return from;
    }

    public IMap getTo()
    {
        return to;
    }

    public IMap getTown()
    {
        return InTown ? from : to;
    }

    public IMap getArea()
    {
        return !InTown ? from : to;
    }

    public Point toPosition()
    {
        return LinkDoor.getPosition();
    }

    public override MapObjectType getType()
    {
        return MapObjectType.DOOR;
    }
}
