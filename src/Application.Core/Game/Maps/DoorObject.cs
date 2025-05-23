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


using tools;

namespace Application.Core.Game.Maps;


/**
 * @author Ronan
 */
public class DoorObject : AbstractMapObject
{
    private int ownerId;
    private int pairOid;

    private IMap from;
    private IMap to;
    private int linkedPortalId;
    private Point linkedPos;

    private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    public DoorObject(int owner, IMap destination, IMap origin, int townPortalId, Point targetPosition, Point toPosition) : base()
    {
        setPosition(targetPosition);

        ownerId = owner;
        linkedPortalId = townPortalId;
        from = origin;
        to = destination;
        linkedPos = toPosition;
    }

    public void update(int townPortalId, Point toPosition)
    {
        lockObj.EnterWriteLock();
        try
        {
            linkedPortalId = townPortalId;
            linkedPos = toPosition;
        }
        finally
        {
            lockObj.ExitWriteLock();
        }
    }

    private int getLinkedPortalId()
    {
        lockObj.EnterReadLock();
        try
        {
            return linkedPortalId;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    private Point getLinkedPortalPosition()
    {
        lockObj.EnterReadLock();
        try
        {
            return linkedPos;
        }
        finally
        {
            lockObj.ExitReadLock();
        }
    }

    public void warp(IPlayer chr)
    {
        var party = chr.getParty();
        if (chr.getId() == ownerId || party != null && party.getMemberById(ownerId) != null)
        {
            chr.sendPacket(PacketCreator.playPortalSound());

            if (!inTown() && party == null)
            {
                chr.changeMap(to, getLinkedPortalId());
            }
            else
            {
                chr.changeMap(to, getLinkedPortalPosition());
            }
        }
        else
        {
            chr.sendPacket(PacketCreator.blockedMessage(6));
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
            if (chr.getParty() != null && (getOwnerId() == chr.getId() || chr.getParty()!.getMemberById(getOwnerId()) != null))
            {
                chr.sendPacket(PacketCreator.partyPortal(getFrom().getId(), getTo().getId(), toPosition()));
            }

            chr.sendPacket(PacketCreator.spawnPortal(getFrom().getId(), getTo().getId(), toPosition()));
            if (!inTown())
            {
                chr.sendPacket(PacketCreator.spawnDoor(getOwnerId(), getPosition(), launched));
            }
        }
    }

    public override void sendDestroyData(IChannelClient client)
    {
        var chr = client.OnlinedCharacter;
        if (from.getId() == chr.getMapId())
        {
            var party = chr.getParty();
            if (party != null && (ownerId == chr.getId() || party.getMemberById(ownerId) != null))
            {
                client.sendPacket(PacketCreator.partyPortal(MapId.NONE, MapId.NONE, new Point(-1, -1)));
            }
            client.sendPacket(PacketCreator.removeDoor(ownerId, inTown()));
        }
    }

    public void sendDestroyData(IChannelClient client, bool partyUpdate)
    {
        if (client != null && from.getId() == client.OnlinedCharacter.getMapId())
        {
            client.sendPacket(PacketCreator.partyPortal(MapId.NONE, MapId.NONE, new Point(-1, -1)));
            client.sendPacket(PacketCreator.removeDoor(ownerId, inTown()));
        }
    }

    public int getOwnerId()
    {
        return ownerId;
    }

    public void setPairOid(int oid)
    {
        pairOid = oid;
    }

    public int getPairOid()
    {
        return pairOid;
    }

    public bool inTown()
    {
        return getLinkedPortalId() == -1;
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
        return inTown() ? from : to;
    }

    public IMap getArea()
    {
        return !inTown() ? from : to;
    }

    public Point getAreaPosition()
    {
        return !inTown() ? getPosition() : getLinkedPortalPosition();
    }

    public Point toPosition()
    {
        return getLinkedPortalPosition();
    }

    public override MapObjectType getType()
    {
        return MapObjectType.DOOR;
    }
}
