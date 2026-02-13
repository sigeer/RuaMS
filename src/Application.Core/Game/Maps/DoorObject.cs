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
    public int OwnerId { get; }
    private IMap from;
    private IMap to;
    public bool InTown => TownPortal != null;
    public MysticDoorPortal? TownPortal { get; }
    public DoorObject LinkDoor { get; }
    public DoorObject(Player owner, Point currentPosition, IMap toMap, IMap fromMap, MysticDoorPortal townPortal) : base()
    {
        setPosition(currentPosition);

        OwnerId = owner.Id;
        from = fromMap;
        to = toMap;

        LinkDoor = new DoorObject(this, townPortal);
    }

    public DoorObject(DoorObject areaDoor, MysticDoorPortal townPortal) : base()
    {
        OwnerId = areaDoor.OwnerId;
        from = areaDoor.to;
        to = areaDoor.from;

        LinkDoor = areaDoor;
        TownPortal = townPortal;
        setPosition(TownPortal.getPosition());
        TownPortal.Door = this;
    }

    public void warp(Player chr)
    {
        var party = chr.getParty();
        if (chr.getId() == OwnerId || party != null && party.containsMembers(OwnerId))
        {
            chr.sendPacket(PacketCreator.playPortalSound());

            if (TownPortal != null)
            {
                chr.changeMap(to, TownPortal.getId());
            }
            else
            {
                chr.changeMap(to, LinkDoor.getPosition());
            }
        }
        else
        {
            chr.sendPacket(PacketCreator.BlockMapMessage(6));
            chr.sendPacket(PacketCreator.enableActions());
        }
    }

    public override void sendSpawnData(IChannelClient client)
    {
        sendSpawnData(client, true);
    }

    public void sendSpawnData(IChannelClient client, bool launched)
    {
        var chr = client.OnlinedCharacter;
        if (getFrom().getId() == chr.getMapId())
        {
            var party = chr.getParty();
            if (party != null && (OwnerId == chr.getId() || party.containsMembers(OwnerId)))
            {
                chr.sendPacket(TeamPacketCreator.partyPortal(getFrom().getId(), getTo().getId(), toPosition()));
            }

            chr.sendPacket(PacketCreator.spawnPortal(getFrom().getId(), getTo().getId(), toPosition()));
            if (!InTown)
            {
                chr.sendPacket(PacketCreator.spawnDoor(getObjectId(), getPosition(), launched));
            }
        }

        if (TownPortal != null)
        {
            TownPortal.Door = this;
        }
    }

    public override void sendDestroyData(IChannelClient client)
    {
        var chr = client.OnlinedCharacter;
        if (from.getId() == chr.getMapId())
        {
            var party = chr.getParty();
            if (party != null && (OwnerId == chr.getId() || party.containsMembers(OwnerId)))
            {
                client.sendPacket(TeamPacketCreator.partyPortal(MapId.NONE, MapId.NONE, new Point(-1, -1)));
            }
            client.sendPacket(PacketCreator.removeDoor(getObjectId(), InTown));
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

    protected override bool IsPlayerVisiable(Player chr)
    {
        return chr.getMapId() == getFrom().getId();
    }
}
